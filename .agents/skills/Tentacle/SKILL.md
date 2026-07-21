---
name: Tentacle
description: 보스 몬스터의 촉수(Tentacle)와 관련된 새로운 기능이나 패턴(State)을 추가할 때 참고하는 가이드 및 규칙입니다.
---

# Tentacle (촉수) 시스템 개발 가이드

이 문서는 `TentacleController` 기반의 촉수 시스템 구조를 이해하고, 새로운 기능(패턴, 상태 등)을 추가할 때 지켜야 할 규칙과 개발 프로세스를 안내합니다.

## 1. 아키텍처 개요

촉수 시스템은 크게 다음 세 가지 핵심 요소로 구성됩니다:
1. **`TentacleController.cs`**: 촉수의 물리/IK(Inverse Kinematics) 렌더링, 위치 업데이트, 상태 머신 업데이트 및 콜라이더 관리를 담당하는 핵심 스크립트. `FABRIK` 알고리즘을 사용해 관절(Segment)들을 자연스럽게 움직입니다.
2. **`IMonsterState` 기반 상태 클래스**: `TentacleIdle`, `TentacleStretch`, `TentacleAttack` 등 각각의 특정 행동(대기, 뻗기, 공격 등)을 정의하는 클래스들.
3. **`MonsterAiBrain.cs`**: 촉수의 상태(State)들과 각 상태 간의 전환(Transition) 조건을 정의하는 팩토리(Factory) 클래스. `MakeMachine("BossTentacle", ...)` 메서드에서 FSM을 구성합니다.

## 2. 주요 제어 변수 (`TentacleController`)

새로운 상태를 만들 때 `TentacleController`의 다음 프로퍼티들을 주로 조작하게 됩니다:

- **`IkTargetPosition` (Vector2)**: 촉수 끝단(Grabber)이 향해야 할 목표 월드 좌표입니다. 이 값을 변경하면 IK가 자동으로 촉수 관절을 목표 위치로 부드럽게 이동시킵니다.
- **`isTrap` (bool)**: 촉수가 보스 몸체에 붙어있는지, 아니면 독립적인 함정(Trap) 형태로 스폰되었는지 구분합니다.
- **`Target` (GameObject)**: 촉수가 쫓거나 공격할 대상(주로 Player)입니다.
- **상태 전이 플래그 (bool)**: 
  - `IsSearch`: 대상을 탐지했는지 여부. (주로 Idle -> Stretch 전환에 사용)
  - `IsAttach`: 대상에게 달라붙었거나 특정 위치에 도달했는지 여부.
  - `Attack`: 실제 공격을 수행할지 여부.
  - `IsAttackTentacle`: 공격용 촉수인지 여부.
  - `IsReturn`: 복귀 상태인지 여부.

## 3. 새로운 기능(State) 추가 프로세스

새로운 촉수 행동(예: 땅에서 솟구쳐 오르는 찌르기, 휘두르기 등)을 추가할 때는 다음 단계를 따릅니다.

### Step 1: 새로운 State 클래스 생성
- `Assets/Project/Scripts/Monster/BossMonster/Interface/Tentacle/` 하위에 새로운 클래스(예: `TentacleSweep.cs`)를 생성합니다.
- `IMonsterState` 인터페이스를 구현합니다.

```csharp
using UnityEngine;

public class TentacleSweep : IMonsterState
{
    private TentacleController _owner;

    public TentacleSweep(TentacleController owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        // 상태 진입 시 초기화 작업
        // 예: 플래그 초기화, 특정 레이어 설정 등
    }

    public void Update()
    {
        // 매 프레임 동작 로직
        // 1. _owner.IkTargetPosition 을 변경하여 촉수를 움직입니다.
        // 2. 조건 만족 시 상태 전이를 위한 플래그(예: _owner.Attack = true)를 변경합니다.
    }

    public void Exit()
    {
        // 상태 종료 시 정리 작업
    }
}
```

### Step 2: FSM(상태 머신)에 State 등록 및 Transition(전이) 설정
- `MonsterAiBrain.cs` 파일의 `MakeMachine(string name, TentacleController owner)` 메서드를 수정합니다.
- 생성한 새로운 State를 인스턴스화합니다.
- `transitionMap`을 이용해 어떤 조건에서 새로운 상태로 진입하고, 다시 어떤 상태로 빠져나갈지 정의합니다.

```csharp
// MonsterAiBrain.cs 내부 예시
IMonsterState sweep = new TentacleSweep(owner);

// 기존 상태에서 Sweep으로 넘어가는 Transition 추가
transitionMap[idle].Add(
    new Transition(
        condition: () => { return owner.SomeSweepCondition; },
        targetState: sweep
    )
);

// Sweep 상태에서 다른 상태로 넘어가는 Transition 정의
transitionMap[sweep] = new List<Transition>
{
    new Transition(
        condition: () => { return !owner.SomeSweepCondition; },
        targetState: idle
    )
};
```

### Step 3: 물리/충돌 및 시각 효과 (필요 시)
- 타격 시점이 되면 `TentacleController.cs` 내부의 메서드나 상태 클래스의 `Update` 로직 안에서 이펙트를 켜거나(예: `SlashAnimation()`), 물리적 판정을 처리할 수 있습니다.
- 촉수의 물리 몸체는 LineRenderer 좌표를 기반으로 `UpdateColliders()` 메서드를 통해 `EdgeCollider2D`로 동기화되므로, `IkTargetPosition`을 움직이는 것만으로도 몸통 물리 판정이 자동으로 이동합니다.

## 4. 주의사항

- **직접 트랜스폼 조작 금지**: 촉수 관절(`_segmentPos`)들을 직접 수정하지 마십시오. 모든 움직임은 `IkTargetPosition`을 조작하여 `FABRIK` 알고리즘이 자연스럽게 계산하도록 해야 합니다.
- **단일 책임 유지**: 한 State 클래스는 한 가지 행동 턴(예: 찌르기, 잡기, 휩쓸기)만 수행하도록 설계하십시오. 복잡한 패턴은 여러 State를 조합(Transition)하여 구현합니다.
- **예외 처리**: 대상(`Target`)이 중간에 사라지거나 죽는 경우(`null`)를 대비하여 `Update()` 루프 내에 방어 코드를 작성하십시오.
- **생성 및 반환**: 촉수를 생성할때와 반환할때 'ObjectPoolManager.cs' 을 이용하십시오.

- **파일 수정**: 모든 논리 코드를 다 작성후 파일 수정하기 위해 허락을 받을것.

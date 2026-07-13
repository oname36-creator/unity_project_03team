# Unity 2D Project: unity_project_03team 코딩 에이전트 개발 규칙 및 지침

본 문서는 `unity_project_03team` 프로젝트 내에서 작업을 수행하는 모든 AI 코딩 에이전트(Antigravity 등)가 준수해야 할 스타일 가이드라인, 설계 원칙, 그리고 개발 환경 규칙을 정의합니다.

---

## 1. 프로젝트 아키텍처 개요

이 프로젝트는 Unity 엔진 기반의 2D 횡스크롤/액션 플랫포머 게임입니다.
  - 총 3개의 페이즈로 각각 1분씩 플레이한다.
    - 1페이즈 : 왼쪽에서 오른쪽으로 움직임.
    - 2페이즈 : 아래에서 위로 움직임
    - 3페이즈 : 5초정도  왼쪽에서 오른쪽으로 움직인 후, 위에서 아래로 움직임.
   상자 : 일반 타일 중에 랜덤으로 스폰
   단, 함정이나 점프대, 풀숲(발판)에서는 스폰 되지 않음.
  - 맵의 배경은 2.5D와 같이 생동감(원근감 부여) 있게 구현한다.
   - 구현 방식 : 카메라의 Orthographic을 사용하고, z축은 0으로 고정된 상태에서 스크립트로 구현.
   - 배경 원근감을 넣을 때는 항상 Parallax.cs의 기법들을 읽어온다.
코드 베이스는 크게 다음과 같은 레이어로 나뉩니다.
- **Core**: 글로벌 데이터 관리 및 유틸리티 매니저 클래스 (`DataManager.cs`, `Singleton.cs`)
- **Player**: 플레이어 입력 처리, 물리 운동, 상태 값(HP, 디버프 등) 및 아이템 효과 적용 (`PlayerControll.cs`, `PlayerStatus.cs`, `ItemEffectApplicator.cs`)
- **Monster**: FSM(유한 상태 머신) 기반의 AI 시스템 (`MonsterStateMachine.cs`, `MonsterAiBrain.cs`, `MonsterController.cs`, `IMonsterState.cs`)
- **UI/Map**: 맵 스폰 트리거 관리, 셔플백 무작위 생성 및 오브젝트 풀링 기반의 맵 배치 (`MapManager.cs`, `MapChunk.cs`)

---

## 2. 코드 스타일 및 명명 규칙 (Naming Conventions)

- **클래스 및 메서드**: `PascalCase` 명명 규칙을 적용합니다. (예: `PlayerControll`, `CheckGrounded()`)
- **변수 및 필드**:
  - `public` 또는 `[SerializeField]` 필드: `camelCase` 또는 `PascalCase` (인스펙터 노출 시 툴팁/헤더 제공 권장).
  - `private` 필드: `_camelCase` (언더바 접두사 사용 권장, 예: `_hp`, `_directionTimer`).
- **Unity 이벤트/콜백**: Input System 콜백 함수는 `On[ActionName]` 패턴을 사용합니다. (예: `OnMove()`, `OnJump()`)
- **스크립트 주석**: 한글 주석을 기본으로 하며, 코드 변경 시 변경 이력이나 중요한 설계 의도(예: `[교정]`, `[유지]`)를 명확하게 남깁니다. 포맷은 'UTF-8'로 하십시오.

---

## 3. 핵심 설계 원칙 및 구현 패턴

### 3.1 단일 책임 원칙 (Single Responsibility Principle)
- 하나의 컴포넌트는 단 하나의 주 기능만 담당해야 합니다.
  - 플레이어의 상태 관리는 `PlayerStatus.cs`에서 전담합니다.
  - 물리 조작 및 입력 인터페이스는 `PlayerControll.cs`에서 처리합니다.
  - 아이템 사용 및 버프/디버프 상태 적용은 `ItemEffectApplicator.cs`에서 관리합니다.

### 3.2 FSM (유한 상태 머신) 및 AI 규칙
- 몬스터 AI 구현 시 `IMonsterState` 인터페이스를 구현하고 `MonsterStateMachine`을 통해 상태 전환을 제어합니다.
- 새로운 상태(State)나 전환 규칙을 설계할 때는 `MonsterAiBrain.cs` 내의 팩토리 메서드 `MakeMachine`에서 람다식 형태의 `Transition` 조건식과 상태 맵을 등록하여 구현합니다.

### 3.3 오브젝트 풀링 (Object Pooling)
- 맵 덩어리(`MapChunk`) 및 몬스터 등 자주 생성/파괴되는 리소스는 `MapManager`의 `InitializationPools()`와 `GetOrCreateMap()`처럼 오브젝트 풀을 활용하여 가비지 컬렉션(GC) 부하를 최소화해야 합니다.

### 3.4 2.5D 배경 원근감 및 Parallax 규칙
- **스크립트 기반 원근감**: 카메라는 Orthographic 모드를 사용하며, z축은 0으로 고정합니다. 카메라 이동량(`camMoveDistance`)에 비례해 배경 레이어가 다르게 움직이도록 `Parallax.cs`를 활용합니다.
- **페이즈별 Y축 적용**: 1페이즈(CurrentPhaseIndex == 0)에서는 y축 카메라 흔들림에 따른 배경 어색함을 방지하기 위해 Y축 Parallax를 완전히 비활성화하며, 2페이즈(CurrentPhaseIndex >= 1) 이상일 때만 활성화합니다.
- **무한 루핑 보정**: 루핑 배경은 `SpriteRenderer` 크기를 pixelsPerUnit 단위로 변환해 텍스처 실제 물리 크기를 계산하고, 카메라 이동 오프셋에 맞춰 기준 시작점(`startPos`)을 프레임별로 보정합니다.
- **대기 원근 효과**: 원근 깊이에 따른 안개/대기 효과를 위해 `applyAtmosphereTint`를 활성화하고, `atmosphereColor`와 `tintStrength`를 이용해 스프라이트 원래 색상을 `Color.Lerp`하여 보정합니다.

### 3.5 이동 플랫폼(Moving Platform) 및 관성 연동 규칙
- **로컬 좌표계 기반 왕복**: 플랫폼 스폰 시의 오작동 및 맵 덩어리와의 연동 딜레이를 방지하기 위해 이동 타겟(시작점/끝점)은 월드 좌표가 아닌 부모 기준의 로컬 좌표(`transform.localPosition`)를 바탕으로 정의합니다.
- **FixedUpdate 물리 이동**: 플랫폼의 실제 물리적 이동은 로컬 타겟 좌표를 부모 기준으로 월드 좌표로 변환(`TransformPoint`)한 뒤, `FixedUpdate` 안에서 `Rigidbody2D.MovePosition`을 사용하여 덜컹거림이 없도록 부드럽게 구현합니다.
- **플레이어 관성 결합**: 플레이어가 플랫폼 위에 서 있을 때(`OnCollisionEnter2D`), 플랫폼의 `Velocity` 정보를 플레이어(`PlayerControll`)에 주입하며, 플레이어는 이동 처리 시 자신의 x 속도 및 플랫폼 속도를 합산해 `linearVelocity`를 대입합니다.
- **Gizmos 가시화**: 개발 중 디버깅의 편의를 위해 `OnDrawGizmos`를 오버라이드하여 플랫폼의 로컬 시작점과 끝점 경로, 크기를 씬 뷰에 시각적(초록색 라인 및 큐브)으로 상시 노출시킵니다.

### 3.6 플레이어 전투 판정 및 피격 예외 처리
- **히트박스 대칭 이동**: 플레이어가 바라보는 방향(`facingDirectionX`)에 맞추어 맨손(`attackHitboxObj`) 및 검(`swordAttackHitboxObj`) 히트박스의 로컬 x 좌표 부호를 반전(대칭 이동)시켜 앞뒤 판정이 알맞게 적용되도록 합니다.
- **피격 시 충돌 버그 방지**: 공격 중(코루틴 처리 시) 또는 피격 직후 무적 상태 동안 플레이어와 몬스터 레이어의 물리 충돌을 `IgnoreLayerCollision`으로 일시 격리하여, 다중 충돌로 인한 억울한 체력 차감을 방지합니다.
- **물리 겹침 튕겨내기**: 넉백 및 무적 발생 순간, 몬스터와 캐릭터의 Collider가 비벼져 다중 피격 판정이 일어나는 것을 방지하고자 피격 당하는 즉시 플레이어 `Collider2D`를 1프레임 동안 완전히 비활성화했다가 안전하게 재활성화합니다.

### 3.7 논리적 페이즈(Logical Phase) 관리 규칙
- **세부 페이즈 분리**: 인스펙터에 등록된 맵 스폰 세부 단계(`currentPhaseIndex` 등)와 게임 핵심 메커니즘을 제어하는 논리적 단계(`currentLogicalPhase`)를 명확히 구분합니다.
- **누적 시간 기반 매핑**: 세부 페이즈가 시작될 때마다 이전 세부 페이즈들의 누적 진행 시간(`cumulativeTime`)을 합산 계산하여 상위 기획 페이즈(0~60초 = 0페이즈, 60~120초 = 1페이즈, 120초 이상 = 2페이즈)를 실시간으로 결정합니다.
- **카메라 및 연출 제어**: 카메라 Y축 추적 모드 변경, Parallax Y축 비활성화 등 기획적 핵심 페이즈별 분기가 필요한 제어 로직은 세부 리스트 인덱스가 아닌, 이 누적 시간으로 계산된 `CurrentLogicalPhase`를 기준으로 동작하도록 설계합니다.

---

## 4. 파일 생성 및 작업 시 주의사항

1. **기존 주석 및 구조 보존**: 코드 리팩토링이나 수정 작업 시 코드 내에 있는 기성 주석(예: 개발 주석 및 수정 태그)을 무단으로 삭제하지 마십시오.
2. **에러 감지 및 로그 작성**: 예외 처리를 철저히 하고, 에러가 발생 가능한 지점에는 `Debug.LogError`를 사용해 의미 있는 메시지를 출력해야 합니다.
3. **Unity Physics2D 연동**: 이동 및 물리 제어 시 `Rigidbody2D`의 `linearVelocity` 속성을 적절히 변경하되, `FixedUpdate` 타이밍에서만 물리 연산을 처리하도록 설계합니다.

## 5. 코드 수정 시 주의사항

1. **수정 전 계획세우기** : 항상 코드를 수정할 때 바로 수정하는 것이 아닌 계획을 수립하고, 검증 후 수정하십시오.

---

## 6. 사용자 학습 중심 안내 규칙 (Learning-Oriented Guidance)

- **스스로 코딩하도록 가이드**: 사용자가 직접 코드를 작성하고 유니티 에디터를 조작하여 학습할 수 있도록 돕습니다. AI 에이전트는 파일을 직접 수정하기보다는, 다음 내용을 상세하게 안내해야 합니다:
  - 변경이 필요한 스크립트와 구체적인 위치
  - 직접 추가하거나 수정해야 할 C# 코드 조각 (구체적인 주석 포함)
  - 유니티 에디터 인스펙터 및 컴포넌트 세팅 단계별 가이드
- **예외 상황**: 사용자가 "알아서 코드를 수정해 줘"라고 명시적으로 요청하거나, 오류가 발생해 자동 디버깅을 요청하는 경우에만 직접 파일을 수정할 수 있습니다.


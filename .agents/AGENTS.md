# Unity 2D Project: unity_project_03team 코딩 에이전트 개발 규칙 및 지침

본 문서는 `unity_project_03team` 프로젝트 내에서 작업을 수행하는 모든 AI 코딩 에이전트(Antigravity 등)가 준수해야 할 스타일 가이드라인, 설계 원칙, 그리고 개발 환경 규칙을 정의합니다.

---

## 1. 프로젝트 아키텍처 개요

이 프로젝트는 Unity 엔진 기반의 2D 횡스크롤/액션 플랫포머 게임입니다.
  - 총 3개의 페이즈로 각각 1분씩 플레이한다.
    - 1페이즈 : 왼쪽에서 오른쪽으로 움직임.
    - 2페이즈 : 아래에서 위로 움직임
    - 3페이즈 : 5초정도  왼쪽에서 오른쪽으로 움직인 후, 위에서 아래로 움직임.
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

---

## 4. 파일 생성 및 작업 시 주의사항

1. **기존 주석 및 구조 보존**: 코드 리팩토링이나 수정 작업 시 코드 내에 있는 기성 주석(예: 개발 주석 및 수정 태그)을 무단으로 삭제하지 마십시오.
2. **에러 감지 및 로그 작성**: 예외 처리를 철저히 하고, 에러가 발생 가능한 지점에는 `Debug.LogError`를 사용해 의미 있는 메시지를 출력해야 합니다.
3. **Unity Physics2D 연동**: 이동 및 물리 제어 시 `Rigidbody2D`의 `linearVelocity` 속성을 적절히 변경하되, `FixedUpdate` 타이밍에서만 물리 연산을 처리하도록 설계합니다.

## 5. 코드 수정 시 주의사항

1. **수정 전 계획세우기** : 항상 코드를 수정할 때 바로 수정하는 것이 아닌 계획을 수립하고, 검증 후 수정하십시오.

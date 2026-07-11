using System.Collections.Generic;
using UnityEngine;



public class MonsterStateMachine
{

    private IMonsterState _currentState { get; set; }

    private Dictionary<IMonsterState, List<Transition>> _transitionMap;

    public MonsterStateMachine(MonsterController owner, IMonsterState initialState, Dictionary<IMonsterState, List<Transition>> transitionMap) 
    {
        _currentState = initialState;
        _transitionMap = transitionMap;
    }
    public MonsterStateMachine(BossController owner, IMonsterState initialState, Dictionary<IMonsterState, List<Transition>> transitionMap)
    {

        _currentState = initialState;
        _transitionMap = transitionMap;
    }

    public void Update() 
    {
        _currentState.Update();

        //  현재 상태에 걸려있는 전이 카드 뭉치(List)가 있는지 확인
        if (_transitionMap.TryGetValue(_currentState, out List<Transition> transitions))
        {
            //  등록된 여러 개의 전이 규칙을 위에서부터 차례대로 체크 (우선순위 순서)
            foreach (Transition transition in transitions)
            {
                if (transition.Condition()) // 람다식 조건 발동
                {
                    // 조건이 하나라도 만족되면 즉시 다음 상태로 전환하고 뼈대 탈출
                    ChangeState(transition.TargetState);
                    break;
                }
            }
        }


    }

    public void ChangeState(IMonsterState newState)
    {
        if (_currentState == newState) return; // 동일 상태로의 중복 전이 방지

        _currentState.Exit();   // 기존 상태 정리 (애니메이션 멈춤 등)
        _currentState = newState;
        _currentState.Enter();  // 새 상태 진입 (새 애니메이션 시작 등)
    }

}

using System;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class Transition
{
    // 1. 프로퍼티 (데이터 저장소)
    public Func<bool> Condition { get; }
    public IMonsterState TargetState { get; }

    // 2. 생성자 (조립기)
    public Transition(Func<bool> condition, IMonsterState targetState)
    {
        Condition = condition;
        TargetState = targetState;
    }

    // 사용 예시
//    new Transition(
//    condition: () => monster.IsPlayerInDetectionRange(),
//    targetState: chaseState
//)

//if (transition.Condition()) // <-- 변수 이름 뒤에 괄호()를 붙여서 함수를 발동
//{
//    ChangeState(transition.TargetState);
//}

}

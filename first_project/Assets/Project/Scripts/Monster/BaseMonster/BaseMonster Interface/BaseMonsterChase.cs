using UnityEngine;
using System.Collections;
public class BaseMonsterChase : MonsterChase
{


    public override void Chase()
    {
        StartCoroutine(CoChaseAnimation());
        StartCoroutine(CoChaseAction());
    }

    // 추적 애니메이션
    IEnumerator CoChaseAnimation()
    {
        // 상태가 바뀌지 않는한 무한 재생
        while (_monsterController.State == Status.Chase)
        {
            // 뛰어가기
            _mAnimator.SetBool(_chaseHash, true);
            yield return null;
        }

        _mAnimator.SetBool(_chaseHash, false);
    }
    // 추적 행위
    IEnumerator CoChaseAction()
    {
        // 상태가 바뀌지 않는한 무한 재생
        while (_monsterController.State == Status.Chase)
        {
            // 뛰어가기
            _monsterController.OnForce(_monsterController.Front, _monsterController.MaxSpeed);
            yield return null;

            //if(만약 Monster TO Player 벡터와 Monster 전방 벡터의 내적이 음수라면 )
            //{
            //      뒤돌기 후 다시 탐색으로 상태 전환
            //}
        }

    }

}

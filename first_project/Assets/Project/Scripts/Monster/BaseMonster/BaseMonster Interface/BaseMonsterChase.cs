using UnityEngine;
using System.Collections;
public class BaseMonsterChase : MonsterChase
{


    public override void Chase()
    {
        StartCoroutine(CoChaseAnimation());
    }

    // 탐색 애니메이션
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

}

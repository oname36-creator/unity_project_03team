using UnityEngine;
using System.Collections;
public class BaseMonsterAttack : MonsterAttack
{



    public override void Attack()
    {
        StartCoroutine(CoAttack());
    }


    // 공격 행위
    IEnumerator CoAttack()
    {
        // 상태가 바뀌지 않는한 무한 재생
        while (_monsterController.State == Status.Attack)
        {
            // 기본 몬스터의 공격
            _mAnimator.SetBool(_attackHash, true);
    
            _monsterController.State = Status.Chase;
            yield return null; 
        }
        _mAnimator.SetBool(_attackHash, false);

    }
}

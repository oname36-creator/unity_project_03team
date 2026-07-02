using UnityEngine;
using System.Collections;

public class BaseMonsterSearch : MonsterSearch
{





    public override void Search()
    {
        StartCoroutine(CoSearchAnimation());
    }

    // 탐색 애니메이션
    IEnumerator CoSearchAnimation()
    {
        // 상태가 바뀌지 않는한 무한 재생
        while (_monsterController.State == Status.Idle)
        {
            // 걷기 애니메이션 재생
            _mAnimator.SetBool(_workHash, true);
            // 앞 방향으로 최대 속도의 절반으로 걷기
            _monsterController.OnForce(_monsterController.Front, _monsterController.MaxSpeed / 2);
            yield return new WaitForSeconds(.5f); // 0.5초 걷기
                                                  // 걷기 애니메이션 해제

            while (!_monsterController.Stop()) // 멈출때까지 속도 줄이기
            {
                _monsterController.OnForce(_monsterController.Front, 0);
                yield return null;
            }
            _mAnimator.SetBool(_workHash, false);
            yield return new WaitForSeconds(.3f); // 0.3초 가만히 있기
            _monsterController.Front = -_monsterController.Front; // 반대전환
        }

        _mAnimator.SetBool(_workHash, false);
    }

}

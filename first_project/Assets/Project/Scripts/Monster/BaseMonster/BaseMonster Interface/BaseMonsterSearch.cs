using UnityEngine;
using System.Collections;

public class BaseMonsterSearch : MonsterSearch
{

    private bool _isStopFlag = false;

    public override void Search()
    {
        StartCoroutine(CoSearchAnimation());
        StartCoroutine(CoSearchAction());
    }

    // 탐색 애니메이션
    IEnumerator CoSearchAnimation()
    {
        // 상태가 바뀌지 않는한 무한 재생
        while (_monsterController.State == Status.Idle)
        {
            // 걷기 애니메이션 재생
            _mAnimator.SetBool(_workHash, true);
            yield return new WaitForSeconds(0.1f);

            // 앞 방향으로 최대 속도의 절반으로 걷기
            _isStopFlag = false;

            yield return new WaitForSeconds(2.0f); // 2.0초 걷기
            Debug.Log("3");                                  
            while (!_monsterController.Stop()) // 멈출때까지 속도 줄이기
            {
                _isStopFlag = true;

                yield return null;
            }
            _mAnimator.SetBool(_workHash, false); // 걷기 애니메이션 해제
            yield return new WaitForSeconds(1.0f); // 1.0초 가만히 있기
            _monsterController.Front = -_monsterController.Front; // 반대전환

        }

        _mAnimator.SetBool(_workHash, false);
    }
    
    // 행위 
    IEnumerator CoSearchAction()
    {
        while (_monsterController.State == Status.Idle)
        {
            if (_isStopFlag)
            {
                _monsterController.OnForce(_monsterController.Front, 0);
            }
            else
            {
                _monsterController.OnForce(_monsterController.Front, _monsterController.MaxSpeed / 2);
            }
            yield return null;
        }
    }

}

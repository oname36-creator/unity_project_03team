using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class BaseMonsterSearch : MonsterSearch
{

  

    public override void Search()
    {
 
        StartCoroutine(CoSearch());
    }

    private void ChangeState(Status state)
    {
        _monsterController.State = state;
    }

    private bool CheckRange() 
    {
        // 탐색
        // 코사인 값 (두 벡터는 정규화가 되어있음)
        float dotProduct = Vector2.Dot(_monsterController.Front, _monsterController.GetMToP);

        // 두 벡터 사이의 각도가 각도 범위 사이에 있을때 그리고 범위 안에 있을때
        if (dotProduct > _monsterController.CosValue && _monsterController.InRange)
        {
            return true;
        }
        return false;
    }

    // 탐색 애니메이션
    IEnumerator CoSearch()
    {
        // 상태가 바뀌지 않는한 무한 재생
        while (_monsterController.State == Status.Idle)
        {
            // 걷기 애니메이션 재생
            _mAnimator.SetBool(_workHash, true);
            yield return new WaitForSeconds(0.1f);
            _monsterController.OnForce(_monsterController.Front, _monsterController.MaxSpeed/2);
            // 앞 방향으로 최대 속도의 절반으로 걷기
            if (CheckRange())
            {
                // 추격으로 전환
                ChangeState(Status.Chase);
                _mAnimator.SetBool(_workHash, false);
                yield break;
            }
            yield return new WaitForSeconds(2.0f); // 2.0초 걷기                                 
            while (!_monsterController.Stop()) // 멈출때까지 속도 줄이기
            {
                _monsterController.OnForce(_monsterController.Front, 0);
                yield return null;
            }
            _mAnimator.SetBool(_workHash, false); // 걷기 애니메이션 해제
            yield return new WaitForSeconds(1.0f); // 1.0초 가만히 있기
            _monsterController.Front = -_monsterController.Front; // 반대전환
            Debug.Log("반대전환");

            if (CheckRange())
            {
                // 추격으로 전환
                ChangeState(Status.Chase);
                _mAnimator.SetBool(_workHash, false);
                yield break;
            }
            yield return null;
        }
        _mAnimator.SetBool(_workHash, false);
    }
    










}

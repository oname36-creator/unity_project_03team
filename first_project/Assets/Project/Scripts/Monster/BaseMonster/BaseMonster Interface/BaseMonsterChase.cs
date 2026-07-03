using UnityEngine;
using System.Collections;
public class BaseMonsterChase : MonsterChase
{


    public override void Chase()
    {
 
        StartCoroutine(CoSearch());
    }

    private void ChangeState(Status state) 
    {
        _monsterController.State = state;
    }


    // 추격 범위 벗어났는지 확인
    IEnumerator CoSearch()
    {
        while (_monsterController.State == Status.Chase)
        {
            // 뛰어가기 애니메이션
            _mAnimator.SetBool(_chaseHash, true);
        
            // 코사인 값 (두 벡터는 정규화가 되어있음)
            float dotProduct = Vector2.Dot(_monsterController.Front, _monsterController.GetMToP);

            if (dotProduct < 0) 
            {
                // 추격하다가 점프를 당하면 바로 방향전환
                _monsterController.Front = -_monsterController.Front;

                // 갱신
                dotProduct = Vector2.Dot(_monsterController.Front, _monsterController.GetMToP);

                // 방향전환의 자연스러움을 위해
                yield return new WaitForSeconds(0.1f); 
            }

            // 뛰어가기
            _monsterController.OnForce(_monsterController.Front, _monsterController.MaxSpeed);

            // 두 벡터 사이의 각도가 각도 범위 사이에 없고 또는 범위 밖일때
            if (dotProduct < _monsterController.CosValue || !_monsterController.InRange)
            {
                // 대기 상태로 전환
                ChangeState(Status.Idle);
                yield break;

            }
            // 공격 가능한지 체크
            else if (_monsterController.InAttackRange) 
            {
                // 공격 상태로 전환
                ChangeState(Status.Attack);
                yield break;
            }

            yield return null;
        }
        _mAnimator.SetBool(_chaseHash, false);
    }





}

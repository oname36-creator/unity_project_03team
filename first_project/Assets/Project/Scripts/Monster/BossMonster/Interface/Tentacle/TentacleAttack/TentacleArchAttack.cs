using UnityEngine;
using System.Collections;

public class TentacleArchAttack : IMonsterState
{
    private TentacleController _owner;
    private Transform _bossTransform;
    
    private float _timer;
    private float _duration = 3f; // 내려찍기 + 긁기 복귀 총 3초
    private Vector2 _startPos;
    private Vector2 _targetGroundPos;
    private Vector2 _endRootPos;
    
    public TentacleArchAttack(TentacleController owner)
    {
        _owner = owner;
        _bossTransform = owner.Boss.transform;
    }

    public void Enter()
    {
        _timer = 0f;
        _startPos = _owner.IkTargetPosition;
        
        // 타겟: 플레이어가 있던 바닥 위치로 내려찍기
        if (_owner.Boss.Player != null)
        {
            _targetGroundPos = _owner.Boss.Player.transform.position;
            // 바닥 y 좌표로 보정 (임시로 Player 위치의 아래쪽)
            float playerRadius = _owner.Boss.Player.GetComponent<CapsuleCollider2D>().size.y / 2;
            _targetGroundPos.y -= playerRadius; 
        }
        else
        {
            _targetGroundPos = _startPos;
            _targetGroundPos.y -= 10f;
        }
        
        _endRootPos = _bossTransform.position;
        // 바닥에서 끌어오는 것이므로 y좌표는 바닥으로 고정
        _endRootPos.y = _targetGroundPos.y; 

        // Tentacle 길이를 넉넉하게 늘려줌
        _owner.UpdateSegmentLength(_owner.PrevSegmentLength + 10);
    }

    public void Update()
    {
        _timer += Time.deltaTime;
        float t = Mathf.Clamp01(_timer / _duration);
        
        // 0 ~ 0.5 구간: 바닥으로 내려찍기 (아치를 그리기 위해 베지에 곡선 적용)
        // 0.5 ~ 1.0 구간: 몸통(Root)으로 바닥을 긁으며 돌아오기
        
        if (t <= 0.5f)
        {
            // 0 ~ 1 비율로 변환
            float dropT = t * 2f; 
            
            // 베지에 곡선 (중간 제어점을 시작점과 타겟점 사이 우측으로 살짝 뺌)
            Vector2 controlPoint = _startPos + (_targetGroundPos - _startPos) / 2f;
            controlPoint.x += 5f; // 약간 우측으로 배불뚝이 아치 생성
            
            Vector2 m1 = Vector2.Lerp(_startPos, controlPoint, dropT);
            Vector2 m2 = Vector2.Lerp(controlPoint, _targetGroundPos, dropT);
            
            _owner.IkTargetPosition = Vector2.Lerp(m1, m2, dropT);
        }
        else
        {
            // 0 ~ 1 비율로 변환
            float dragT = (t - 0.5f) * 2f;
            
            // 바닥에 고정된 상태로 Root 쪽으로 끌어오기
            _owner.IkTargetPosition = Vector2.Lerp(_targetGroundPos, _endRootPos, dragT);
        }

        if (_timer >= _duration)
        {
            // 복귀 완료, 초기화 및 풀 반환
            _owner.isArch = false;
            _owner.Attack = false;
            ObjectPoolManager.Instance.TentaclePush(_owner.gameObject);
        }
    }

    public void Exit()
    {
        
    }
}

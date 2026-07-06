using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class FlyMonsterAttack : IMonsterState
{
    private MonsterController _owner;
    private Animator _animator;
    private Transform _ownerTransform;
    private Vector2 _mToP;
    private Vector2 _playerPos;
    private Vector2 _myPos;
    private Vector2 _pivot;
    private Vector2 _moveDir;

    private float _radius;
    private float _radian;


    private float _totalMoveRadian;
    
    private float _delta;



    // 생성자에서 owner를 직접 받도록 셋업
    public FlyMonsterAttack(MonsterController owner)
    {
        this._owner = owner;
        _animator = _owner.GetComponent<Animator>();
    }

    public void Enter()
    {
        Debug.Log("Bird : attack");
        if (_owner.IsAttack) 
        {
            Debug.Log("Bird : attack return");
            return; 
        }
        
        _owner.IsAttack = true;
        
        _totalMoveRadian = 0f;

        _ownerTransform = _owner.GetComponent<Transform>();

        _myPos = _ownerTransform.position; 

        _mToP = _owner.GetMToP * _owner.GetMToPDistance; // 플에이어 위치
        _playerPos = _mToP + _myPos; // 플에이어 위치

        _mToP.x = Mathf.Abs(_mToP.x);
        _mToP.y= Mathf.Abs(_mToP.y);

        _pivot.x = _playerPos.x;
        _pivot.y = _playerPos.y + ((_mToP.x) * (_mToP.x) + (_mToP.y) * (_mToP.y))/(2 * _mToP.y);
        _radius = _pivot.y - _playerPos.y;


        // 최저점이 3*PI/2 이므로, 중심각 세타를 구해서 빼거나 더해줍니다.
        float theta = Mathf.Acos(_mToP.x / _radius);

        if (_myPos.x < _playerPos.x)
        {
            // 왼쪽에 있으면 각도가 3*PI/2 보다 작아야 시계방향(아래)으로 내려감
            _radian = (3f * Mathf.PI / 2f) - theta;
            _delta = 0.02f; // 시계 반대 방향으로 각도 증가 -> 플레이어 거쳐 우상향
        }
        else
        {
            // 오른쪽에 있으면 각도가 3*PI/2 보다 커야 반시계방향(아래)으로 내려감
            _radian = (3f * Mathf.PI / 2f) + theta;
            _delta = -0.02f; // 시계 방향으로 각도 감소 -> 플레이어 거쳐 좌상향
        }

        _owner.StartCoroutine(AttackRoutine());

    }


    public void Update()
    {
        
    }

    public void Exit()
    {
        
    }

    private IEnumerator AttackRoutine() 
    {
        float totalMovedRadian = 0f;
        // 반원만큼 다 이동할 때까지 루프를 돕니다.
        float targetTotalRadian = Mathf.PI * 0.90f; // 약 90% 지점까지 올라가면 종료
        

        while (totalMovedRadian < targetTotalRadian)
        {
            // 프레임 독립적인 각도 변화량 계산
            float angularSpeed = _delta * Time.deltaTime * 50f;
            _radian += angularSpeed;
            totalMovedRadian += Mathf.Abs(angularSpeed);

            // 원의 방정식을 이용한 다음 목표 위치 계산
            Vector2 nextPos;
            nextPos.x = _pivot.x + _radius * Mathf.Cos(_radian);
            nextPos.y = _pivot.y + _radius * Mathf.Sin(_radian);

            // 이동 벡터 구하기
            Vector2 currentPos = _ownerTransform.position;
            Vector2 moveDir = nextPos - currentPos;

            _owner.Move(moveDir);

            // 다음 프레임까지 대기
            yield return null;
        }

        // 루프를 정상적으로 탈출하면 공격 상태 종료 처리
        _owner.IsAttack = false;
        yield break;
    }


}

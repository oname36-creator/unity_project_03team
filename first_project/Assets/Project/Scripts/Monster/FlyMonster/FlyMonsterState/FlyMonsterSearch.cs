using UnityEngine;
using System.Collections;

public class FlyMonsterSearch : IMonsterState
{
    private MonsterController _owner;
    private Animator _animator;
    private Coroutine _patrolCoroutine;
    private Transform _ownerTransform;

    private Vector2 _centerPos;

    private float _patrolRadius = 10f;
    private float _flySpeed;
    private float _directionChangeInterval = 2f;

    public FlyMonsterSearch(MonsterController owner)
    {
        this._owner = owner;
        this._ownerTransform = _owner.GetComponent<Transform>();
        _animator = _owner.GetComponent<Animator>();
        _flySpeed = _owner.Speed;
    }
    public void Enter()
    {
        Debug.Log("Bird : serch");
        Vector2 startCenterPos = _ownerTransform.position;
        _patrolCoroutine = _owner.StartCoroutine(PatrolRoutine(_centerPos));

    }

    public void Update()
    {



    }

    public void Exit()
    {
        if (_patrolCoroutine != null)
        {
            _owner.StopCoroutine(_patrolCoroutine);
            _patrolCoroutine = null;
        }

    }

    private IEnumerator PatrolRoutine(Vector2 centerPosition)
    {
        // 초기 목표 위치는 현재 위치로 설정
        Vector2 targetPosition = _ownerTransform.position;
        float timer = _directionChangeInterval;

        while (true)
        {
            timer += Time.deltaTime;

            // 설정한 주기마다 '지정 범위(반경)' 내의 새로운 무작위 목표점을 계산
            if (timer >= _directionChangeInterval)
            {
                timer = 0f;

                // 중심점 기준 반지름(_patrolRadius) 안의 무작위 원형 좌표 구하기
                Vector2 randomOffset = Random.insideUnitCircle * _patrolRadius;
                targetPosition = centerPosition + randomOffset;
            }

            // 현재 위치에서 목표 위치로 부드럽게 이동하기 위한 방향 벡터 구하기
            Vector2 currentPos = _ownerTransform.position;
            Vector2 moveDir = (targetPosition - currentPos).normalized;

            // 목적지에 거의 다 도달했으면 굳이 더 움직이지 않고 미세하게 멈춤 (떠 있는 느낌)
            if (Vector2.Distance(currentPos, targetPosition) > 0.1f)
            {
                _owner.Move(moveDir);
            }
            else
            {
                _owner.Move(Vector2.zero);
            }
            yield return null;
        }
    }
}

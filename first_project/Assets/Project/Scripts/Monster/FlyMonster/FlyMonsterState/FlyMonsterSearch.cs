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
        _owner.GetComponent<Rigidbody2D>().gravityScale = 0;
    }
    public void Enter()
    {
        Debug.Log("Bird : serch");
        Vector2 startCenterPos = _ownerTransform.position;
        _owner.Stop();
        _patrolCoroutine = _owner.StartCoroutine(PatrolRoutine(_centerPos));
        _owner.IsBack = false;

    }

    public void Update()
    {



    }

    public void Exit()
    {
        Debug.Log("Bird : serchEnd");
        // 코루틴 종료
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

         
            if (timer >= _directionChangeInterval)
            {
                timer = 0f;
                _owner.Stop();
                _owner.Front = -_owner.Front;
                
                Vector2 randomOffset = _owner.Front * _patrolRadius;
                targetPosition = centerPosition + randomOffset;
                //_owner.Move(Vector2.up, true);
            }

            // 현재 위치에서 목표 위치로 이동하기 위한 방향 벡터
            Vector2 currentPos = _ownerTransform.position;
            Vector2 moveDir = (targetPosition - currentPos).normalized;

            // 중력 상쇄
            //_owner.Move(Vector2.up, false, true);

            if (Vector2.Distance(currentPos, targetPosition) > 0.1f)
            {
                _owner.Move(moveDir);
            }

            yield return null;
        }
    }
}

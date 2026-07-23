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
        _owner.SetExclamationMark(false);
        _owner.SetQuestionMark(true);
        //Debug.Log("Bird : search");

        _centerPos = _ownerTransform.position;

        _owner.Stop();
        _patrolCoroutine = _owner.StartCoroutine(PatrolRoutine(_centerPos));
        _owner.IsBack = false;
    }

    public void Update()
    {
    }

    public void Exit()
    {
        //Debug.Log("Bird : searchEnd");
        if (_patrolCoroutine != null)
        {
            _owner.StopCoroutine(_patrolCoroutine);
            _patrolCoroutine = null;
        }
    }

    private IEnumerator PatrolRoutine(Vector2 centerPosition)
    {
        Vector2 targetPosition = _ownerTransform.position;
        float timer = _directionChangeInterval;

        while (true)
        {
            timer += Time.fixedDeltaTime;

            if (timer >= _directionChangeInterval)
            {
                timer = 0f;
                _owner.Stop();
                _owner.Front = -_owner.Front;

                Vector2 randomOffset = _owner.Front * _patrolRadius;
                targetPosition = centerPosition + randomOffset;
            }

            Vector2 currentPos = _ownerTransform.position;

            if (Vector2.Distance(currentPos, targetPosition) > 0.1f)
            {

                Vector2 nextPosition = Vector2.MoveTowards(currentPos, targetPosition, _flySpeed * Time.fixedDeltaTime);

                _owner.MoveToPosition(nextPosition);
            }

            yield return new WaitForFixedUpdate();
        }
    }
}
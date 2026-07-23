using UnityEngine;
using System.Collections;

public class BodyMove : IMonsterState
{
    private BodyController _owner;
    private Transform _ownerTransform;
    private Transform _playerTransform;
    private Rigidbody2D _rigidbody2D;
    private float _timeCounter = 0f;
    private float _prevCounter = 0f;
    private Coroutine _movingCoroutine;

    private float _time = 0f;

    public BodyMove(BodyController owner)
    {
        this._owner = owner;
        _movingCoroutine = null;
        _ownerTransform = _owner.GetComponent<Transform>();
        _rigidbody2D = _owner.GetComponent<Rigidbody2D>();
        _playerTransform = _owner.Boss.Player.transform;
    }

    public void Enter()
    {

        //Debug.Log("BodyMove 진입");

        if (_movingCoroutine == null)
        {
            _timeCounter = 0f;
            _movingCoroutine = _owner.StartCoroutine(Move());
        }
    }

    public void Update()
    {
       if(_owner.Phase > 2) 
        {
            _time += Time.deltaTime;

            if(_time > 25f) 
            {
                _owner.CreateArch = true;
                _time = 0f;
            }
        }
    }

    public void Exit()
    {
    }

    IEnumerator Move()
    {


        while (true)
        {
            _timeCounter += Time.deltaTime;

            Vector2 forwardDir = ((Vector2)_playerTransform.position - (Vector2)_owner.transform.position).normalized;

            Vector2 perpDir = new Vector2(-forwardDir.y, forwardDir.x);
            float currentSpeed = _owner.MoveSpeed;

            float sineSpeed = Mathf.Cos(_timeCounter * _owner.SineFrequency) * _owner.SineAmplitude * _owner.SineFrequency;
            Vector2 sineVelocity = perpDir * sineSpeed;

            if (_owner.Boss != null && _owner.Boss.isIntro)
            {
                float distanceToPlayer = _owner.Distance;
                if (distanceToPlayer <= _owner.Boss.introSafeDistance)
                {
                    currentSpeed = 0;
                }
                else
                {
                    currentSpeed = _owner.MoveSpeed * _owner.Boss.introSpeedMultiplier;
                }

                sineVelocity = Vector2.zero;
            }

            Vector2 forwardVelocity = forwardDir * currentSpeed;

            _rigidbody2D.linearVelocity = forwardVelocity + sineVelocity;

            _timeCounter += Time.deltaTime;

            if (_timeCounter > _owner.TentacleCycle + _prevCounter)
            {

                if(_owner.Boss != null && !_owner.Boss.isIntro)
                {
                    _owner.Create = true;
                }
                _prevCounter = _timeCounter;
            }

            yield return null; 
        }
    }
}
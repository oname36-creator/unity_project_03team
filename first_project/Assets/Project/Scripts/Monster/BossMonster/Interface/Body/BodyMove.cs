using UnityEngine;
using System.Collections;

public class BodyMove : IMonsterState
{
    private BodyController _owner;
    private Transform _ownerTransform;
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
    }

    public void Enter()
    {

        Debug.Log("BodyMove 진입");

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

            if(_time > 10f) 
            {
                _owner.CreateArch = true;
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

            Vector2 forwardDir = _owner.Boss.Front;
            Vector2 perpDir = new Vector2(-forwardDir.y, forwardDir.x);
            float currentSpeed = _owner.MoveSpeed;
            
            float sineSpeed = Mathf.Cos(_timeCounter * _owner.SineFrequency) * _owner.SineAmplitude * _owner.SineFrequency;
            Vector2 sineVelocity = perpDir * sineSpeed;

            // 보스의 인트로 연출 상태 예외 처리
            if(_owner.Boss != null && _owner.Boss.isIntro)
            {
                float distanceToPlayer = _owner.Distance;
                if(distanceToPlayer <= _owner.Boss.introSafeDistance)
                {
                    currentSpeed = 0;
                }
                else
                {
                    currentSpeed = _owner.MoveSpeed * _owner.Boss.introSpeedMultiplier;
                }

                // 인트로 중에는 웨이브 차단
                sineVelocity = Vector2.zero;
            }
            Vector2 forwardVelocity = forwardDir * _owner.MoveSpeed;
            _rigidbody2D.linearVelocity = forwardVelocity + sineVelocity;

            if (_timeCounter > _owner.TentacleCycle + _prevCounter)
            {
                // 인트로 중 촉수 생성X
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
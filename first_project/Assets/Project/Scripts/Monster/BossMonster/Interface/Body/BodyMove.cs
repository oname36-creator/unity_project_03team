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
       
    }

    public void Exit()
    {
        if (_movingCoroutine != null)
        {
            _owner.StopCoroutine(_movingCoroutine);
            _movingCoroutine = null; // 참조 초기화
        }
    }

    IEnumerator Move()
    {
        while (true)
        {
            _timeCounter += Time.deltaTime;

            Vector2 forwardDir = _owner.Boss.Front;
            Vector2 perpDir = new Vector2(-forwardDir.y, forwardDir.x);

            Vector2 forwardVelocity = forwardDir * _owner.MoveSpeed;
            float sineSpeed = Mathf.Cos(_timeCounter * _owner.SineFrequency) * _owner.SineAmplitude * _owner.SineFrequency;
            Vector2 sineVelocity = perpDir * sineSpeed;

            _rigidbody2D.linearVelocity = forwardVelocity + sineVelocity;

            if (_timeCounter > 5f + _prevCounter)
            {
                _owner.Create = true;
                _prevCounter = _timeCounter;
            }

            yield return null; 
        }
    }
}
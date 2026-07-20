using UnityEngine;

public class BodyIdle : IMonsterState
{
    private BodyController _owner;
    private Transform _ownerTransform;
    private Rigidbody2D _rigidbody2D;
    private float _timeCounter = 0f;

    public BodyIdle(BodyController owner)
    {
        this._owner = owner;
        _ownerTransform = _owner.GetComponent<Transform>();
        _rigidbody2D = _owner.GetComponent<Rigidbody2D>();
    }

    public void Enter()
    {
        _timeCounter = 0f;
        _rigidbody2D.linearVelocity = Vector2.zero;
        //Debug.Log("BodyIdle");

    }

    public void Update()
    {
        _timeCounter += Time.deltaTime;

        float hoverVelocity = Mathf.Cos(_timeCounter * _owner.HoverFrequency) * _owner.HoverAmplitude;

        _rigidbody2D.linearVelocity = _ownerTransform.up * hoverVelocity;

        

    }

    public void Exit()
    {
        _rigidbody2D.linearVelocity = Vector2.zero;
    }
}
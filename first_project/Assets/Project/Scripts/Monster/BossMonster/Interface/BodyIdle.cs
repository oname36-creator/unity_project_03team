using UnityEngine;

public class BodyIdle : IMonsterState
{
    private BodyController _owner;
    private Transform _ownerTransform;
    private Rigidbody2D _rigidbody2D;

    public BodyIdle(BodyController owner)
    {
        this._owner = owner;
        _ownerTransform = _owner.GetComponent<Transform>(); //[cite: 8]
        _rigidbody2D = _owner.GetComponent<Rigidbody2D>();
    }

    public void Enter()
    {
 
        if (_rigidbody2D != null)
        {
            _rigidbody2D.linearVelocity = Vector2.zero;
        }
        Debug.Log("BodyIdle");
    }

    public void Update()
    {
    }

    public void Exit()
    {
    }
}
using UnityEngine;

public class BodyMove : IMonsterState
{
    private BodyController _owner;
    private Transform _ownerTransform;
    private Rigidbody2D _rigidbody2D;
    private Transform _targetTransform;

    private float maxSpeed;

    public BodyMove(BodyController owner)
    {
        this._owner = owner;
        _ownerTransform = _owner.GetComponent<Transform>();

        _rigidbody2D = _owner.GetComponent<Rigidbody2D>();

        maxSpeed = _owner.Boss.MaxSpeed;
    }

    public void Enter()
    {
        _targetTransform = _owner.Boss.Target;
        Debug.Log("BodyMove");
    }

    public void Update()
    {
        if (_targetTransform == null) return;

        // 현재 위치와 목적지(타겟) 사이의 거리 계산
        float distance = Vector2.Distance(_rigidbody2D.position, _targetTransform.position);

        // 목적지에 도달하면 Move false로
        if (distance <= _owner.ReleaseDistance) 
        {
            _owner.Move = false; 
            return;
        }

        Vector2 direction = ((Vector2)_targetTransform.position - _rigidbody2D.position).normalized;
        _rigidbody2D.AddForce(direction * _owner.PullForce * Time.deltaTime, ForceMode2D.Force);

        if (_rigidbody2D.linearVelocity.magnitude > maxSpeed)
        {
            _rigidbody2D.linearVelocity = _rigidbody2D.linearVelocity.normalized * maxSpeed;
        }

    }

    public void Exit()
    {
        _owner.Boss.Attached = false; // 도착 시 부착 상태 해제 (필요에 따라 유지 가능)
        _rigidbody2D.linearVelocity = Vector2.zero; // 정지
    }
}
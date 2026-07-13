using UnityEngine;

public class BodyMove : IMonsterState
{
    private BodyController _owner;
    private Transform _ownerTransform;
    private Rigidbody2D _rigidbody2D;
    private float _timeCounter = 0f;

    public BodyMove(BodyController owner)
    {
        this._owner = owner;
        _ownerTransform = _owner.GetComponent<Transform>();
        _rigidbody2D = _owner.GetComponent<Rigidbody2D>();
    }

    public void Enter()
    {
        _timeCounter = 0f;
        Debug.Log("BodyMove");
    }

    public void Update()
    {

        _timeCounter += Time.deltaTime;

        // 1. Front 방향 
        Vector2 forwardDir = _owner.Boss.Front;

        // 2. 수직 방향 벡터 (정면 벡터의 90도 회전: (-y, x))
        Vector2 perpDir = new Vector2(-forwardDir.y, forwardDir.x);

        // 3. 속도 벡터 계산
        // 위치를 A * sin(wt)로 만들기 위해 속도에는 미분값인 A * w * cos(wt)를 적용
        Vector2 forwardVelocity = forwardDir * _owner.MoveSpeed;
        float sineSpeed = Mathf.Cos(_timeCounter * _owner.SineFrequency) * _owner.SineAmplitude * _owner.SineFrequency;
        Vector2 sineVelocity = perpDir * sineSpeed;

        // 4. 리지드바디에 합성된 속도 적용
        _rigidbody2D.linearVelocity = forwardVelocity + sineVelocity;


    }

    public void Exit()
    {
        _rigidbody2D.linearVelocity = Vector2.zero;
    }
}
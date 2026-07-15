using UnityEngine;

public class TentacleReturn : IMonsterState
{
    private TentacleController _owner;

    private Vector2 _rootPos;
    private Vector2 _targetPos;

    private float _reachThreshold = 0.5f;



    public TentacleReturn(TentacleController owner)
    {
        _owner = owner;
    }
    public void Enter()
    {
        _rootPos = _owner.RootPos;
        _targetPos = _owner.IkTargetPosition;

    }

    public void Update()
    {
        
        float dt = Time.deltaTime;
        float deltaX = Mathf.Sin(dt);

        _owner.segmentDistance -= dt;
        _targetPos.x += deltaX;
        _owner.IkTargetPosition = _targetPos;

        if (_owner.segmentDistance < 0.1f) 
        {
            _owner.isTrap = false;
        }
    }

    public void Exit()
    {
        ObjectPoolManager.Instance.TentaclePush(_owner.gameObject);
    }
}

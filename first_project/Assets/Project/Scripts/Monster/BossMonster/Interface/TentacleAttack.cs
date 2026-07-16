using UnityEngine;

public class TentacleAttack : IMonsterState
{
    private TentacleController _owner;
    private Transform _bossTransform;
    private Transform _playerTransform;

    // 5초 타이머를 위한 변수
    private float _timer;
    private float _duration = 0.25f;
    private float _playerRadius;


    private Vector3 _targetPos;

    private float _reachThreshold = 0.5f;  // 목표 지점에 도달했는지 판단하는 거리


    public TentacleAttack(TentacleController owner)
    {
        _owner = owner;
        _bossTransform = owner.Boss.transform;
        _playerTransform = owner.Boss.Player.transform;
        _playerRadius = _owner.Boss.Player.GetComponent<CapsuleCollider2D>().size.y / 2;
    }
    public void Enter()
    {
        _owner.UpdateSegmentLength(_owner.PrevSegmentLength + 5);
        _targetPos = _playerTransform.position;
        _targetPos.y -= _playerRadius;

        _owner.segmentDistance = (_targetPos - _bossTransform.position).magnitude / 20f;

        _owner.IkTargetPosition = _targetPos;

        _owner.SlashAnimation();
    }

    public void Update()
    {

        if (_owner.IsAttach)
        {
            _owner.Attack = false;
            return;
        }

        float distanceToFinalTarget = Vector2.Distance(_owner.GetGrabber.position, _targetPos);

        if (distanceToFinalTarget < _reachThreshold)
        {
            Debug.Log("TentacleAttack: 뻗었지만 아무것도 닿지 않음... 회수 시작");
            _owner.Attack = false;
        }

        if (_timer >= _duration)
        {
            _owner.Attack = false;

        }

    }

    public void Exit()
    {
        if (!_owner.IsAttach) 
        {
            ObjectPoolManager.Instance.TentaclePush(_owner.gameObject);
        }
        
    }
}

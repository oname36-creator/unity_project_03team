using Unity.Cinemachine;
using UnityEngine;

public class TentacleTrapAction : IMonsterState
{


    private TentacleController _owner;
    private CinemachineImpulseSource _cinemachine;


    private Vector2 _rootPos;
    private Vector2 _targetPos;
    private Vector2 _currentIkPos;


    private float _riseSpeed = 50f;
    private float _reachThreshold = 0.5f;


    public TentacleTrapAction(TentacleController owner)
    {
        _owner = owner;
        _cinemachine = _owner.GetComponent<CinemachineImpulseSource>();
    }

    public void Enter()
    {
        //Debug.Log("TentacleTrap Action");
        _rootPos = _owner.RootPos;
        _targetPos = _rootPos;

        float dy;
        if (_owner.Up) 
        {
            dy = 20;
        }
        else 
        {
            dy = -20;
        }


        _targetPos.y += dy;

        _owner.UpdateSegmentLength(20);
        _owner.segmentDistance = 1f;

        _currentIkPos = _rootPos;
        _owner.IkTargetPosition = _currentIkPos;

        _cinemachine.GenerateImpulse();

        _owner.SlashAnimation(true);

        SoundManager.Instance.PlaySFX("BossTrapAttack");

    }

    public void Update()
    {
        if (_owner.IsAttach)
        {
            return;
        }

        _currentIkPos = Vector2.MoveTowards(_currentIkPos, _targetPos, _riseSpeed * Time.deltaTime);
        _owner.IkTargetPosition = _currentIkPos;

        float sqrDistanceToFinalTarget = (_currentIkPos - _targetPos).sqrMagnitude;
        if (sqrDistanceToFinalTarget < _reachThreshold * _reachThreshold)
        {
            _owner.Attack = false;
        }

    }

    public void Exit()
    {
        _owner.SlashAnimation(true);
    }
}

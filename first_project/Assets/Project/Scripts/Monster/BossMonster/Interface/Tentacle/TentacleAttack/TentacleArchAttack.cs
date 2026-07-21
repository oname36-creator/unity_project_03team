using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class TentacleArchAttack : IMonsterState
{
    private TentacleController _owner;
    private CinemachineImpulseSource _cinemachine;



    private float _startAngle;
    private float _rotationSpeed = 120f; // 내려찍는 회전 속도
    private float _shrinkSpeed = 1.5f;   // 수축하는 속도
    private bool _hitGround = false;
    private float _waitTimer = 0f;
    
    public TentacleArchAttack(TentacleController owner)
    {
        _owner = owner;
        _cinemachine = _owner.GetComponent<CinemachineImpulseSource>();
    }

    public void Enter()
    {
        Debug.Log("TentacleArchAttack");
        _owner.isParabola = true;
        _startAngle = _owner.parabolaAngle;
        
        _owner.IsGroundHit = false;
        _hitGround = false;
        _waitTimer = 0f;
    }

    public void Update()
    {
        if (_hitGround)
        {
            _waitTimer += Time.deltaTime;
            if (_waitTimer >= 0.5f)
            {
                _owner.segmentDistance -= _shrinkSpeed * Time.deltaTime;
                
                if (_owner.segmentDistance < 0.1f)
                {
                    _owner.isArch = false;
                    _owner.isParabola = false;
                    _owner.Attack = false;
                    ObjectPoolManager.Instance.TentaclePush(_owner.gameObject);
                }
            }
        }
        else
        {
            // 각도를 누적하며 회전 (내려찍기)
            _owner.parabolaAngle -= _rotationSpeed * Time.deltaTime;

            _owner.SlashAnimation(true);

            if (_owner.IsGroundHit)
            {
                _hitGround = true;
                _cinemachine.GenerateImpulse();
                SoundManager.Instance.PlaySFX("BossTrapAttack");
            }
        }

        
    }

    public void Exit()
    {
        _owner.isArch = false;
        _owner.isParabola = false;
    }
}
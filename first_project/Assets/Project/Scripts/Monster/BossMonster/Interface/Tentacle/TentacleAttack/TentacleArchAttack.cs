using UnityEngine;
using System.Collections;

public class TentacleArchAttack : IMonsterState
{
    private TentacleController _owner;
    
    private float _startAngle;
    private float _rotationSpeed = 120f; // 내려찍는 회전 속도
    private bool _hitGround = false;
    private float _waitTimer = 0f;
    
    public TentacleArchAttack(TentacleController owner)
    {
        _owner = owner;
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
                _owner.isArch = false;
                _owner.isParabola = false;
                _owner.Attack = false;
                ObjectPoolManager.Instance.TentaclePush(_owner.gameObject);
            }
        }
        else
        {
            // 각도를 누적하며 회전 (내려찍기)
            _owner.parabolaAngle -= _rotationSpeed * Time.deltaTime;

            if (_owner.IsGroundHit)
            {
                _hitGround = true;
            }
        }
    }

    public void Exit()
    {
        _owner.isArch = false;
        _owner.isParabola = false;
    }
}
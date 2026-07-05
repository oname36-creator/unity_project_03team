using Unity.VisualScripting;
using UnityEngine;

public class FlyMonsterAttack : IMonsterState
{
    private MonsterController _owner;
    private Animator _animator;
    private Vector2 _mToP;
    private Vector2 _playerPos;
    private Vector2 _myPos;
    private Vector2 _pivot;
    private Vector2 _moveDir;

    private float _radius;
    private float _radian;
    private float _delta;

    private float _timer;
    // 애니메이션 작동 시간
    private readonly float _attackDuration = 10f / 12f;


    // 생성자에서 owner를 직접 받도록 셋업
    public FlyMonsterAttack(MonsterController owner)
    {
        this._owner = owner;
        _animator = _owner.GetComponent<Animator>();
    }

    public void Enter()
    {
        _owner.IsAttack = true;
        
        _myPos = _owner.GetComponent<Transform>().position; 
        _mToP = _owner.GetMToP * _owner.GetMToPDistance; // 플에이어 위치
        _playerPos = _mToP + _myPos; // 플에이어 위치
        _mToP.x = Mathf.Abs(_mToP.x);
        _mToP.y= Mathf.Abs(_mToP.y);

        _pivot.x = _playerPos.x;
        _pivot.y = _playerPos.y + ((_mToP.x) * (_mToP.x) + (_mToP.y) * (_mToP.y))/(2 * _mToP.y);

        _radius = _pivot.y - _playerPos.y;

        // cos 값
        _radian = Vector2.Dot(Vector2.Normalize(_playerPos - _pivot), Vector2.Normalize(_myPos - _pivot));
        // arccos 
        _radian = Mathf.Acos(_radian);

        if (_radian < 0)
        {
            _delta = 0.01f;
        }
        else 
        {
            _delta = -0.01f;
        }
    }


    public void Update()
    {
        _radian += _delta;
        _moveDir.x = _radius * Mathf.Cos(_radian);
        _moveDir.y = -_radius * Mathf.Sin(_radian);

        if ((_owner.Front.x < 0 && _moveDir.x > 0) ||
            (_owner.Front.x > 0 && _moveDir.x < 0)) 
        {
            _moveDir.x = -_moveDir.x;
        } 


        _owner.Move(_moveDir);

        if (_radius < 0.05F && _radius > -0.05F) 
        {
            _owner.IsAttack = false;
        }
    }

    public void Exit()
    {
        
    }
}

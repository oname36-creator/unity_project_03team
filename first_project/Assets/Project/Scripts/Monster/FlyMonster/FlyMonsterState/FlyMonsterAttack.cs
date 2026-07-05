using Unity.VisualScripting;
using UnityEngine;

public class FlyMonsterAttack : IMonsterState
{
    private MonsterController _owner;
    private Animator _animator;
    private Vector2 _playerPos;
    private Vector2 _myPos;

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
        _playerPos = _owner.GetMToP * _owner.GetMToPDistance;
        _myPos = _owner.GetComponent<Transform>().position;
    }


    public void Update()
    {

    }

    public void Exit()
    {

    }
}

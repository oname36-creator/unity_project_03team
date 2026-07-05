using UnityEngine;

public class FlyMonsterCollider : IMonsterState
{
    private MonsterController _owner;
    private Animator _animator;

    private Vector2 _jumpDir;


    // 생성자에서 owner를 직접 받도록 셋업
    public FlyMonsterCollider(MonsterController owner)
    {
        this._owner = owner;
        _animator = _owner.GetComponent<Animator>();
    }
    public void Enter()
    {
        _owner.IsHurt = true;
        _owner.IsBack = true;
        _owner.Front = -_owner.Front;
    }

    public void Update()
    {

    }

    public void Exit()
    {

    }
}


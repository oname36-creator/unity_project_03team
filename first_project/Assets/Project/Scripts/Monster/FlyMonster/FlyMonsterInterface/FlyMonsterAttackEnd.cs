using UnityEngine;

public class FlyMonsterAttackEnd : IMonsterState
{
    private MonsterController _owner;
    private Animator _animator;



    // 생성자에서 owner를 직접 받도록 셋업
    public FlyMonsterAttackEnd(MonsterController owner)
    {
        this._owner = owner;
        _animator = _owner.GetComponent<Animator>();
    }
    public void Enter()
    {

    }


    public void Update()
    {

    }

    public void Exit()
    {

    }
}

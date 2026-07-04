using UnityEngine;

public class MonsterDie : IMonsterState
{
    private MonsterController _owner;
    private Animator _animator;


    // 생성자에서 owner를 직접 받도록 셋업
    public MonsterDie(MonsterController owner)
    {
        this._owner = owner;
        _animator = _owner.GetComponent<Animator>();
    }
    public void Enter()
    {
        _animator.SetBool(AnimatorHash.IsDead, true);
    }

    public void Update()
    {

    }

    public void Exit()
    {
        _owner.IsDead = false;
        _animator.SetBool(AnimatorHash.IsDead, false);
    }


}

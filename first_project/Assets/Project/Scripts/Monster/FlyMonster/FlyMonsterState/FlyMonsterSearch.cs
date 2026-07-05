using UnityEngine;

public class FlyMonsterSearch : IMonsterState
{
    private MonsterController _owner;
    private Animator _animator;


    // 생성자에서 owner를 직접 받도록 셋업
    public FlyMonsterSearch(MonsterController owner)
    {
        this._owner = owner;
        _animator = _owner.GetComponent<Animator>();
    }
    public void Enter()
    {
        _animator.SetBool(AnimatorHash.IsFly, false);
        _animator.SetBool(AnimatorHash.Idle, true);
    }

    public void Update()
    {
        // 가만히 서있기

    }

    public void Exit()
    {
        _animator.SetBool(AnimatorHash.Idle, false);
    }

}

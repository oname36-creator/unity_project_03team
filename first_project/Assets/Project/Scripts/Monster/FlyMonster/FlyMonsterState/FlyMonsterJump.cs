using UnityEngine;

public class FlyMonsterJump : IMonsterState
{
    private MonsterController _owner;
    private Animator _animator;

    private Vector2 _jumpDir;


    // 생성자에서 owner를 직접 받도록 셋업
    public FlyMonsterJump(MonsterController owner)
    {
        this._owner = owner;
        _animator = _owner.GetComponent<Animator>();
    }
    public void Enter()
    {
        _animator.SetBool(AnimatorHash.IsFly, false);
        _animator.SetTrigger(AnimatorHash.IsJump);
        
        _jumpDir = _owner.Front;
        _jumpDir.y = 1;
        _jumpDir = _jumpDir.normalized;
        _owner.Move(_jumpDir);
    }


    public void Update()
    {
        
    }

    public void Exit()
    {

    }
}

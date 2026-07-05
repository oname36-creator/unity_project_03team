using UnityEngine;

public class FlyMonsterFly : IMonsterState
{
    private MonsterController _owner;
    private Animator _animator;



    // 생성자에서 owner를 직접 받도록 셋업
    public FlyMonsterFly(MonsterController owner)
    {
        this._owner = owner;
        _animator = _owner.GetComponent<Animator>();
    }

    public void Enter()
    {
        _animator.SetBool(AnimatorHash.IsFly, true);
    }


    public void Update()
    {
        _owner.Move(_owner.Front); // 앞으로 진행
        _owner.Move(Vector2.up, true); // 떨어지지 않게
    }

    public void Exit()
    {

    }
}

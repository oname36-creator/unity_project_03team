using UnityEngine;

public class DarkWolfChase : IMonsterState
{
    private MonsterController _owner;
    private Animator _animator;

    private float _maxSpeed;

    // 생성자에서 owner를 직접 받도록 셋업
    public DarkWolfChase(MonsterController owner)
    {
        this._owner = owner;
        _animator = _owner.GetComponent<Animator>();
        _maxSpeed = _owner.MaxSpeed;
    }
    public void Enter()
    {

        _owner.SetExclamationMark(true);
        _owner.SetQuestionMark(false);


        _animator.SetBool(AnimatorHash.IsChase, true);

        _owner.IsWalk = false;
    }

    public void Update()
    {
        _owner.Move(_owner.GetMToP * _maxSpeed);
    }

    public void Exit()
    {
        _animator.SetBool(AnimatorHash.IsChase, false);


    }


}

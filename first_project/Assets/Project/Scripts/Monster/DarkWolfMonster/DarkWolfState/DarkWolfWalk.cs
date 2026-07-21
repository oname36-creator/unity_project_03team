using UnityEngine;

public class DarkWolfWalk : IMonsterState
{
    private MonsterController _owner;
    private Animator _animator;
    private Rigidbody2D _ownerRigidbody;

    private float _speed;

    // 생성자에서 owner를 직접 받도록 셋업
    public DarkWolfWalk(MonsterController owner)
    {
        this._owner = owner;
        _animator = _owner.GetComponent<Animator>();
        _ownerRigidbody = _owner.GetComponentInChildren<Rigidbody2D>();
        _speed = _owner.Speed;
    }
    public void Enter()
    {

        _owner.SetExclamationMark(false);
        _owner.SetQuestionMark(true);


        _animator.SetBool(AnimatorHash.IsWalk, true);

        _owner.IsWalk = true;

    }

    public void Update()
    {
        _owner.MoveToPosition(_speed * _owner.Front * Time.deltaTime + _ownerRigidbody.position);
    }

    public void Exit()
    {
        _animator.SetBool(AnimatorHash.IsWalk, false);

    }


}

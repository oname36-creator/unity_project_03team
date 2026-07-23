using UnityEngine;

public class DarkWolfAttack : IMonsterState
{
    private MonsterController _owner;
    private Animator _animator;

    private float _time;

    // 생성자에서 owner를 직접 받도록 셋업
    public DarkWolfAttack(MonsterController owner)
    {
        this._owner = owner;
        _animator = _owner.GetComponent<Animator>();
    }
    public void Enter()
    {

        _owner.SetExclamationMark(true);
        _owner.SetQuestionMark(false);

        _animator.SetTrigger(AnimatorHash.IsAttack);

        _time = 0f;

        SoundManager.Instance.PlaySFX("DarkWolfAttack");
    }

    public void Update()
    {
        if(_time >= 1f) 
        {
            _animator.SetTrigger(AnimatorHash.IsAttack);
            _time = 0f;
        }
    }

    public void Exit()
    {

    }


}

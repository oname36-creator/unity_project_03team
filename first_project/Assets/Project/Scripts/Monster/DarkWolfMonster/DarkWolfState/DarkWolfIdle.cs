using UnityEngine;

public class DarkWolfIdle : IMonsterState
{
    private MonsterController _owner;
    private Animator _animator;

    private Rigidbody2D _ownerRigidbody;

    private float _time = 0f;


    // 생성자에서 owner를 직접 받도록 셋업
    public DarkWolfIdle(MonsterController owner)
    {
        this._owner = owner;
        _animator = _owner.GetComponent<Animator>();
        _ownerRigidbody = _owner.GetComponent<Rigidbody2D>();
    }
    public void Enter()
    {
        //Debug.Log("DWIdle");
        _owner.SetExclamationMark(false);
        _owner.SetQuestionMark(true);

        _animator.SetBool(AnimatorHash.Idle, true);

        _time = 0f;

        if (_owner.IsWalk)
        {
            _owner.Stop();
            _owner.IsWalk = false;
        }
    }

    public void Update()
    {

        _time += Time.deltaTime;

        if (_time >= 2 && _ownerRigidbody.linearVelocityY > -0.01f)
        {
            // 뒤집어 주기
            _owner.Front = -_owner.Front;

            _time = 0f;
        }

    }

    public void Exit()
    {

        _animator.SetBool(AnimatorHash.Idle, false);

    }


}

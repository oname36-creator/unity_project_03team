using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class DarkWolfWalk : IMonsterState
{
    private MonsterController _owner;

    private Animator _animator;
    private Rigidbody2D _ownerRigidbody;

    private float _speed;

    private float _time;

    private Vector2 _prePos;
    public float _currentSpeed; 

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
        Debug.Log("DWWalk");
        _time = 0f;
        _owner.SetExclamationMark(false);
        _owner.SetQuestionMark(true);


        _animator.SetBool(AnimatorHash.IsWalk, true);

        _owner.IsWalk = true;
        _prePos = Vector2.zero;
    }

    public void Update()
    {

        Vector2 curPos = _ownerRigidbody.position;
        Vector2 dir = _speed * _owner.Front * Time.deltaTime + _ownerRigidbody.position;

        _owner.MoveToPosition(dir);
        float distance = Vector2.Distance(curPos, _prePos);
        _currentSpeed = distance / Time.deltaTime;

        Debug.Log("속도 : " + _currentSpeed);
        if (_currentSpeed < 0.01f)
        {
            _time += Time.deltaTime;
            if (_time > 1f)
            {
                _owner.Front = -_owner.Front;
                _time = 0f;
            }
        }
        else 
        {
            _time = 0f;
        }

        _prePos = curPos;

    }

    public void Exit()
    {
        _animator.SetBool(AnimatorHash.IsWalk, false);

    }


}

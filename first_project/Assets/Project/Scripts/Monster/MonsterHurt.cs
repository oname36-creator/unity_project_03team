using UnityEngine;

public class MonsterHurt : IMonsterState
{

    private MonsterController _owner;
    private Animator _animator;

    private float _timer;

    // 애니메이션 작동 시간
    private readonly float _attackDuration = 10f / 12f;

    // 생성자에서 owner를 직접 받도록 셋업
    public MonsterHurt(MonsterController owner)
    {
        this._owner = owner;
        _animator = _owner.GetComponent<Animator>();
    }
    public void Enter()
    {
        _animator.SetBool(AnimatorHash.IsHurt, true);
        _timer = 0f;
    }

    public void Update()
    {
        if (!_owner.Stop()) 
        {
            _owner.Move(-_owner.Front);
        }

        _timer += Time.deltaTime;

        if (_timer >= _attackDuration)
        {
            _owner.IsHurt =false;
        }

    }

    public void Exit()
    {
        _owner.IsHurt = false;
        _animator.SetBool(AnimatorHash.IsHurt, false);
    }



}

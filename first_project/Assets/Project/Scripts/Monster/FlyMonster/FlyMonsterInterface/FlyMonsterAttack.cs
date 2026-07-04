using UnityEngine;

public class FlyMonsterAttack : IMonsterState
{
    private MonsterController _owner;
    private Animator _animator;

    private float _timer;
    // 애니메이션 작동 시간
    private readonly float _attackDuration = 10f / 12f;


    // 생성자에서 owner를 직접 받도록 셋업
    public FlyMonsterAttack(MonsterController owner)
    {
        this._owner = owner;
        _animator = _owner.GetComponent<Animator>();
    }

    public void Enter()
    {

    }


    public void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= _attackDuration)
        {
            _owner.IsAttack = false;
        }

    }

    public void Exit()
    {

    }
}

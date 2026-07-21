using UnityEngine;

public class MonsterHurt : IMonsterState
{

    private MonsterController _owner;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    private float _timer;
    private string _name;

    // 애니메이션 작동 시간
    private readonly float _attackDuration = 0.5f;

    // 생성자에서 owner를 직접 받도록 셋업
    public MonsterHurt(MonsterController owner)
    {
        this._owner = owner;
        _animator = _owner.GetComponent<Animator>();
        _spriteRenderer = _owner.GetComponent<SpriteRenderer>();
        _name = _owner.Name;
    }
    public void Enter()
    {
        _animator.SetTrigger(AnimatorHash.IsHurt);

        _timer = 0f;

        if(_name == "Bird")
        {
            SoundManager.Instance.PlaySFX("BirdHurt");
        }
        else if (_name == "DarkWolf")
        {
            SoundManager.Instance.PlaySFX("DarkWolfHurt");
        }
    }

    public void Update()
    {
        _owner.Stop();

        _timer += Time.deltaTime;
        // 반투명한 빨간색
        _spriteRenderer.color = new Color(1f, 0f, 0f, 0.5f);
        if (_timer >= _attackDuration)
        {

            _owner.IsHurt =false;
        }

    }

    public void Exit()
    {
        _owner.IsHurt = false;
        // 원상복귀
        _spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
    }



}

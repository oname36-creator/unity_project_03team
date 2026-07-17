using UnityEngine;

public class BodyHurt : IMonsterState
{

    private BossController _owner;
    private SpriteRenderer _spriteRenderer;
    private float _time;
    private float _attackDuration = 0.5f;

    public BodyHurt(BossController owner)
    {
        this._owner = owner;
        _spriteRenderer = _owner.GetComponent<SpriteRenderer>();
    }


    public void Enter()
    {
        _time = 0f;
    }

    public void Update()
    {
        _time += Time.deltaTime;
        // 반투명
        _spriteRenderer.color = new Color(0f, 0f, 0f, 0.5f);
        if (_time >= _attackDuration)
        {

            _owner.IsHurt = false;
        }

    }

    public void Exit()
    {
        _owner.IsHurt = false;
    }
}



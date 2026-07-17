using UnityEngine;

public class BossAttack : IMonsterState
{

    private BossController _owner;
    private Transform _ownerTransform;
    private GameObject _player;
    private Transform _playerTransform;

    public BossAttack(BossController owner)
    {
        this._owner = owner;
        _ownerTransform = _owner.transform;

        _player = _owner.Player;
        _playerTransform = _player.transform;
    }

    public void Enter()
    {
        _owner.Chase = false;
        _owner.gameObject.tag = "Monster";
        _owner.gameObject.layer = LayerMask.NameToLayer("Monster");
    }

    public void Update()
    {


    }

    public void Exit()
    {

    }
}

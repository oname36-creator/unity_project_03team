using UnityEngine;

public class MonsterDie : IMonsterState
{
    private MonsterController _owner;
    private Animator _animator;

    private GameObject _ownerGameObject;


    // 생성자에서 owner를 직접 받도록 셋업
    public MonsterDie(MonsterController owner)
    {
        this._owner = owner;
        _animator = _owner.GetComponent<Animator>();
        _ownerGameObject = _owner.gameObject;
    }
    public void Enter()
    {
        Debug.Log("Die 상태");
        _ownerGameObject.tag = "Untagged";
        _ownerGameObject.layer = LayerMask.NameToLayer("Default");
        _animator.SetBool(AnimatorHash.Idle, false);
        _animator.SetTrigger(AnimatorHash.IsDead);

        _owner.Stop();

    }

    public void Update()
    {

    }

    public void Exit()
    {
        _owner.IsDead = false;
        _animator.SetBool(AnimatorHash.Idle, true);
        //_owner.gameObject.SetActive(true);
    }


}

using UnityEngine;

public class MonsterDie : IMonsterState
{
    private MonsterController _owner;
    private Animator _animator;

    private GameObject _ownerGameObject;
    private string _name;

    // 생성자에서 owner를 직접 받도록 셋업
    public MonsterDie(MonsterController owner)
    {
        this._owner = owner;
        _animator = _owner.GetComponent<Animator>();
        _ownerGameObject = _owner.gameObject;
        _name = _owner.Name;
    }
    public void Enter()
    {
        //Debug.Log("Die 상태");
        // Default = 0
        _ownerGameObject.layer = 0;
        _animator.SetBool(AnimatorHash.Idle, false);
        _animator.SetTrigger(AnimatorHash.IsDead);

        _owner.StopAllCoroutines();

        _owner.Stop();

        if (_name == "Bird")
        {
            SoundManager.Instance.PlaySFX("BirdDead");
        }
        else if(_name == "DarkWolf") 
        {
            SoundManager.Instance.PlaySFX("DarkWolfDead");
        }
        else if(_name == "Base") 
        {
            SoundManager.Instance.PlaySFX("SlimBoom");
        }

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

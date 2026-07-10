using UnityEngine;
using System.Collections;
public class BaseMonsterChase : IMonsterState
{
    private MonsterController _owner;
    private Animator _animator;

    // 생성자에서 owner를 직접 받도록 셋업
    public BaseMonsterChase(MonsterController owner)
    {
        this._owner = owner;
        _animator = _owner.GetComponent<Animator>();
    }
    public void Enter() 
    {
        //Debug.Log("추격");
        //_animator.SetBool(AnimatorHash.IsChase, true);
    }

    public void Update() 
    {
        _owner.Move(_owner.GetMToP);
    }

    public void Exit() 
    {
        //_animator.SetBool(AnimatorHash.IsChase, false);

    }






}

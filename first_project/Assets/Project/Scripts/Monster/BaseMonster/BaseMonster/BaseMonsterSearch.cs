using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class BaseMonsterSearch :  IMonsterState
{

    private MonsterController _owner;
    private Animator _animator;
  

    // 생성자에서 owner를 직접 받도록 셋업
    public BaseMonsterSearch(MonsterController owner)
    {
        this._owner = owner;
        _animator = _owner.GetComponent<Animator>();
    
    }


    public void Enter() 
    {
        Debug.Log("대기");
        _animator.SetBool(AnimatorHash.Idle, true);
    }

    public void Update() 
    {

        _owner.Move(_owner.Front); 
        
        

    }

    public void Exit() 
    {
        _animator.SetBool(AnimatorHash.Idle, false);
    }






}

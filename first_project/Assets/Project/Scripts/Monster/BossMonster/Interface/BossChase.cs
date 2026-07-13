using System.Collections;
using UnityEngine;

public class BossChase : IMonsterState
{
    private BossController _owner;
    private Transform _ownerTransform;

    private Coroutine _chaseCoroutine;



    // 생성자에서 owner를 직접 받도록 셋업
    public BossChase(BossController owner)
    {
        this._owner = owner;
        _ownerTransform = _owner.GetComponent<Transform>();
        _chaseCoroutine = _owner.StartCoroutine(Chase());

    }


    public void Enter()
    {

    }

    public void Update()
    {


    }

    public void Exit()
    {

    }



    IEnumerator Chase() 
    {


        //_owner.SetTarget();

        yield return new WaitForSeconds(2.0f);
    }





}

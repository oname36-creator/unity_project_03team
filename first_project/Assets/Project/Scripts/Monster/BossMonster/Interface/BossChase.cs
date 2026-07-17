using System.Collections;
using UnityEngine;

public class BossChase : IMonsterState
{
    private BossController _owner;
    private Transform _ownerTransform;
    private Transform _playerTransform;

    private MonsterRespawn _monsterRespawn;

    
    private Coroutine _chaseCoroutine;



    private float _time;


    // 생성자에서 owner를 직접 받도록 셋업
    public BossChase(BossController owner)
    {
        this._owner = owner;
        _ownerTransform = _owner.GetComponent<Transform>();
        _chaseCoroutine = _owner.StartCoroutine(Chase());
        _monsterRespawn = _owner.MonsterRespawner.GetComponent<MonsterRespawn>();
        _playerTransform = _owner.Player.transform;
    }


    public void Enter()
    {
        _time = 0f;
    }

    public void Update()
    {
        _time += Time.deltaTime;

        if(_time > 6f) 
        {

            Vector2 pos = _playerTransform.position;
            pos.x += 10f;
            pos.y -= 10f;
            Debug.Log("Boss Respawn Tentacle Trap");
            _time = 0f;
        }

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

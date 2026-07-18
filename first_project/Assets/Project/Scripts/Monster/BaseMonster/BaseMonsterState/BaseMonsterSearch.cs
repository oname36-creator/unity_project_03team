using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class BaseMonsterSearch :  IMonsterState
{

    private MonsterController _owner;
    private Animator _animator;

    private float _directionTimer;
    private readonly float _changeDirectionTime = 1.0f; // 1초마다 방향 전환

    // 생성자에서 owner를 직접 받도록 셋업
    public BaseMonsterSearch(MonsterController owner)
    {
        this._owner = owner;
        _animator = _owner.GetComponent<Animator>();
    
    }


    public void Enter() 
    {
        _owner.SetExclamationMark(false);
        _owner.SetQuestionMark(true);
        Debug.Log("대기");
        _animator.SetBool(AnimatorHash.Idle, true);
        _directionTimer = 0f;
    }

    public void Update() 
    {

        _directionTimer += Time.deltaTime;

        // 지정한 시간이 지나면 몬스터의 앞 방향을 반대로 뒤집음
        if (_directionTimer >= _changeDirectionTime)
        {
            _owner.Stop();
            _owner.Front = -_owner.Front;
            _directionTimer = 0f;
        }

    }

    public void Exit() 
    {
        //_animator.SetBool(AnimatorHash.Idle, false);
    }

}

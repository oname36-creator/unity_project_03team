using System.Threading;
using UnityEngine;

public class BaseMonsterAttack : IMonsterState
{
    private MonsterController _owner;
    private Animator _animator;
    private Transform _ownerTransform;

    private float _timer;

    // 애니메이션 작동 시간
    private readonly float _attackDuration = 1f;
    
    // 생성자에서 owner를 직접 받도록 셋업
    public BaseMonsterAttack(MonsterController owner)
    {
        this._owner = owner;
        _animator = _owner.GetComponent<Animator>();
        _ownerTransform = _owner.GetComponent<Transform>();
    }
    public void Enter() 
    {
        Debug.Log("공격");
        _owner.IsAttack = true;
        _animator.SetTrigger(AnimatorHash.IsAttack);
        _timer = 0f;
    }

    public void Update() 
    {
        _timer += Time.deltaTime;

        if (_timer >= _attackDuration)
        {
            FireBullet();
            _owner.IsAttack = false;
        }

    }

    public void Exit() 
    {
        _owner.IsAttack = false;
    }


    private void FireBullet()
    {
        GameObject bulletObj = ObjectPoolManager.Instance.MonsterBulletPop();

        if (bulletObj != null)
        {
            MonsterBullet bullet = bulletObj.GetComponent<MonsterBullet>();


            Vector2 myPos = _ownerTransform.position;

            Vector2 fireDir = _owner.GetMToP;

            myPos.y += 0.5f;
    
            // 총알 발사 (시작위치, 발사방향, 속도 8f)
            bullet.Launch(myPos, fireDir, 8f);
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BossController : MonoBehaviour
{


    #region Serialized Fields
    [Header("Monster Data")] // 인스펙터에 제목 표시
    public BaseMonsterData MonsterData;

    [Header("Player")]
    public GameObject Player;

    [Header("Monster Respawn")]
    public GameObject MonsterRespawner;


    #endregion


    #region Private Fields
    private int _hp;
    private int _damage;
    private int _searchRange; // 탐색 깊이
    private int _attackRange; // 공격 가능 거리
    private int _force; // 힘

    private float _angle;   // 탐색 각도
    private float _cosValue; // 각도의 cos 값
    private float _currentAngle = 0f;

    private float _speed;
    private float _maxSpeed; // 최대 속도

    private bool _isChase;
    private bool _isDead;
    private bool _isAttached;
    private bool _isGround;


    private Vector2 _frontVector;




    private MonsterStateMachine _monsterMachine;
    private Transform _transform;

    // 현재 촉수들이 찜한 타겟들을 모아두는 목록
    private HashSet<GameObject> _targetedObjects = new HashSet<GameObject>();


    #endregion


    public bool Chase
    {
        get { return _isChase; }
        set { _isChase = value; }
    }

    public bool Attack { get; set; } = false;

    public bool IsDead
    {
        get { return _isDead; }
        set { _isDead = value; }
    }

    public bool Attached
    {
        get { return _isAttached; }
        set  { _isAttached =  value; }
    }

    public float MaxSpeed 
    {
        get { return _maxSpeed; }
    }
    public float MoveSpeed
    {
        get { return _speed; }
    }

    public bool IsAttackTentacle { get; set; }

    public Vector3 Front 
    {
        get { return _frontVector; }
        set { _frontVector = value; }
    }


    public Transform Transform 
    {
        get { return  _transform; }
    }

    private void Awake()
    {
        _hp = MonsterData.hp;
        _damage = MonsterData.damage;
        _searchRange = MonsterData.searchRange;
        _attackRange = MonsterData.attackRange;
        _angle = MonsterData.angle;
        _speed = MonsterData.Speed;
        _maxSpeed = MonsterData.MaxSpeed;
        _force = MonsterData.Force;

        _frontVector = Vector3.right;

        IsAttackTentacle = false;
        _isDead = false;
        _isChase = true;
        _isGround = true;
        _isAttached = false;

        _transform = GetComponent<Transform>();
    }


    #region Unity Lifecycle
    void Start()
    {
        Application.targetFrameRate = 120;

        _monsterMachine = MonsterAiBrain.MakeMachine("Boss", this);
    }


    void Update()
    {
        if (_isDead) { return; }

        _monsterMachine.Update();

    }

    #endregion


    // 해당 오브젝트가 이미 다른 촉수에게 타겟팅 되었는지 확인
    public bool IsTargeted(GameObject obj)
    {
        return _targetedObjects.Contains(obj);
    }

    // 타겟 찜하기
    public void AddTarget(GameObject obj)
    {
        _targetedObjects.Add(obj);
    }

    // 타겟 놓아주기 (회수하거나 다 끌고왔을 때)
    public void RemoveTarget(GameObject obj)
    {
        if (_targetedObjects.Contains(obj))
        {
            _targetedObjects.Remove(obj);
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Chase) { return; }

        if (collision.CompareTag("PlayerAttack") || collision.CompareTag("Bullet"))
        {
            //isHurt = true;
            // 일단 float -> int로 


            _hp -= collision.GetComponent<PlayerAttack>().Damage;
            Debug.Log("Damage : " + collision.GetComponent<PlayerAttack>().Damage + " hp : " + _hp);

            if (_hp <= 0)
            {
                IsDead = true;
            }


        }

    }




}
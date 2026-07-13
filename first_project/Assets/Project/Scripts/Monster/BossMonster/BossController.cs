using UnityEngine;
using System.Collections;


public class BossController : MonoBehaviour
{


    #region Serialized Fields
    [Header("Monster Data")] // 인스펙터에 제목 표시
    public BaseMonsterData MonsterData;


 
 

    #endregion


    #region Private Fields
    private int _hp;
    private int _damage;
    private int _searchRange; // 탐색 깊이
    private int _attackRange; // 공격 가능 거리
    private int _force; // 힘
    private int _nodeCount;

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

    private Vector3[] _ableTargetVectors;



    private MonsterStateMachine _monsterMachine;
    private Transform _transform;
    private Transform _targetTransform;

    #endregion


    public bool Chase
    {
        get { return _isChase; }
        set { _isChase = value; }
    }
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


    public Vector2 TargetVector 
    {
        get;
        private set;
    }


    public Transform Transform 
    {
        get { return  _transform; }
    }
    public Transform Target
    {
        get { return _targetTransform; }
        set 
        {
            _targetTransform = value;
            CalculateTargetPointVector(_targetTransform.position);
        }
    }


    #region Unity Lifecycle
    void Start()
    {
        Application.targetFrameRate = 120;

        _hp = MonsterData.hp;
        _damage = MonsterData.damage;
        _searchRange = MonsterData.searchRange;
        _attackRange = MonsterData.attackRange;
        _angle = MonsterData.angle;
        _speed = MonsterData.Speed;
        _maxSpeed = MonsterData.MaxSpeed;
        _force = MonsterData.Force;

        _frontVector.x = 1;

        _isDead = false;
        _isChase = true;
        _isGround = true;
        _isAttached = false;

        _transform = GetComponent<Transform>();



        _ableTargetVectors = new Vector3[_nodeCount];

        for (int i = 0; i < _nodeCount; i++)
        {
            float randomXAngle = Random.Range(-_angle, _angle);
            Quaternion spreadRotation = Quaternion.Euler(randomXAngle, 0f, 0f);
            _ableTargetVectors[i] = spreadRotation * Vector3.forward;
        }

        _monsterMachine = MonsterAiBrain.MakeMachine("Boss", this);
    }


    void Update()
    {
        if (_isDead) { return; }

        _monsterMachine.Update();

    }

    #endregion

    private void CalculateTargetPointVector(Vector3 targetPoint)
    {
        TargetVector = (targetPoint - _transform.position).normalized;
    }





}
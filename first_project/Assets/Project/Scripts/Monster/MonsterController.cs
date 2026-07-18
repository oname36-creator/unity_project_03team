using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;



public class MonsterController : MonoBehaviour 
{

    #region Serialized Fields
    [Header("Monster Data")] // 인스펙터에 제목 표시
    public BaseMonsterData MonsterData;

    [Header("Player Object")]
    public GameObject Player;


    [Header("Obstacle Layer")]
    public LayerMask _obstacleLayer;

    [Header("Mark Object")]
    public GameObject QuestionMark;
    public GameObject ExclamationMark;




    #endregion


    #region Private Fields
    private int _hp;
    private int _damage;
    private int _searchRange; // 탐색 깊이
    private int _attackRange; // 공격 가능 거리
    private int _force; // 힘

    private float _angle;   // 탐색 각도
    private float _cosValue; // 각도의 cos 값

    private float _speed;
    private float _maxSpeed; // 최대 속도

    private bool isDead;
    //private bool isFounded;
    private bool isAttack;
    private bool isAttackable;
    private bool isFly;
    private bool isBack;
    private bool isHurt;
    private bool isCollision;

    // 뒤집기 bool
    private bool onFlip = true;

    private string _name;

    private Vector2 _frontVector;  // 앞 방향 저장 // (1,0)이면 오른쪽 (-1,0)이면 왼쪽
    private Vector2 _mToPlayer;
    private Vector2 _mToPlayerDistance; // 거리
    private Vector2 _attackStartPoint;

    private Rigidbody2D _rigidBody2D;
    private SpriteRenderer _renderer;

    private MonsterStateMachine _monsterMachine;

    private Transform _playerTransform;

    private float _playerRadius;

    private Transform _monsterTransform;

    #endregion

    #region Properties

    public string Name 
    {
        get { return _name; }
    }
    public float Speed
    {
        get { return _speed; }
    }

    public float MaxSpeed
    {
        get { return _maxSpeed; }
    }
    public float SearchRange
    {
        get { return _searchRange; }
    }

    public float CosValue
    {
        get { return _cosValue; }
    }

    public bool IsAttack
    {
        get { return isAttack; }
        set { isAttack = value; }
    }
    public bool IsAttackable
    {
        get { return isAttackable; }
        set { isAttackable = value; }
    }

    public bool IsDead
    {
        get { return isDead; }
        set 
        { 
            isDead = value;
        }
    }

    public bool IsHurt
    {
        get { return isHurt; }
        set { isHurt = value; }
    }

    public bool IsFly
    {
        get { return isFly; }
        set { isFly = value; }
    }

    public bool IsBack
    {
        get { return isBack; }
        set { isBack = value; }
    }

    public bool IsCollision
    {
        get { return isCollision; }
        set { isCollision = value; }
    }


    public bool InRange  // Range 안에 있을때
    {
        get
        {
            return _mToPlayerDistance.magnitude < _searchRange;
        }

    }

    public bool InAngle
    {
        get
        {
            return (Vector2.Dot(_frontVector, _mToPlayer) > _cosValue && !CheckForObstacles());
        }
    }



    public bool InAttackRange  // AttackRange 안에 있을때
    {
        get
        {
            return _mToPlayerDistance.magnitude < _attackRange;
        }

    }


    public Vector2 Front
    {
        get { return _frontVector; }
        set
        {
            if (_frontVector == value) return;  // 방향이 바뀌지 않았다면 리턴
            onFlip = !onFlip;                  // 방향이 바뀌면 true -> false,  false -> true로 바꾸고 
            _renderer.flipX = onFlip;          // flip 해주기
            _frontVector = value;
        }
    }
    public Vector2 GetMToP // 몬스터에서 플레이어 방향의 유닛 벡터 Get
    {
        get { return _mToPlayer; }
    }

    public Vector2 GetMToPDistance // 몬스터에서 플레이어 방향의 유닛 벡터 Get
    {
        get { return _mToPlayerDistance; }
    }

    public Vector2 GetPlayerPos 
    {
        get { return _playerTransform.position; }
    }

    public Vector2 AttackStartPoint 
    {
        get { return _attackStartPoint; }
        set { _attackStartPoint = value;}
    }


    #endregion

    #region Unity Lifecycle

    private void OnEnable()
    {
        if (gameObject.CompareTag("Untagged"))
        {
            gameObject.tag = "Monster";
            gameObject.layer = LayerMask.GetMask("Monster");
        }
        if (!onFlip) 
        {
            onFlip = true;
        }
        _hp = MonsterData.hp;

        isDead = false;
        //isFounded = false;
        isAttack = false;
        isAttackable = true;
        isHurt = false;
        isBack = false;
    }





    void Start()
    {
        // 1초에 120번만 계산되도록
        Application.targetFrameRate = 120;

        _damage = MonsterData.damage;
        _searchRange = MonsterData.searchRange;
        _attackRange = MonsterData.attackRange;
        _angle = MonsterData.angle;
        _speed = MonsterData.Speed;
        _maxSpeed = MonsterData.MaxSpeed;
        _force = MonsterData.Force;
        _name = MonsterData.Name;



        _cosValue = Mathf.Cos(_angle * Mathf.Deg2Rad);
        _rigidBody2D = this.GetComponent<Rigidbody2D>();
        _renderer = GetComponent<SpriteRenderer>();
        _frontVector = new Vector2Int(-1, 0);
        onFlip = true;
        _renderer.flipX = onFlip;


        _playerTransform = Player.GetComponent<Transform>();
        _playerRadius = Player.GetComponent<CapsuleCollider2D>().size.y/2;
        _monsterTransform = this.GetComponent<Transform>();

        _monsterMachine = MonsterAiBrain.MakeMachine(_name, this);


        SetExclamationMark(false);
        SetQuestionMark(true);

    }

    void Update()
    {

        // 죽었다면
        
        CaculateMonsterToPlayerVector();
        _monsterMachine.Update();


    }

    #endregion


    public void Move(Vector2 dir, bool Impulse = false, bool Fly = false)
    {
        if (Fly)
        {
            _rigidBody2D.AddForce(Vector2.up * 9.81f, ForceMode2D.Force);
            return;
        }

        if (Impulse)
        {
            _rigidBody2D.AddForce(dir * _speed, ForceMode2D.Impulse);
        }
        else
        {
            //Debug.Log("dir * _speed : " + dir * _speed);
            _rigidBody2D.AddForce(dir * _speed, ForceMode2D.Force);
        }

        if (_rigidBody2D.linearVelocity.magnitude > _maxSpeed)
        {
            _rigidBody2D.linearVelocity = _rigidBody2D.linearVelocity.normalized * _maxSpeed;
        }
    }

    public void MoveToPosition(Vector2 dir) 
    {
        _rigidBody2D.MovePosition(dir);
        //Debug.Log("current Pos:" + _rigidBody2D.position);
    }


    public void Stop()
    {
        _rigidBody2D.linearVelocity = Vector2.zero;
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerAttack") || collision.CompareTag("Bullet"))
        {
           
            // 일단 float -> int로 


            _hp -= collision.GetComponent<PlayerAttack>().Damage;
            Debug.Log("Damage : " + collision.GetComponent<PlayerAttack>().Damage + " hp : " + _hp);

            if (_hp <= 0)
            {
                IsDead = true;
                Debug.Log("isDead :" + IsDead);
            }
            else
            {
                isHurt = true;
            }
        }

        if (collision.CompareTag("Boss")) 
        {
            IsDead = true;
        }

    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    // 벽 부딪힘 체크
    //    if (collision.gameObject.CompareTag("Untagged")) 
    //    {
    //        isHurt = true;
    //    }
    //}

    private void CaculateMonsterToPlayerVector()
    {
        // 플레이어의 몸통(중심) 높이로 타겟 지정
        Vector2 playerPos = _playerTransform.position;
        // playerPos.y -= _playerRadius; // <-- 이 줄은 삭제하세요!
        playerPos.y += 0.5f; // 플레이어 중심 높이에 맞게 적절한 값을 더해줍니다.

        // 몬스터의 눈(중심) 높이로 시작점 지정
        Vector2 myPos = _monsterTransform.position;
        myPos.y += 0.5f; // 몬스터 중심 높이에 맞게 적절한 값을 더해줍니다. (이전 코드의 origin.y += 1 역할을 여기서 해줍니다)

        _mToPlayerDistance = playerPos - myPos;
        _mToPlayer = _mToPlayerDistance.normalized;
    }

    public bool CheckForObstacles()
    {
        // 1. 몬스터의 시야 시작점 (눈 높이)
        Vector2 origin = transform.position;
        origin.y += 0.5f;

        // 2. 플레이어의 머리와 발 위치 계산
        Vector2 playerPos = _playerTransform.position;

        // pivot(중심점)이 캐릭터 중앙에 있다고 가정할 때:
        Vector2 headTarget = playerPos;
        headTarget.y += _playerRadius; // 머리 위치 (위로)

        Vector2 feetTarget = playerPos;
        feetTarget.y -= _playerRadius; // 발 위치 (아래로)

        // 3. 머리와 발을 향하는 각각의 방향 벡터 계산
        Vector2 dirToHead = (headTarget - origin).normalized;
        Vector2 dirToFeet = (feetTarget - origin).normalized;

        // 4. 레이캐스트 발사
        RaycastHit2D hitHead = Physics2D.Raycast(origin, dirToHead, _searchRange, _obstacleLayer);
        RaycastHit2D hitFeet = Physics2D.Raycast(origin, dirToFeet, _searchRange, _obstacleLayer);

        Debug.DrawRay(origin, dirToHead * _searchRange, Color.yellow); // 머리: 노란색
        Debug.DrawRay(origin, dirToFeet * _searchRange, Color.red);    // 발: 빨간색

        bool isHeadBlocked = hitHead.collider != null && hitHead.collider.CompareTag("Ground");
        bool isFeetBlocked = hitFeet.collider != null && hitFeet.collider.CompareTag("Ground");

        return isHeadBlocked && isFeetBlocked;
    }

    public void SetQuestionMark(bool active)
    {
        QuestionMark.SetActive(active);
    }

    public void SetExclamationMark(bool active)
    {
        ExclamationMark.SetActive(active);
    }

    public void OnDieAnimationEnd()
    {
        Debug.Log("리턴 함수");
        ObjectPoolManager.Instance.MonsterPush(this.gameObject);
    }



}

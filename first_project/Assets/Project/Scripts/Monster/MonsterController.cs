using UnityEngine;

public enum Status
{
    None,
    Idle,
    Chase,
    Attack

}




public class MonsterController : MonoBehaviour
{

    #region Serialized Fields
    [Header("Monster Data")] // 인스펙터에 제목 표시
    public BaseMonsterData monsterData;

    [Header("Player")]
    //public PlayerController player;

    [Header("Interface")]
    public IMonster Interface;
    #endregion


    #region Private Fields
    private int _hp;
    private int _damage;
    private int _searchRange; // 탐색 깊이
    private int _force; // 힘

    private float _angle;   // 탐색 각도
    private float _cosValue; // 각도의 cos 값

    private float _maxSpeed; // 최대 속도

    private bool isDead;
    private bool isfounded;

    // 뒤집기 bool
    private bool onFlip;

    // 몬스터의 상태 저장
    private Status _Estatus;
    // 몬스터의 이전 상태
    private Status _EpreStatus;

    private Vector2Int _frontVector;  // 앞 방향 저장 // (1,0)이면 오른쪽 (-1,0)이면 왼쪽
    private Vector2 _mToPlayer;

    private Rigidbody2D _rigidBody2D;
    private SpriteRenderer _renderer;

    #endregion

    #region Properties

    public float MaxSpeed
    {
        get { return _maxSpeed; }
    }

    public bool IsDead
    {
        get { return isDead; }
    }

    public Vector2Int Front
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

    public float SearchRange
    {
        get { return _searchRange; }
    }

    public float CosValue
    {
        get { return _cosValue; }
    }

    public Status State
    {
        get { return _Estatus; }
        set { _Estatus = value; }
    }

    #endregion

    #region Unity Lifecycle
    void Start()
    {
        // 1초에 120번만 계산되도록
        Application.targetFrameRate = 120;

        _hp = monsterData.hp;
        _damage = monsterData.damage;
        _searchRange = monsterData.searchRange;
        _angle = monsterData.angle;
        _maxSpeed = monsterData.MaxSpeed;
        isDead = false;
        isfounded = false;
        _Estatus = Status.Idle;
        _EpreStatus = Status.None;
        _cosValue = Mathf.Cos(_angle * Mathf.Deg2Rad);
        _rigidBody2D = this.GetComponent<Rigidbody2D>();
        _renderer = GetComponent<SpriteRenderer>();
        _frontVector = new Vector2Int(-1, 0);
        onFlip = true;
        _renderer.flipX = onFlip;

    }

    void Update()
    {

        // 죽었다면
        if (isDead)
        {
            gameObject.SetActive(false);
            return;
        }

        // 이전 상태와 동일하다면
        if (_Estatus == _EpreStatus) { return; }


        // 상태에 따른 행동
        switch (_Estatus)
        {
            case Status.Idle:
                Interface.Search();
                break;

            case Status.Chase:
                Interface.Chase();
                break;

            case Status.Attack:
                Interface.Attack();
                break;

        }
        // 갱신
        _EpreStatus = _Estatus;


    }

    #endregion

    // 직선 이동만 가능
    public void OnForce(Vector2Int dir, float wantSpeed)
    {
        Vector2 velocity = _rigidBody2D.linearVelocity;

        if (Mathf.Abs(wantSpeed) > _maxSpeed) { return; }

        if (_frontVector.x < 0.1f && velocity.x > 0.1f)
        {
            this.Front = new Vector2Int(1, 0); // 방향이 오른쪽
        }
        else if (_frontVector.x > 0.1f && velocity.x < 0.1f)
        {
            this.Front = new Vector2Int(-1, 0); // 방향이 왼쪽
        }

        // 방향이 x방향이고 원하는 속도에 도달했으면
        if (dir.y == 0)
        {
            if (velocity.x < wantSpeed + 1 && velocity.x > wantSpeed - 1)
            {
                _rigidBody2D.linearVelocityX = wantSpeed * dir.x;
                return;
            }

        }
        else
        {
            if (velocity.y < wantSpeed + 1 && velocity.y > wantSpeed - 1)
            {
                _rigidBody2D.linearVelocityY = wantSpeed * dir.y;
                return;
            }
        }

        _rigidBody2D.AddForce(dir * _force, ForceMode2D.Force);
    }


    public bool Stop()
    {
        if (_rigidBody2D.linearVelocityX == 0 || _rigidBody2D.linearVelocityY == 0)
            return true;
        else return false;

    }





    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Attack")
        {
            //_hp -= collision.GetComponent<PlayerController>().GetDamage();
            if (_hp <= 0)
            {
                Interface.Dead();
                isDead = true;
            }
            else
            {
                Interface.Hurt();
            }

        }
    }

    private void CaculateMonsterToPlayerVector()
    {
        // 플레이어 좌표를 받아서 
        // 몬스터의 위치에서 플레이어 좌표의 유닛 벡터를 구하고
        // _mToPlayer에 저장하기
    }

}

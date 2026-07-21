using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BodyController : MonoBehaviour
{
    [Header("Floating Movement")]
    public float SineAmplitude = 2f;   // 사인파 이동 시 상하 진폭
    public float SineFrequency = 3f;   // 사인파 이동 시 진동 주기(속도)

    [Header("Idle Hovering")]
    public float HoverAmplitude = 0.5f; // 대기 상태일 때 둥둥거리는 진폭
    public float HoverFrequency = 2f;   // 대기 상태일 때 둥둥거리는 주기

    [Header("Boss")]
    public BossController Boss;
    public float ReleaseDistance = 3f;
    

    private bool _isDead = false;
    private int _phase = 1;

    private Rigidbody2D _rigidbody2D;
    private MonsterStateMachine _monsterMachine;
    private Transform _ownerTransform;
    private Transform _playerTransform;
    private SpriteRenderer _sprite;

    

    public BossController GetBoss { get { return Boss; } }

    public bool Move { get; set; }
    public bool Chase { get { return Boss.Chase; } }

    public bool Create { get; set; }
    public bool Throw { get; set; } = false;

    public float MoveSpeed 
    {
        get {  return Boss.MoveSpeed; }   
    }

    public float YCenter { get; private set;  }

    public int TentacleCycle { get; set; }

    public int ThrowCycle { get; set; } = 10;

    public int Phase { get { return _phase; } }

    public float Distance 
    {
        get 
        {
            return _playerTransform.position.x - _ownerTransform.position.x;
        }
    }


    private void OnEnable()
    {
        // 이벤트 구독
        if (Boss != null)
        {
            Boss.OnPhaseChanged += HandlePhaseChanged;
        }
    }

    private void OnDisable()
    {
        if (Boss != null)
        {
            Boss.OnPhaseChanged -= HandlePhaseChanged;
        }
    }

    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();

        _monsterMachine = MonsterAiBrain.MakeMachine("BossBody", this);
        _ownerTransform = GetComponent<Transform>();
        _sprite = GetComponent<SpriteRenderer>();
        _playerTransform = Boss.Player.transform;

        YCenter = _ownerTransform.position.y;
        //Move = false;
        Move = true;
        Create = false;
        TentacleCycle = 7;
        //Create = true;
    }

    void Update()
    {
        if (_isDead) { return; }
        _monsterMachine.Update();
    }

    public void OnFlip(bool flip)
    {
        _sprite.flipY = flip;
    }

    public void Stop() 
    {
        _rigidbody2D.linearVelocity = Vector3.zero;
    }


    private void HandlePhaseChanged(int newPhase)
    {
        _phase = newPhase;
        if (newPhase == 2) 
        {
            TentacleCycle = 5;
        }


    }

}
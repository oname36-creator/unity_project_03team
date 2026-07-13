using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


public class BodyController : MonoBehaviour
{


    [Header("Body Movement")]
    public float PullForce = 50f;
    public float MaxGrappleDistance = 15f;
    public float ReleaseDistance = 3f;

    [Header("Boss")]
    public BossController Boss;


    private bool _isDead = false;
    private bool _isGround = false;

    private Vector2 _targetVector;

    private Rigidbody2D _rigidbody2D;
    private MonsterStateMachine _monsterMachine;
    private Transform _ownerTransform;
    private Transform _targetTransform;
    private SpriteRenderer _sprite;

    public BossController GetBoss 
    {
        get { return Boss; }
    }

    public bool Move 
    {
        get; 
        set; 
    }

    public bool Chase
    {
        get { return Boss.Chase; }
    }

    public bool IsGround 
    {
        get { return  _isGround; }
    }

    


    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _monsterMachine = MonsterAiBrain.MakeMachine("BossBody", this);
        _ownerTransform = GetComponent<Transform>();
        _sprite = GetComponent<SpriteRenderer>();

        Move = false;
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

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") && !_isGround)
        {
            _ownerTransform.up = collision.GetContact(0).normal;
            _isGround = true;
        }
        if (collision.gameObject.GetComponent<Transform>() == Boss.Target && Boss.Attached)
        {
            Move = false;
            Boss.Attached = false;

        }
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") && _isGround)
        {
            _ownerTransform.up = Boss.TargetVector;
            _isGround = false;
        }
    }




}
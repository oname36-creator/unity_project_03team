using System.Collections;
using UnityEngine;


public class MonsterBullet : MonoBehaviour
{


    private Vector2 _startPos;
    private Vector2 _moveDir;
    private float _speed;

    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private Coroutine _moveCoroutine;


    public void OnEnable()
    {
        if(_animator != null) 
        {
            _animator.SetBool(AnimatorHash.Idle, true);
        }
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Start()
    {
    }



    public void Launch(Vector2 startPosition, Vector2 direction, float speed)
    {
        _startPos = startPosition;
        _moveDir = direction.normalized; 
        _speed = speed;

        if (direction.x < 0)
        {
            _spriteRenderer.flipX = true;
        }
        else 
        {
            _spriteRenderer.flipX = false;
        }

        transform.position = _startPos;

        _moveCoroutine = StartCoroutine(MoveRoutine());
    }

    private IEnumerator MoveRoutine()
    {
        // 화면 밖으로 무한히 날아가는 것을 방지하기 위해 최대 5초 뒤 자동 소멸 
        float liveTime = 5f;
        float timer = 0f;

        while (timer < liveTime)
        {
            transform.Translate(_moveDir * _speed * Time.deltaTime, Space.World);

            timer += Time.deltaTime;
            yield return null;
        }


        ObjectPoolManager.Instance.MonsterBulletPush(this.gameObject);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster"))
        {
            return;
        }

        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
        }

        SoundManager.Instance.PlaySFX("SlimBoom");
        _animator.SetBool(AnimatorHash.Idle, false);
        _animator.SetTrigger(AnimatorHash.IsAttack);
    }

    public void OnBulletAnimationDone() 
    {
        ObjectPoolManager.Instance.MonsterBulletPush(this.gameObject);
    }
}
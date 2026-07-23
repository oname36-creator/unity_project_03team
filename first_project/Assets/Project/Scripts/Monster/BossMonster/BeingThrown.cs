using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody2D))]
public class BeingThrown : MonoBehaviour
{
    [Header("Warning Effect")]
    [SerializeField] private GameObject _warringObject;

    [Header("Throw Settings")]
    [SerializeField] private float _throwDistance = 20f;
    [SerializeField] private float _chargeTime = 3f;
    [SerializeField] private float _throwPower = 15f;


    [Header("Name")]
    [SerializeField] private string _name;

    [Header("Sound Key")]
    [SerializeField] private string boomKey;


    [SerializeField]
    private CinemachineImpulseSource _impulseSource;

    private Rigidbody2D _rigidBody;
    private SpriteRenderer _warringRenderer;
    private SpriteRenderer _myRenderer;

    private Coroutine _fadeCoroutine;

    private bool _isThrown = false;

    private bool _first = true;

    private float _localScaleY;
    private float _fadeDuration = 3f;

    
    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _myRenderer = GetComponent<SpriteRenderer>();


        if (_myRenderer != null)
        {
            _localScaleY = _warringObject.transform.localScale.y;
        }

    }


    void OnEnable()
    {
        _isThrown = false;
        _first = true;


        if (_rigidBody != null)
        {
            _rigidBody.linearVelocity = Vector2.zero; 
            _rigidBody.angularVelocity = 0f;
            _rigidBody.bodyType = RigidbodyType2D.Kinematic;
            _rigidBody.gravityScale = 1;
        }
    }

    private void Start()
    {

    }


    void Update()
    {
        if (_isThrown && _rigidBody.linearVelocity.sqrMagnitude <= 0.01f)
        {
            ReturnToPool();
        }
    }
    void LateUpdate()
    {
        if (_warringObject != null && _warringObject.activeSelf)
        {
            _warringObject.transform.rotation = Quaternion.identity;
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_first)
        {
            if (_impulseSource != null)
            {
                _impulseSource.GenerateImpulse();
            }

            SoundManager.Instance.PlaySFX(boomKey);
            _first = false;
            Vector2 contactPoint = collision.GetContact(0).point;

            SetUpEffect(contactPoint);
        }
    }

    public void InitializeThrow(Vector2 targetPos)
    {
        Vector2 startDir = new Vector2(-1f, 1f).normalized;
        transform.position = targetPos + (startDir * _throwDistance);

        SetupWarringEffect(targetPos);

        StartCoroutine(ChargeAndThrowRoutine(targetPos, _chargeTime));
    }

    private void SetUpEffect(Vector2 position) 
    {

        GameObject smokeBurst = ObjectPoolManager.Instance.SmokeBurstEffectPop();
        if (smokeBurst != null)
        {
            smokeBurst.transform.position = position;
        }

        int smokeCount = 6;
        float spreadRadius = 1.0f; 

        for (int i = 0; i < smokeCount; i++)
        {
            GameObject dustSmoke = ObjectPoolManager.Instance.SmokeEffectPop();
            if (dustSmoke != null)
            {
                Vector2 randomOffset = new Vector2(Random.Range(-spreadRadius, spreadRadius), Random.Range(0f, 0.5f));
                dustSmoke.transform.position = position + randomOffset;
            }
        }

    }



    private void SetupWarringEffect(Vector2 targetPos)
    {
        if (_warringObject == null) return;

        _warringObject.SetActive(true);
        _warringObject.transform.position = new Vector2(targetPos.x, targetPos.y + _localScaleY);
        _warringObject.transform.rotation = Quaternion.identity;

        if (_warringRenderer != null)
        {
            Color color = _warringRenderer.color;
            color.a = 0f;
            _warringRenderer.color = color;

            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = StartCoroutine(FadeInCoroutine());
        }
    }

    private void ThrowTo(Vector2 targetPos)
    {
        if (_warringObject != null)
        {
            _warringObject.SetActive(false);
        }


        _rigidBody.bodyType = RigidbodyType2D.Dynamic;

        Vector2 startPos = transform.position;
        Vector2 distance = targetPos - startPos;

        float time = Mathf.Max(distance.magnitude / _throwPower, 0.5f);

        float gravity = Physics2D.gravity.y * _rigidBody.gravityScale;

        float velocityX = distance.x / time;
        float velocityY = (distance.y - 0.5f * gravity * time * time) / time;

        _rigidBody.linearVelocity = new Vector2(velocityX, velocityY);

        _isThrown = true;
    }

    private void ReturnToPool()
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = null;
        }
        ObjectPoolManager.Instance.ThrownPush(this.gameObject);
    }


    private IEnumerator ChargeAndThrowRoutine(Vector2 targetPos, float chargeTime)
    {
        float elapsed = 0f;

        if (_warringRenderer != null)
        {
            Color color = _warringRenderer.color;
            while (elapsed < chargeTime)
            {
                elapsed += Time.deltaTime;
                color.a = Mathf.Lerp(0f, 1f, elapsed / chargeTime);
                _warringRenderer.color = color;
                yield return null;
            }
            color.a = 1f;
            _warringRenderer.color = color;
        }
        else
        {
            yield return new WaitForSeconds(chargeTime);
        }

        ThrowTo(targetPos);
    }

    private IEnumerator FadeInCoroutine()
    {
        float elapsedTime = 0f;
        Color color = _warringRenderer.color;

        while (elapsedTime < _fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            color.a = Mathf.Lerp(0f, 0.75f, elapsedTime / _fadeDuration);
            _warringRenderer.color = color;

            yield return null; 
        }

        _warringRenderer.color = color;
    }
}
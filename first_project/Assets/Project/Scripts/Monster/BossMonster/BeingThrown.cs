using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BeingThrown : MonoBehaviour
{
    [Header("Warning Effect")]
    [SerializeField] private GameObject _warringObject;

    [Header("Throw Settings")]
    [SerializeField] private float _throwDistance = 20f;
    [SerializeField] private float _chargeTime = 3f;
    [SerializeField] private float _throwPower = 15f;


    [Header("Audio Clip")]
    [SerializeField] private AudioClip _startAudio;
    [SerializeField] private AudioClip _boomAudio;

    [Header("Name")]
    [SerializeField] private string _name;

    private Rigidbody2D _rigidBody;
    private SpriteRenderer _warringRenderer;
    private SpriteRenderer _myRenderer;


    private bool _isThrown = false;

    private float _localScaleY;

    private string startKey;
    private string boomKey;

    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _myRenderer = GetComponent<SpriteRenderer>();


        if (_myRenderer != null)
        {
            _warringObject.transform.localScale = _myRenderer.bounds.size / 4;
            _localScaleY = _warringObject.transform.localScale.y;
        }

    }


    void OnEnable()
    {
        _isThrown = false;

      
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
        startKey = _name + "startKey";
        boomKey = _name + "boomKey";

        SoundManager.Instance.AddSfx(startKey,_startAudio ,0.3f);
        SoundManager.Instance.AddSfx(boomKey, _boomAudio ,0.5f);

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
        SoundManager.Instance.PlaySFX(boomKey);
        _rigidBody.gravityScale *= 2;
    }

    public void InitializeThrow(Vector2 targetPos)
    {
        Vector2 startDir = new Vector2(-1f, 1f).normalized;
        transform.position = targetPos + (startDir * _throwDistance);

        SetupWarringEffect(targetPos);

        StartCoroutine(ChargeAndThrowRoutine(targetPos, _chargeTime));
    }

    private void SetupWarringEffect(Vector2 targetPos)
    {
        if (_warringObject == null) return;

        _warringObject.SetActive(true);
        _warringObject.transform.position = new Vector2 (targetPos.x, targetPos.y + _localScaleY);

        _warringObject.transform.rotation = Quaternion.identity;

        if (_warringRenderer != null)
        {
            Color color = _warringRenderer.color;
            color.a = 0f;
            _warringRenderer.color = color;
        }
    }

    private void ThrowTo(Vector2 targetPos)
    {
        if (_warringObject != null)
        {
            _warringObject.SetActive(false);
        }

        SoundManager.Instance.PlaySFX(startKey);

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

}
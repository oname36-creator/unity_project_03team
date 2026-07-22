using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class TentacleUp : IMonsterState
{
    private TentacleController _owner;
    private Transform _bossTransform;

    private SpriteRenderer _warningEffect;

    private Transform _playerTransform;

    private Camera _camera;

    // 3 타이머를 위한 변수
    private float _timer;
    private float _duration = 3f;


    private Vector2 _targetOffset;

    private float _playerRadius;

    // 움직임 보간을 위한 위치 변수
    private Vector2 _startOffset;
    private Vector2 _targetPos;

    private Vector2 _prevPlayerPos;
    private Vector2 _velocity = Vector2.zero; // SmoothDamp의 내부 속도 계산용
    private float _followSmoothTime = 0.1f;  // 따라가는 딜레이 (값이 클수록 무겁게/느리게 따라감)

    private float _targetAlpha = 0.5f;

    private float _CameraWidth;
    private float _orthoSize;
    private float length;
    float facingDir;

    public TentacleUp(TentacleController owner)
    {
        _owner = owner;
        _playerTransform = _owner.Boss.Player.transform;
        _playerRadius = _owner.Boss.Player.GetComponent<CapsuleCollider2D>().size.y / 2;

        _bossTransform = _owner.Boss.transform;

        _camera = _owner.Boss.Camera;

        _orthoSize = _camera.orthographicSize;
        _CameraWidth = _orthoSize * _camera.aspect;

        _warningEffect = _owner.warningEffectRenderer_1;
    }

    public void Enter()
    {
        _owner.segmentDistance = 0.5f;
        _owner.Target = null;
        _owner.Attack = false; // 시작할 때 Attack 상태 초기화

        _timer = 0f;

        length = _owner.TentacleLength;
        facingDir = Mathf.Sign(_owner.transform.lossyScale.x);

        _targetOffset = new Vector2(length * -0.3f * facingDir, length * 0.7f);

        _startOffset = _owner.IkTargetPosition - (Vector2)_owner.tentacleRoot.position;

        // 시작 시점의 플레이어 위치 저장
        _prevPlayerPos = _playerTransform.position;

        //Debug.Log("TentacleUp");

        if (_warningEffect != null)
        {
            // 이펙트를 처음 플레이어 위치(_prevPlayerPos)에 둡니다.
            _warningEffect.transform.position = _prevPlayerPos;
            _warningEffect.gameObject.SetActive(true);

            Color color = _warningEffect.color;
            color.a = 0f;
            _warningEffect.color = color;
        }
    }

    public void Update()
    {
        _timer += Time.deltaTime;

        // 1. 5초 동안 목표 위치를 향해 부드럽게 이동 (0 ~ 1 사이의 비율)
        float t = Mathf.Clamp01(_timer / _duration);

        float invT = 1f - t;
        float easeOutT = 1f - (invT * invT * invT);


        _targetPos = (Vector2)_owner.tentacleRoot.position + _targetOffset;

        Vector2 currentLerpOffset = Vector2.Lerp(_startOffset, _targetOffset, easeOutT);


        _owner.IkTargetPosition = (Vector2)_owner.tentacleRoot.position + currentLerpOffset;

        // 4. 이펙트 위치 및 페이드인(알파값) 동시 갱신
        if (_warningEffect != null && _warningEffect.gameObject.activeSelf)
        {
            float posX = _prevPlayerPos.x - _CameraWidth * 0.8f;
            Vector2 EffecetPos = new Vector2(posX, EffectPosition(posX));


            _warningEffect.transform.position = Vector2.SmoothDamp(
                _warningEffect.transform.position,
                EffecetPos,
                ref _velocity,
                _followSmoothTime
            );

            Color color = _warningEffect.color;
            color.a = Mathf.Lerp(0f, _targetAlpha, t);
            _warningEffect.color = color;

            _prevPlayerPos = _playerTransform.position;
            _prevPlayerPos.y -= _playerRadius;
        }

        // 5초가 경과하면 Attack 상태를 true로 전환
        if (_timer >= _duration)
        {
            _owner.Attack = true;
            //Debug.Log("5초 경과! 촉수 공격 준비 완료 (Attack = true)");


        }
    }

    public void Exit()
    {
        SoundManager.Instance.PlaySFX("BossAttack");

        if (_warningEffect != null)
        {
            _warningEffect.gameObject.SetActive(false);
        }
    }

    private float EffectPosition(float x) 
    {
        float gradient = (_prevPlayerPos.y - _bossTransform.position.y + 2f)/(_prevPlayerPos.x - _bossTransform.position.x); 

        return gradient * (x - _bossTransform.position.x) + _bossTransform.position.y + 2f;

    }




}
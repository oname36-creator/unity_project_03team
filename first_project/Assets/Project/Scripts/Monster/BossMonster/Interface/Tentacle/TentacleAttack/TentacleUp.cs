using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class TentacleUp : IMonsterState
{
    private TentacleController _owner;
    private Transform _playerTransform;

    // 5초 타이머를 위한 변수
    private float _timer;
    private float _duration = 5f;

    private float _playerRadius;

    // 움직임 보간을 위한 위치 변수
    private Vector2 _startOffset;
    private Vector2 _targetPos;

    // [추가] 잔상 추적을 위한 변수
    private Vector2 _prevPlayerPos;
    private Vector2 _velocity = Vector2.zero; // SmoothDamp의 내부 속도 계산용
    private float _followSmoothTime = 0.15f;  // 따라가는 딜레이 (값이 클수록 무겁게/느리게 따라감)

    private float _targetAlpha = 0.5f;

    public TentacleUp(TentacleController owner)
    {
        _owner = owner;
        _playerTransform = _owner.Boss.Player.transform;
        _playerRadius = _owner.Boss.Player.GetComponent<CapsuleCollider2D>().size.y/2;
    }

    public void Enter()
    {
        _owner.segmentDistance = 0.5f;
        _owner.Target = null;
        _owner.Attack = false; // 시작할 때 Attack 상태 초기화

        _timer = 0f;

        _startOffset = _owner.IkTargetPosition - (Vector2)_owner.tentacleRoot.position;

        // 시작 시점의 플레이어 위치 저장
        _prevPlayerPos = _playerTransform.position;

        Debug.Log("TentacleUp");

        if (_owner.warningEffectRenderer != null)
        {
            // 이펙트를 처음 플레이어 위치(_prevPlayerPos)에 둡니다.
            _owner.warningEffectRenderer.transform.position = _prevPlayerPos;
            _owner.warningEffectRenderer.gameObject.SetActive(true);

            Color color = _owner.warningEffectRenderer.color;
            color.a = 0f;
            _owner.warningEffectRenderer.color = color;
        }
    }

    public void Update()
    {
        _timer += Time.deltaTime;

        // 1. 5초 동안 목표 위치를 향해 부드럽게 이동 (0 ~ 1 사이의 비율)
        float t = Mathf.Clamp01(_timer / _duration);


        float easeOutT = 1f - Mathf.Pow(1f - t, 3f);
        float length = _owner.TentacleLength;
        float facingDir = Mathf.Sign(_owner.transform.lossyScale.x);


        Vector2 targetOffset = new Vector2(length * -0.3f * facingDir, length * 0.7f);

        _targetPos = (Vector2)_owner.tentacleRoot.position + targetOffset;

        Vector2 currentLerpOffset = Vector2.Lerp(_startOffset, targetOffset, easeOutT);

        currentLerpOffset.y -= _playerRadius;

        _owner.IkTargetPosition = (Vector2)_owner.tentacleRoot.position + currentLerpOffset;

        // 4. 이펙트 위치 및 페이드인(알파값) 동시 갱신
        if (_owner.warningEffectRenderer != null && _owner.warningEffectRenderer.gameObject.activeSelf)
        {

            _owner.warningEffectRenderer.transform.position = Vector2.SmoothDamp(
                _owner.warningEffectRenderer.transform.position,
                _prevPlayerPos,
                ref _velocity,
                _followSmoothTime
            );

            Color color = _owner.warningEffectRenderer.color;
            color.a = Mathf.Lerp(0f, _targetAlpha, t);
            _owner.warningEffectRenderer.color = color;

            _prevPlayerPos = _playerTransform.position;
            _prevPlayerPos.y -= _playerRadius;
        }

        // 5초가 경과하면 Attack 상태를 true로 전환
        if (_timer >= _duration)
        {
            _owner.Attack = true;
            Debug.Log("5초 경과! 촉수 공격 준비 완료 (Attack = true)");
        }
    }

    public void Exit()
    {
        SoundManager.Instance.PlaySFX("BossAttack");
        if (_owner.warningEffectRenderer != null)
        {
            _owner.warningEffectRenderer.gameObject.SetActive(false);
        }
    }
}
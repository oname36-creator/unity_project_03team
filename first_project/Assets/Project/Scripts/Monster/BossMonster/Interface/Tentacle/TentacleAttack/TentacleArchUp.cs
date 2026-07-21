using UnityEngine;
using System.Collections;

public class TentacleArchUp : IMonsterState
{
    private TentacleController _owner;
    private Camera _camera;
    private SpriteRenderer _warningEffect;

    // 5초 타이머
    private float _timer;
    private float _duration = 5f;

    private Vector2 _startOffset;
    private Vector2 _targetPos;

    public TentacleArchUp(TentacleController owner)
    {
        _owner = owner;
        _camera = owner.Boss.Camera;
        _warningEffect = owner.warningEffectRenderer_Arch;
    }

    public void Enter()
    {
        _owner.segmentDistance = 0.5f;
        _owner.Target = null;
        _owner.Attack = false;

        _timer = 0f;

        // 시작 위치 오프셋
        _startOffset = _owner.IkTargetPosition - (Vector2)_owner.tentacleRoot.position;

        // 카메라 우측 상단 타겟 계산
        float orthoSize = _camera.orthographicSize;
        float cameraWidth = orthoSize * _camera.aspect;
        Vector2 cameraPos = _camera.transform.position;
        
        // 타겟 위치: 카메라 가장 우측 위 (적절히 offset 부여)
        _targetPos = new Vector2(cameraPos.x + cameraWidth + 2f, cameraPos.y + orthoSize + 2f);

        Debug.Log("TentacleArchUp");

        if (_warningEffect != null)
        {
            // 이펙트를 예상 떨어질 지점(플레이어 위치 등) 화면에 표시
            if (_owner.Boss != null && _owner.Boss.Player != null)
            {
                 _warningEffect.transform.position = _owner.Boss.Player.transform.position;
            }
            
            _warningEffect.gameObject.SetActive(true);
            Color color = _warningEffect.color;
            color.a = 0f;
            _warningEffect.color = color;
        }
    }

    public void Update()
    {
        _timer += Time.deltaTime;

        float t = Mathf.Clamp01(_timer / _duration);
        float easeOutT = 1f - Mathf.Pow(1f - t, 3f);

        // 끝단을 우측 상단으로 이동
        Vector2 rootPos = _owner.tentacleRoot.position;
        _owner.IkTargetPosition = Vector2.Lerp(rootPos + _startOffset, _targetPos, easeOutT);

        // Warning 이미지 투명도 0 -> 0.75f 로 변경
        if (_warningEffect != null && _warningEffect.gameObject.activeSelf)
        {
            Color color = _warningEffect.color;
            color.a = Mathf.Lerp(0f, 0.75f, t);
            _warningEffect.color = color;
        }

        // 5초 경과 후 내려찍기(Attack)으로 전환
        if (_timer >= _duration)
        {
            _owner.Attack = true;
        }
    }

    public void Exit()
    {
        if (_warningEffect != null)
        {
            _warningEffect.gameObject.SetActive(false);
        }
    }
}

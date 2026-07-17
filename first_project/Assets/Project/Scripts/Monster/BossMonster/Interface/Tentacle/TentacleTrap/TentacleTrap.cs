using UnityEngine;

public class TentacleTrap : IMonsterState
{
    private TentacleController _owner;


    private SpriteRenderer _warningEffect;

    private Vector2 _rootPos;
    private Vector2 _targetPos;

    // SmoothDamp 연산용 속도(Velocity) 변수들
    private Vector2 _posVelocity;
    private float _scaleYVelocity; // Y축 크기 조절용 속도 변수 추가

    private Vector3 _initialScale; // 처음 크기 저장용


    private float _moveSmoothTime = 0.3f;   // Y축 이동 속도 (기존 _moveSpeedTime 대체)
    private float _scaleSmoothTime = 0.3f;  // Y축 크기 증가 속도 (새로 추가)
    private float _targetYOffset = 10f;     // 위로 얼마나 이동할지 (+y offset)
    private float _targetYScale = 15.0f;    // Y축으로 최대 얼마나 길어질지

    private float _currentYScale = 1.0f;    // 현재 실시간 Y축 크기 저장용 변수 추가

    private float _time = 0f;
    private float _targetAlpha = 0.5f;
    private float _fadeInDuration = 0.5f;  

    private int _duration = 3;

    public TentacleTrap(TentacleController owner)
    {
        _owner = owner;

        _warningEffect = _owner.warningEffectRenderer_2;
    }

    public void Enter()
    {
        Debug.Log("TentacleTrap Enter");
        _owner.IsReturn = false;
        _owner.Attack = false;
        _owner.IsAttach = false;

        _rootPos = _owner.RootPos;
        _time = 0f; // 상태 진입 시 타이머 초기화

        _owner.UpdateSegmentLength(1);
        _owner.segmentDistance = 0.1f;


        if (_warningEffect != null)
        {
            // 이펙트를 처음 시작 위치(_rootPos)에 둡니다.
            _warningEffect.transform.position = _rootPos;
            _warningEffect.gameObject.SetActive(true);

            _initialScale = _warningEffect.transform.localScale;
            _currentYScale = _initialScale.y; // 시작 크기를 초기 스케일 값으로 세팅

            // 알파값 0으로 초기화
            Color color = _warningEffect.color;
            color.a = 0f;
            _warningEffect.color = color;
        }
    }

    public void Update()
    {
        if (_rootPos == Vector2.zero)
        {
            _rootPos = _owner.RootPos;
            return;
        }
        _time += Time.deltaTime;

        // 4. 이펙트 위치, 크기 및 페이드인(알파값) 동시 갱신
        if (_warningEffect != null && _warningEffect.gameObject.activeSelf)
        {
            // A. Y축 크기(Scale) 서서히 증가시키기
            _currentYScale = Mathf.SmoothDamp(_currentYScale, _targetYScale, ref _scaleYVelocity, _scaleSmoothTime);

            Vector3 localScale = _warningEffect.transform.localScale;
            localScale.y = _currentYScale;
            _warningEffect.transform.localScale = localScale;

            // B. 목표 위치 계산 (처음 위치 _rootPos에서 Y축으로만 _targetYOffset만큼 더한 위치)
            Vector2 targetPos = _rootPos + new Vector2(0f, _targetYOffset);

            // C. Y축으로만 부드럽게 이동하기
            _warningEffect.transform.position = Vector2.SmoothDamp(
                _warningEffect.transform.position,
                targetPos,
                ref _posVelocity,
                _moveSmoothTime
            );

            // D. 페이드인 (경과 시간 _time을 기준으로 서서히 알파값 올리기)
            Color color = _warningEffect.color;
            // _time이 _fadeInDuration에 도달할 때까지 0에서 1로 변하는 비율(t) 계산
            float fadeProgress = Mathf.Clamp01(_time / _fadeInDuration);
            color.a = Mathf.Lerp(0f, _targetAlpha, fadeProgress);
            _warningEffect.color = color;
        }

        if (_time > _duration)
        {
            _owner.Attack = true;
        }
    }

    public void Exit()
    {
        if (_warningEffect != null)
        {
            _warningEffect.gameObject.SetActive(false);
            _warningEffect.transform.localScale = _initialScale;
        }
    }
}
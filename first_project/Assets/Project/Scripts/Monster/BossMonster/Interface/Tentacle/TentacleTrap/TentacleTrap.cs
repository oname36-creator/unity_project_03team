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
        //Debug.Log("TentacleTrap Enter");
        _owner.IsReturn = false;
        _owner.Attack = false;
        _owner.IsAttach = false;

        _rootPos = _owner.RootPos;
        _time = 0f; // 상태 진입 시 타이머 초기화

        _owner.UpdateSegmentLength(1);
        _owner.segmentDistance = 0.1f;

        float dy;
        if (_owner.Up)
        {
            dy = 12.5f;
        }
        else 
        {
            dy = -12.5f;
        }

        if (_warningEffect != null)
        {
            // 이펙트를 처음 시작 위치(_rootPos)에 둡니다.
            _warningEffect.transform.position = new Vector3(_rootPos.x, _rootPos.y + dy, 0);
            
            _warningEffect.gameObject.SetActive(true);

            _initialScale = _warningEffect.transform.localScale;

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
using UnityEngine;

public class TentacleReturn : IMonsterState
{
    private TentacleController _owner;

    private Vector2 _rootPos;
    private Vector2 _targetPos; // 현재 IK 목표 위치 (점점 바닥으로 내려감)

    // --- 속도 조절 변수들 ---
    private float _descendSpeed = 7.5f;  // 촉수 끝(머리)이 바닥으로 내려가는 속도 (올라올 때와 비슷하게 맞춤)
    private float _shrinkSpeed = 1.5f;  // 촉수 마디 간격이 줄어드는 속도

    private float _swaySpeed = 15f;     // 내려갈 때 좌우로 요동치는 속도
    private float _swayAmount = 2f;     // 좌우로 요동치는 폭

    private float _time = 0f;

    public TentacleReturn(TentacleController owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        //Debug.Log("TentacleTrap Return");

        // _owner.tag = "Boss"; // GC 방지를 위해 태그 할당 제거
        _owner.SetLayer(true);

        _rootPos = _owner.RootPos;
        _targetPos = _owner.IkTargetPosition; // 현재 하늘에 있는 촉수 끝 위치에서 시작
        _time = 0f;
    }

    public void Update()
    {
        float dt = Time.deltaTime;
        if(_time < 0.5f)
        {
            _time += dt;
            return; 
        }
        // 1. 머리 위치를 바닥(RootPos)을 향해 서서히 내림
        _targetPos = Vector2.MoveTowards(_targetPos, _rootPos, _descendSpeed * dt);

        // 2. 내려가면서 좌우로 살짝 흔들리는 효과 (필요 없다면 이 두 줄은 지워도 무방합니다)
        float swayX = Mathf.Sin(Time.time * _swaySpeed) * _swayAmount * dt;
        _targetPos.x += swayX;

        // 목표 위치 적용
        _owner.IkTargetPosition = _targetPos;

        // 3. 마디 간격을 줄여서 텐타클이 쪼그라들게 만듦
        _owner.segmentDistance -= _shrinkSpeed * dt;

        // 4. 바닥에 거의 다 내려왔거나, 마디가 완전히 쪼그라들었으면 종료
        float sqrDistanceToRoot = (_targetPos - _rootPos).sqrMagnitude;

        // Debug.Log("rootPos: " + _rootPos + ", targetPos: " + _targetPos + " distance : " + distanceToRoot);
        if (sqrDistanceToRoot < 0.25f || _owner.segmentDistance < 0.1f)
        {
            _owner.isTrap = false;
        }
    }

    public void Exit()
    {
        ObjectPoolManager.Instance.TentaclePush(_owner.gameObject);
    }
}
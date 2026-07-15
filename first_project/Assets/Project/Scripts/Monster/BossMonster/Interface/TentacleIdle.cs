using Unity.VisualScripting;
using UnityEngine;

public class TentacleIdle : IMonsterState
{
    private TentacleController _owner;
    private float _rayDistance;
    private LayerMask _targetLayer;

    private float _sweepAngle = 45f;
    private float _sweepSpeed = 2f;
    private float _time = 0f;
    private float _fixedLength = 5f;

    public TentacleIdle(TentacleController owner)
    {
        _owner = owner;
        _targetLayer = LayerMask.GetMask("Player", "Monster");
    }

    public void Enter()
    {
        _time = 0f;
        _owner.IsSearch = false;
        _owner.IsAttach = false;
        _owner.Attack = false;
        
        _owner.Target = null;
        _owner.UpdateSegmentLength(_owner.PrevSegmentLength);
        _owner.segmentDistance = 0.5f;

        // 촉수가 Root 근처로 회수된 상태로 대기하도록 초기 목표 설정
        _owner.IkTargetPosition = _owner.tentacleRoot.position;
        _rayDistance = _owner.TentacleLength;
        Debug.Log("TentacleIdle");
    }

    public void Update()
    {
        _time += Time.deltaTime * _sweepSpeed;
        float currentAngle = Mathf.Sin(_time) * _sweepAngle;

        Vector2 rayDirection = Quaternion.Euler(0, 0, currentAngle) * Vector2.right;
        Vector2 rayOrigin = _owner.tentacleRoot.position;

        // 대기 상태일 때는 촉수가 탐색 방향으로 부드럽게 뻗어 있게 합니다. (회수 중일 땐 Root로 가고, 회수되면 흔들림)
        float distanceToRoot = Vector2.Distance(_owner.GetGrabber.position, rayOrigin);
        if (distanceToRoot > _fixedLength + 1f) // 아직 회수 중
        {
            _owner.IkTargetPosition = rayOrigin;
        }
        else // 회수 완료, 탐색 흔들림 시작
        {
            _owner.IkTargetPosition = rayOrigin + rayDirection * _fixedLength;
        }

        Debug.DrawRay(rayOrigin, rayDirection * _rayDistance, Color.red);

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, _rayDistance, _targetLayer);

        if (hit.collider != null)
        {

            _owner.IsSearch = true; // StateMachine에서 Shot으로 넘어가도록 유도
            GameObject hitObj = hit.collider.gameObject;


            if (!_owner.Boss.IsTargeted(hitObj))
            {
                _owner.Boss.AddTarget(hitObj); // 보스에 타겟 등록
                _owner.Target = hitObj;        // 내 전용 타겟으로 할당

                _owner.IsSearch = true; 
            }

        }
    }

    public void Exit()
    {
    }
}
using UnityEngine;

public class TentacleIdle : IMonsterState
{
    private TentacleController _owner;
    private float _rayDistance;
    private LayerMask _targetLayer;
    private int _tentacleLayer; // 레이어 캐싱

    private float _sweepAngle = 45f;
    private float _sweepAngleRad; // 삼각함수 연산을 위한 라디안 값 캐싱
    private float _sweepSpeed = 2f;
    private float _time = 0f;

    private float _fixedLength = 5f;
    private float _sqrRecoveryDistance; // 거리 비교를 위한 제곱값 캐싱

    private float _raycastTimer = 0f;
    private float _raycastInterval = 0.05f;

    public TentacleIdle(TentacleController owner)
    {
        _owner = owner;
        _targetLayer = LayerMask.GetMask("Player", "Monster");

        _tentacleLayer = LayerMask.NameToLayer("Tentacle");

        _sweepAngleRad = _sweepAngle * Mathf.Deg2Rad;

        float recoveryDist = _fixedLength + 1f;
        _sqrRecoveryDistance = recoveryDist * recoveryDist;
    }

    public void Enter()
    {
        _time = 0f;
        _owner.IsSearch = false;
        _owner.IsAttach = false;
        _owner.Attack = false;


        _owner.gameObject.layer = _tentacleLayer;

        _owner.SetLayer(false);

        _owner.Target = null;
        _owner.UpdateSegmentLength(_owner.PrevSegmentLength);
        _owner.segmentDistance = 0.5f;


        _owner.IkTargetPosition = _owner.tentacleRoot.position;
        _rayDistance = _owner.TentacleLength;
        //Debug.Log("TentacleIdle");
    }

    public void Update()
    {
        _time += Time.deltaTime * _sweepSpeed;


        float currentAngleRad = Mathf.Sin(_time) * _sweepAngleRad;
        Vector2 rayDirection;
        rayDirection.x = Mathf.Cos(currentAngleRad);
        rayDirection.y = Mathf.Sin(currentAngleRad);


        Vector2 rayOrigin = _owner.tentacleRoot.position;


        Vector2 grabberPos = _owner.GetGrabber.position;
        float sqrDistToRoot = (grabberPos - rayOrigin).sqrMagnitude;

        if (sqrDistToRoot > _sqrRecoveryDistance) 
        {
            _owner.IkTargetPosition = rayOrigin;
        }
        else 
        {
            _owner.IkTargetPosition = rayOrigin + rayDirection * _fixedLength;
        }

        //Debug.DrawRay(rayOrigin, rayDirection * _rayDistance, Color.red);

        _raycastTimer += Time.deltaTime;
        if (_raycastTimer >= _raycastInterval)
        {
            _raycastTimer = 0f;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, _rayDistance, _targetLayer);

            if (hit.collider != null)
            {
                _owner.IsSearch = true;
                GameObject hitObj = hit.collider.gameObject;

                if (!_owner.Boss.IsTargeted(hitObj))
                {
                    _owner.Boss.AddTarget(hitObj); 
                    _owner.Target = hitObj;        
                }
            }
        }
    }

    public void Exit()
    {
    }
}
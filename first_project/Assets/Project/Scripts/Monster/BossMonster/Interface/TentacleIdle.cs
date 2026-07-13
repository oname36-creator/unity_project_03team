using System.Collections;
using UnityEngine;

public class TentacleIdle : IMonsterState
{
    private TentacleController _owner;
    private Transform _ownerTransform;

    private float _rayDistance = 10f;
    private LayerMask _targetLayer;

    // 탐색 범위 및 속도 변수
   
    private GameObject _anchorPoint;
    private float _sweepAngle = 45f; // 위아래 탐색 각도
    private float _sweepSpeed = 2f;
    private float _time = 0f;
    private float _fixedLength = 5f;

    public TentacleIdle(TentacleController owner)
    {
        this._owner = owner;
        _ownerTransform = owner.GetComponent<Transform>();

        _targetLayer = LayerMask.GetMask("Player", "Ground");
    }

    public void Enter()
    {
        _time = 0f;
        _owner.IsSearch = false; 
        Debug.Log("TentacleIdle");
    }

    public void Update()
    {
        // 1. Ray로 지정 범위를 위에서 아래로 훑기 위한 각도 계산 (Sin을 이용한 왕복)
        _time += Time.deltaTime * _sweepSpeed;
        float currentAngle = Mathf.Sin(_time) * _sweepAngle;

        // 기준 방향(오른쪽)에서 currentAngle만큼 회전한 방향 벡터
        Vector2 rayDirection = Quaternion.Euler(0, 0, currentAngle) * Vector2.right;
        Vector2 rayOrigin = _owner.tentacleRoot.position;

        // 2. 촉수의 끝 방향을 Ray의 방향으로 고정 (길이는 _fixedLength)
        _owner.IkTargetPosition = rayOrigin + rayDirection * _fixedLength;
        Debug.DrawRay(rayOrigin, rayDirection * _rayDistance, Color.red);

        // 3. 타겟 탐색
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, _rayDistance, _targetLayer);

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Ground"))
            {
                // 땅일 경우: 맞은 표면에 임시 닻(Anchor)을 만들고 타겟으로 설정
                if (_anchorPoint == null)
                {
                    _anchorPoint = new GameObject("TentacleAnchor");
                }
                _anchorPoint.transform.position = hit.point; // Ray가 맞은 정확한 표면 좌표
                _anchorPoint.transform.SetParent(hit.collider.transform); // 땅이 움직일 경우를 대비해 자식으로 설정

                _owner.Boss.Target = _anchorPoint.transform;
            }
            else
            {
                // 플레이어일 경우: 기존대로 플레이어 자체를 타겟
                _owner.Boss.Target = hit.transform;
            }

            _owner.IsSearch = true;
        }
    }

    public void Exit()
    {
        
    }
}
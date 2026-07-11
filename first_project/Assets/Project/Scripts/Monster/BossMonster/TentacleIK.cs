using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TentacleIK : MonoBehaviour
{
    public int nodeCount = 10;           // 관절 개수
    public float boneLength = 0.5f;      // 각 마디의 길이
    public int iterations = 5;           // 연산 반복 횟수 (보통 3~5면 충분)
    public float tolerance = 0.01f;      // 허용 오차 거리

    private Vector3[] _positions;        // 관절 위치 배열
    private float[] _boneLengths;        // 각 뼈대의 길이 배열
    private float _totalLength;          // 척추의 총 길이

    private LineRenderer _lineRenderer;

    void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        InitNodes();
    }

    // 뼈대 배열 초기화
    private void InitNodes()
    {
        _positions = new Vector3[nodeCount];
        _boneLengths = new float[nodeCount - 1];
        _totalLength = 0f;

        for (int i = 0; i < nodeCount; i++)
        {
            // 초기 위치를 위쪽으로 곧게 뻗은 상태로 세팅
            _positions[i] = transform.position + Vector3.up * (i * boneLength);

            if (i < nodeCount - 1)
            {
                _boneLengths[i] = boneLength;
                _totalLength += boneLength;
            }
        }

        _lineRenderer.positionCount = nodeCount;
    }


    public Vector3 GetNodePosition(int index)
    {
        if (_positions == null || _positions.Length == 0) return transform.position;

        // 인덱스가 범위를 벗어나지 않도록 안전 처리
        index = Mathf.Clamp(index, 0, _positions.Length - 1);
        return _positions[index];
    }

    // BossController -> Spine에서 매 프레임 호출할 IK 연산 함수
    public void ResolveIK(Vector3 basePosition, Vector3 targetPosition)
    {
        // 1. 시작점 고정 (메인 척추가 이동했을 수 있으므로 전체를 이동)
        Vector3 moveOffset = basePosition - _positions[0];
        for (int i = 0; i < nodeCount; i++)
        {
            _positions[i] += moveOffset;
        }

        // 2. 타겟까지의 거리 검사
        float targetDist = Vector3.Distance(_positions[0], targetPosition);

        if (targetDist >= _totalLength)
        {
            // 타겟이 너무 멀면 일직선으로 뻗음
            Vector3 direction = (targetPosition - _positions[0]).normalized;
            for (int i = 1; i < nodeCount; i++)
            {
                _positions[i] = _positions[i - 1] + direction * _boneLengths[i - 1];
            }
        }
        else
        {
            // 타겟이 닿는 거리라면 FABRIK 반복 연산 수행
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                // 끝점이 타겟에 충분히 가까워졌다면 연산 종료 (최적화)
                if (Vector3.Distance(_positions[^1], targetPosition) < tolerance)
                    break;

                // [Backward Pass] 끝점에서 시작점 방향으로
                _positions[^1] = targetPosition;
                for (int i = nodeCount - 2; i >= 0; i--)
                {
                    Vector3 dir = (_positions[i] - _positions[i + 1]).normalized;
                    _positions[i] = _positions[i + 1] + dir * _boneLengths[i];
                }

                // [Forward Pass] 시작점에서 끝점 방향으로
                _positions[0] = basePosition;
                for (int i = 1; i < nodeCount; i++)
                {
                    Vector3 dir = (_positions[i] - _positions[i - 1]).normalized;
                    _positions[i] = _positions[i - 1] + dir * _boneLengths[i - 1];
                }
            }
        }

        // 3. 연산된 위치를 LineRenderer에 반영
        _lineRenderer.SetPositions(_positions);
    }
}
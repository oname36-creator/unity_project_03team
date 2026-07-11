using UnityEngine;
using System.Collections.Generic;

public class SubSpine : MonoBehaviour
{
    [Header("Tentacles")]
    public List<Tentacle> tentacles = new List<Tentacle>();

    [Header("Vertex count")]
    public int _positionCount = 10;

    [Header("Spine Settings")]
    public float nodeDistance = 0.5f;
    public float circleRadius = 2.0f;

    private Vector3 _startPoint;
    private Vector3 _endPoint;
    private LineRenderer _lineRenderer;
    private Rigidbody2D _rigidbody2D;


    // 프로퍼티를 통해 MainSpine에서 접근할 수 있도록 열어줍니다.
    public Vector3 StartPoint => _startPoint;
    public Vector3 EndPoint => _endPoint;

    public void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();

        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = _positionCount; // 런타임에 pointCount 강제 적용

        _lineRenderer.startWidth = 0.5f;
        _lineRenderer.endWidth = 0.5f;

        // 초기화 (게임 시작 시 한 점에 뭉치지 않도록 원형으로 분산 배치)
        for (int i = 0; i < _positionCount; i++)
        {
            float angle = i * (360f / _positionCount) * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * circleRadius;
            _lineRenderer.SetPosition(i, transform.position + offset);
        }

        UpdatePoints();
    }

    // [정방향 IK] 시작점을 타겟으로 옮기고, 뒤따르는 마디들을 끌어당깁니다.
    public void UpdateForward(Vector3 targetStartPos)
    {
        if (_lineRenderer == null) return;

        _lineRenderer.SetPosition(0, targetStartPos);

        for (int i = 1; i < _positionCount; i++)
        {
            Vector3 currentPos = _lineRenderer.GetPosition(i);
            Vector3 prevPos = _lineRenderer.GetPosition(i - 1);

            Vector3 dir = (currentPos - prevPos).normalized;
            if (dir == Vector3.zero) dir = transform.forward; // 영벡터 예외 처리

            _lineRenderer.SetPosition(i, prevPos + dir * nodeDistance);
        }

        UpdatePoints();
    }

    // [역방향 IK] 끝점을 타겟(다음 선의 시작점)으로 옮기고, 앞의 마디들을 끌어당깁니다.
    public void UpdateBackward(Vector3 targetEndPos)
    {
        if (_lineRenderer == null) return;

        int lastIndex = _positionCount - 1;
        _lineRenderer.SetPosition(lastIndex, targetEndPos);

        for (int i = lastIndex - 1; i >= 0; i--)
        {
            Vector3 currentPos = _lineRenderer.GetPosition(i);
            Vector3 nextPos = _lineRenderer.GetPosition(i + 1);

            Vector3 dir = (currentPos - nextPos).normalized;
            if (dir == Vector3.zero) dir = -transform.forward;

            _lineRenderer.SetPosition(i, nextPos + dir * nodeDistance);
        }

        UpdatePoints();
    }

    private void UpdatePoints()
    {
        _startPoint = _lineRenderer.GetPosition(0);
        _endPoint = _lineRenderer.GetPosition(_positionCount - 1);
    }

    // MainSpine에서 모든 계산이 끝난 후 호출됩니다.
    public void UpdateTentacles()
    {
        for (int i = 0; i < tentacles.Count; i++)
        {
            // tentacles[i].UpdateTentacleProcess(...);
        }
    }
}
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer), typeof(EdgeCollider2D))]
public class TentacleController : MonoBehaviour
{
    [Header("Boss")]
    public BossController Boss;
    public BodyController Body;

    [Header("Tentacle IK Setting")]
    public int segmentLength = 15;        // 촉수 마디 개수
    public float segmentDistance = 0.5f;  // 마디 사이의 간격
    public float smoothSpeed = 0.05f;     // 끝단이 목표로 이동하는 속도

    [Header("Components")]
    public Transform tentacleRoot;        // 촉수가 시작되는 위치 (보스 몸통 등)
    public Transform grabberHead;         // 물건을 잡을 트리거가 있는 실제 오브젝트

    private LineRenderer _lineRend;
    private EdgeCollider2D _edgeCollider;

    private Vector2[] _segmentPos;
    private Vector2[] _segmentVelocity;

    // 상태 머신(TentacleStretch 등)에서 이 값을 변경하여 촉수를 조종합니다.
    public Vector2 IkTargetPosition { get; set; }

    private bool _isDead = false;
    private bool _isAttach = false;
    private MonsterStateMachine _monsterMachine;

    public BossController GetBoss => Boss;
    public BodyController GetBody => Body;
    public bool IsAttach
    {
        get => _isAttach;
        set => _isAttach = value;
    }

    void Start()
    {
        _lineRend = GetComponent<LineRenderer>();
        _edgeCollider = GetComponent<EdgeCollider2D>();

        _lineRend.positionCount = segmentLength;
        _segmentPos = new Vector2[segmentLength];
        _segmentVelocity = new Vector2[segmentLength];

        // 초기 위치 세팅
        for (int i = 0; i < segmentLength; i++)
        {
            _segmentPos[i] = tentacleRoot.position;
        }
        IkTargetPosition = tentacleRoot.position;

        _monsterMachine = MonsterAiBrain.MakeMachine("BossTentacle", this);
    }

    void Update()
    {
        if (_isDead) return;
        _monsterMachine.Update();
    }

    void LateUpdate()
    {
        if (_isDead) return;
        UpdateIK();
        UpdateColliders();
    }

    private void UpdateIK()
    {
        // 1. 끝단(Head)은 목표 위치(IkTargetPosition)를 향해 부드럽게 이동
        _segmentPos[0] = Vector2.SmoothDamp(_segmentPos[0], IkTargetPosition, ref _segmentVelocity[0], smoothSpeed);

        // 2. 나머지 마디들은 앞의 마디를 일정 간격을 두고 따라감
        for (int i = 1; i < segmentLength; i++)
        {
            Vector2 targetPos = _segmentPos[i - 1] + (_segmentPos[i] - _segmentPos[i - 1]).normalized * segmentDistance;
            _segmentPos[i] = Vector2.SmoothDamp(_segmentPos[i], targetPos, ref _segmentVelocity[i], smoothSpeed);
        }

        // 3. (선택) 시작점을 보스 몸통(Root)에 고정하고 싶다면 FABRIK 알고리즘처럼 역순으로 다시 맞춰야 하지만,
        // 단순하고 자연스러운 움직임을 위해 마지막 마디를 Root에 강제 고정
        _segmentPos[segmentLength - 1] = tentacleRoot.position;

        // 렌더러 업데이트
        for (int i = 0; i < segmentLength; i++)
        {
            _lineRend.SetPosition(i, _segmentPos[i]);
        }

        // Grabber 오브젝트(끝단) 위치 동기화
        if (grabberHead != null)
        {
            grabberHead.position = _segmentPos[0];
        }
    }

    private void UpdateColliders()
    {
        // LineRenderer의 점들을 EdgeCollider2D에 복사하여 몸통 물리 충돌 구현
        List<Vector2> colliderPoints = new List<Vector2>();
        for (int i = 0; i < segmentLength; i++)
        {
            // 콜라이더는 로컬 좌표 기준이므로 변환 필요
            colliderPoints.Add(transform.InverseTransformPoint(_segmentPos[i]));
        }
        _edgeCollider.SetPoints(colliderPoints);
    }




}
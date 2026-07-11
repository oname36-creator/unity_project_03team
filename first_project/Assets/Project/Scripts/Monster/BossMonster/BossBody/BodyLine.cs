using System;
using System.Collections.Generic;
using UnityEngine;

// 1. 인스펙터 노출용 커스텀 구조체
// [System.Serializable]을 반드시 선언해야 유니티 직렬화 시스템이 인식합니다.
[Serializable]
public struct NodeConfig
{
    [Tooltip("설정할 노드의 인덱스 (Key 역할)")]
    public int NodeIndex;

    [Header("해당 노드에 할당할 프리팹들 (Value 역할)")]
    public GameObject[] Sprites;
    public GameObject[] Branches;
    public GameObject[] Hands;
}


public class BodyLine : MonoBehaviour
{
    #region Serialized Fields
    [Header("촉수 설정 (Body Line Settings)")]
    public int NodeCount = 10;           // LineRenderer의 Node N개
    public float SegmentLength = 0.5f;   // 각 노드 사이의 간격 (Constraint)
    public float Gravity = 0.0f;         // 중력 (캐리온은 벽을 타므로 보통 0으로 설정)
    public float Stiffness = 10f;        // 촉수의 뻣뻣함 정도 (반복 계산 횟수)

    [Header("노드별 설정")]
    public List<NodeConfig> NodeSettings = new List<NodeConfig>();
    // 주의: 유니티 기본 인스펙터에서는 Dictionary가 노출되지 않습니다. 
    // 인스펙터에서 설정하려면 Odin Inspector를 쓰거나 별도의 구조체/리스트로 우회해야 할 수 있습니다.

    [Header("헤드 이동 설정")]
    public float HeadSmoothTime = 0.05f;  // 목표까지 도달하는 대략적인 시간 (작을수록 민첩함)
    private Vector2 _headVelocity;        // SmoothDamp 내부에서 현재 속도를 추적하기 위한 변수

    [Header("시각적 요소 (Visuals)")]
    public LineRenderer LineRenderer;    // 몸체를 그릴 LineRenderer
    #endregion

    #region Private Fields

    private int _headNode;

    // 베를레 적분을 위한 노드 데이터
    private Vector2[] _positions;
    private Vector2[] _oldPositions;

    // 생성된 오브젝트들을 담을 배열
    private Dictionary<int, GameObject[]> _nodeSprites;
    private Dictionary<int, GameObject[]> _nodeBranches;
    private Dictionary<int, GameObject[]> _nodeHands;

    // 이동을 쉽게 하기 위해 묶어둘 부모 객체들
    private Transform[] _nodeParents;
    // 잔가지들을 묶어줄 부모 트랜스폼 배열 추가
    private Transform[] _branchParents;
    private Transform[] _handParents;

    private Transform _target;             // 본체(0번 노드)가 따라갈 목표 위치 (플레이어 조작 위치)
    #endregion


    public int SetHeadNode
    {
        set 
        {
            if (value < 0 || value >= NodeCount) { return; }
            _headNode= value; 
        }
    }

    public Transform Target 
    {
        set { _target = value; }
    }




    #region Unity Lifecycle
    void Start()
    {
        // 1. 배열 및 LineRenderer 초기화
        _headNode = 0;
        _positions = new Vector2[NodeCount];
        _oldPositions = new Vector2[NodeCount];
        
        _nodeSprites = new Dictionary<int, GameObject[]>(); // 딕셔너리로 초기화
        _nodeBranches = new Dictionary<int, GameObject[]>();
        _nodeHands = new Dictionary<int, GameObject[]>();

        _nodeParents = new Transform[NodeCount];
        _branchParents = new Transform[NodeCount]; // 잔가지 부모 배열 초기화
        _handParents = new Transform[NodeCount]; // 잔가지 부모 배열 초기화

        LineRenderer.positionCount = NodeCount;

        Vector2 startPos = transform.position;

        // 2. 인스펙터 리스트 데이터를 O(1) 검색이 가능한 임시 딕셔너리로 변환 (프리팹 매핑용)
        Dictionary<int, NodeConfig> configDict = new Dictionary<int, NodeConfig>();
        foreach (var config in NodeSettings)
        {
            // 중복 키 입력(인스펙터 휴먼 에러) 방지 처리
            if (_nodeSprites.ContainsKey(config.NodeIndex))
            {
                Debug.LogWarning($"[BodyLine] 노드 인덱스 {config.NodeIndex}가 중복 설정되었습니다.");
                continue;
            }
        }

        // 2. Node N개 초기 세팅 (Sprite, Branch 생성)
        for (int i = 0; i < NodeCount; i++)
        {
            _positions[i] = startPos - new Vector2(0, i * SegmentLength);
            _oldPositions[i] = _positions[i];

            // 해당 노드 인덱스에 설정된 데이터(NodeConfig)가 있는지 확인
            if (configDict.TryGetValue(i, out NodeConfig config))
            {
                // --- Sprites 생성 ---
                if (config.Sprites != null && config.Sprites.Length > 0)
                {
                    GameObject parentObj = new GameObject($"Node_{i}_Visuals");
                    parentObj.transform.parent = this.transform;
                    _nodeParents[i] = parentObj.transform;

                    GameObject[] spawnedSprites = new GameObject[config.Sprites.Length];
                    for (int j = 0; j < config.Sprites.Length; j++)
                    {
                        GameObject spr = Instantiate(config.Sprites[j], parentObj.transform);
                        spr.transform.localPosition = UnityEngine.Random.insideUnitCircle * 0.3f;
                        spawnedSprites[j] = spr;
                    }
                    _nodeSprites.Add(i, spawnedSprites); // 딕셔너리에 '생성된 인스턴스' 할당
                }

                // --- Branches 생성 ---
                if (config.Branches != null && config.Branches.Length > 0)
                {
                    GameObject branchParentObj = new GameObject($"Node_{i}_Branches");
                    branchParentObj.transform.parent = this.transform;
                    _branchParents[i] = branchParentObj.transform;

                    GameObject[] spawnedBranches = new GameObject[config.Branches.Length];
                    for (int j = 0; j < config.Branches.Length; j++)
                    {
                        GameObject branch = Instantiate(config.Branches[j], _positions[i], Quaternion.identity, branchParentObj.transform);
                        spawnedBranches[j] = branch;
                    }
                    _nodeBranches.Add(i, spawnedBranches);
                }

                if (config.Hands != null && config.Hands.Length > 0) 
                {

                    GameObject handParentObj = new GameObject($"Node_{i}_Branches");
                    handParentObj.transform.parent = this.transform;
                    _handParents[i] = handParentObj.transform;

                    GameObject[] spawnedHands = new GameObject[config.Hands.Length];
                    for (int j = 0; j < config.Hands.Length; j++)
                    {
                        GameObject Hands = Instantiate(config.Hands[j], _positions[i], Quaternion.identity, handParentObj.transform);
                        spawnedHands[j] = Hands;
                    }
                    _nodeHands.Add(i, spawnedHands);
                }
            }
        }
    }

    void Update()
    {
        // 매 프레임마다 계산된 위치를 바탕으로 시각적 요소(LineRenderer, Sprites) 업데이트
        UpdateVisuals();
    }


    void FixedUpdate()
    {
        // 물리 연산 (베를레 적분)은 FixedUpdate에서 처리
        Simulate();        // 1. 중력 및 관성 적용 (베를레 적분)
        MoveHeadTarget();  // 2. 헤드 노드를 타겟으로 부드럽게 이동 (단 1회 호출)

        // 연결 유지 (Constraint) 계산을 여러 번 반복할수록 촉수가 뻣뻣하고 안정적으로 변함
        for (int i = 0; i < Stiffness; i++)
        {
            ApplyConstraints();
        }
    }
    #endregion

    private void Simulate()
    {
        // 베를레 적분: 이전 위치와 현재 위치의 차이(관성)를 이용해 다음 위치를 계산
        for (int i = 0; i < NodeCount; i++) 
        {
            if(_headNode == i) { continue; }
            Vector2 velocity = _positions[i] - _oldPositions[i];
            _oldPositions[i] = _positions[i];

            // 새로운 위치 = 현재 위치 + 속도 + (중력 * 시간 * 시간)
            _positions[i] += velocity + (Vector2.down * Gravity * Time.fixedDeltaTime * Time.fixedDeltaTime);
        }
    }


    private void MoveHeadTarget()
    {
        // 목표를 향해 가상의 감쇠 스프링을 연결한 것과 동일한 효과를 냅니다.
        // 시스템의 감쇠비가 1(ζ = 1)인 상태로 모델링되어 진동(Jitter) 없이 안정적으로 타겟에 수렴합니다.
        _positions[_headNode] = Vector2.SmoothDamp(
            _positions[_headNode],
            _target.position,
            ref _headVelocity,
            HeadSmoothTime,
            Mathf.Infinity,
            Time.fixedDeltaTime
        );
    }
    private void ApplyConstraints()
    {

        // 각 노드 사이의 거리가 segmentLength를 유지하도록 위치를 보정
        for (int i = 0; i < NodeCount - 1; i++)
        {
            Vector2 direction = _positions[i + 1] - _positions[i];
            float currentDistance = direction.magnitude;

            // 거리가 0일 경우 오류 방지
            if (currentDistance == 0) continue;

            // 유지해야 할 거리(segmentLength)와 현재 거리의 차이 계산
            float error = currentDistance - SegmentLength;
            Vector2 correction = direction.normalized * error;

            // 0번 노드(본체)는 고정이므로, 1번 노드만 100% 보정 이동시킴
            if (i == _headNode)
            {
                _positions[i + 1] -= correction;
            }
            else
            {
                // 나머지 노드들은 서로 절반씩 이동하여 거리를 맞춤 (장력 시뮬레이션)
                _positions[i] += correction * 0.5f;
                _positions[i + 1] -= correction * 0.5f;
            }
        }
    }

    private void UpdateVisuals()
    {
        for (int i = 0; i < NodeCount; i++)
        {
            // LineRenderer의 점 위치 갱신
            LineRenderer.SetPosition(i, _positions[i]);

            // 각 Node의 Sprite 위치 갱신
            if (_nodeParents[i] != null)
            {
                _nodeParents[i].position = _positions[i];
            }

            // 잔가지(Branch) 위치 갱신 (잔가지의 시작점을 이 노드로 고정)
            if (_branchParents[i] != null)
            {
                _branchParents[i].position = _positions[i];
            }
        }
    }
}

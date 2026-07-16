using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.IK;

[RequireComponent(typeof(LineRenderer), typeof(EdgeCollider2D))]
public class TentacleController : MonoBehaviour
{
    [Header("Boss")]
    public BossController Boss;
    public BodyController Body;

    [Header("Tentacle IK Setting")]
    public int segmentLength = 15;        // 촉수 마디 개수
    public float segmentDistance = 0.5f;  // 마디 사이의 간격
    public float smoothSpeed = 0.05f;     // 끝단이 목표로 이동하
    // FABRIK 연산 반복 횟수 (보통 2~3회면 충분히 자연스럽게 수렴)
    public int iterations = 3;


    [Header("Components")]
    public Transform tentacleRoot;        // 촉수가 시작되는 위치 (보스 몸통 등)
    public Transform grabberHead;         // 물건을 잡을 트리거가 있는 실제 오브젝트

    [Header("Effects")]
    public SpriteRenderer warningEffectRenderer;

    [Header("Trap")]
    public bool isTrap = false;


    private LineRenderer _lineRend;
    private EdgeCollider2D _edgeCollider;

    private Vector2[] _segmentPos;
    private Vector2[] _segmentVelocity;

    public Vector2 IkTargetPosition { get; set; }

    private bool _isDead = false;
    private bool _isAttach = false;
    private bool _isSearch = false;

    private MonsterStateMachine _monsterMachine;

    public BossController GetBoss
    {
        get { return Boss; } 
    }
    public BodyController GetBody 
    {
        get { return Body; }
    }

    public Transform GetGrabber 
    {
        get { return  grabberHead; }
    }

    public int PrevSegmentLength { get; set; }

    public float TentacleLength 
    {
        get { return segmentLength * segmentDistance; }
    }

    public bool IsAttach
    {
        get { return _isAttach; }
        set { _isAttach = value; }
    }

    public bool IsSearch 
    {
        get { return _isSearch; }
        set { _isSearch = value; }
    }

    public bool IsDead 
    {
        get { return _isDead; }
        set { _isDead = value; }
    }

    public bool IsAttackTentacle { get; set; }

    public bool Attack { get; set; }

    public bool IsReturn { get; set; } = false;

    public Vector2 RootPos {  get; set; } = Vector2.zero;

    public GameObject Target { get; set; }


    void Awake()
    {
        _lineRend = GetComponent<LineRenderer>();
        _edgeCollider = GetComponent<EdgeCollider2D>();

        // 외부(Boss)에서 SetRootPos 등을 호출하기 전에 배열이 무조건 준비되어 있도록 여기서 생성!
        _segmentPos = new Vector2[segmentLength];
        _segmentVelocity = new Vector2[segmentLength];
        PrevSegmentLength = segmentLength;
        Debug.Log("생성");
    }

    void Start()
    {

        Debug.Log("Tentacle Start");

        _lineRend.positionCount = segmentLength;


        IsAttackTentacle = false;
        Attack = false;



        tentacleRoot = Boss.transform;
        if (tentacleRoot != null && !Target)
        {
            // 초기 위치 세팅
            for (int i = 0; i < segmentLength; i++)
            {
                _segmentPos[i] = tentacleRoot.position;
            }
            IkTargetPosition = tentacleRoot.position;
        }

        
        _monsterMachine = MonsterAiBrain.MakeMachine("BossTentacle", this);
    }

    void Update()
    {
        if (_isDead) return;

        if (tentacleRoot == null && !Target)
        {
            tentacleRoot = Boss.transform;
            // 초기 위치 세팅
            for (int i = 0; i < segmentLength; i++)
            {
                _segmentPos[i] = tentacleRoot.position;
            }
            IkTargetPosition = tentacleRoot.position;
        }

        _monsterMachine.Update();
    }

    void LateUpdate()
    {
        if (_isDead) return;
        UpdateIK();
        UpdateColliders();
    }


    public void SetRootPos(Vector2 pos)
    {
        if (isTrap)
        {
            Debug.Log("Tentacle SetRootPos");
            // 초기 위치 세팅
            for (int i = 0; i < segmentLength; i++)
            {
                _segmentPos[i] = pos;
            }
            IkTargetPosition = pos;

            RootPos = pos;
        }
    }




    private void UpdateIK()
    {

        Vector2 targetPos = Vector2.SmoothDamp(_segmentPos[0], IkTargetPosition, ref _segmentVelocity[0], smoothSpeed);


        for (int iter = 0; iter < iterations; iter++)
        {
            // ==========================================
            // [Phase 1] Backward Reaching (끝단 -> 루트 방향)
            // ==========================================

            // 끝단(0번 인덱스)을 목표 위치(targetPos)에 강제로 맞춥니다.
            _segmentPos[0] = targetPos;

            for (int i = 1; i < segmentLength; i++)
            {
                // 현재 마디가 앞 마디(목표 쪽)를 향하는 방향 벡터
                Vector2 dir = (_segmentPos[i] - _segmentPos[i - 1]).normalized;

                // 앞 마디에서 지정된 간격(segmentDistance)만큼 떨어진 곳으로 현재 마디 이동
                _segmentPos[i] = _segmentPos[i - 1] + dir * segmentDistance;

            }
            // ==========================================
            // [Phase 2] Forward Reaching (루트 -> 끝단 방향)
            // ==========================================
            Vector2 basePosition = isTrap ? RootPos : (Vector2)tentacleRoot.position;

            // Phase 1을 거치면 마지막 마디(루트)가 원래 있어야 할 위치(tentacleRoot)에서 벗어납니다.
            // 따라서 마지막 마디를 다시 텐타클의 진짜 루트 위치에 강제로 맞춥니다.
            _segmentPos[segmentLength - 1] = basePosition;

            // 역방향으로 다시 간격을 맞춰줍니다.
            for (int i = segmentLength - 2; i >= 0; i--)
            {
                // 현재 마디가 뒤 마디(루트 쪽)를 향하는 방향 벡터
                Vector2 dir = (_segmentPos[i] - _segmentPos[i + 1]).normalized;

                // 뒤 마디에서 지정된 간격(segmentDistance)만큼 떨어진 곳으로 현재 마디 이동
                _segmentPos[i] = _segmentPos[i + 1] + dir * segmentDistance;
            }
        }

        // 2. 렌더러 업데이트
        for (int i = 0; i < segmentLength; i++)
        {
            _lineRend.SetPosition(i, _segmentPos[i]);
        }

        // 3. Grabber 오브젝트(끝단) 위치 동기화
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

    public void UpdateSegmentLength(int newLength)
    {
        if (segmentLength == newLength) return;

        int oldLength = segmentLength;
        segmentLength = newLength;

        // 라인 렌더러 점 개수 업데이트
        if (_lineRend != null)
        {
            _lineRend.positionCount = segmentLength;
        }

        // 새 길이의 배열 생성
        Vector2[] newPos = new Vector2[segmentLength];
        Vector2[] newVel = new Vector2[segmentLength];

        for (int i = 0; i < segmentLength; i++)
        {
            if (i < oldLength && _segmentPos != null)
            {
                // 기존 위치 데이터가 있으면 그대로 복사
                newPos[i] = _segmentPos[i];
                newVel[i] = _segmentVelocity[i];
            }
            else
            {
                // 새로 늘어난 마디들은 루트 위치로 초기화
                newPos[i] = tentacleRoot != null ? (Vector2)tentacleRoot.position : Vector2.zero;
            }
        }

        // 기존 배열을 새 배열로 덮어쓰기
        _segmentPos = newPos;
        _segmentVelocity = newVel;
    }

    public void SlashAnimation(bool up = false) 
    {
        int count = 0;
        for(int i = 0; i < segmentLength; i++)
        {
            ++count;
            if(count%3 == 0)
            {
                GameObject obj = ObjectPoolManager.Instance.SlashEffectPop();
                obj.transform.position = _segmentPos[i];
                if (up) 
                {
                    obj.transform.rotation = Quaternion.Euler(0, 0, 90);
                }
            }
        }


    }

}
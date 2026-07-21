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

    [Range(0f, 180f)]
    public float maxBendAngle = 25f; // 각 마디 사이의 최대 꺾임 각도 (작을수록 뻣뻣해짐)

    [Header("Elbow Constraint")]
    [Range(-180f, 180f)]
    public float minBendAngle = 0f;
    [Range(-180f, 180f)]
    public float elbowMaxBendAngle = 180f;

    [Header("Whip Physics")]
    public float tailGravity = 30f;


    [Header("Components")]
    public Transform tentacleRoot;        // 촉수가 시작되는 위치 (보스 몸통 등)
    public Transform grabberHead;         // 물건을 잡을 트리거가 있는 실제 오브젝트

    [Header("Effects")]
    public SpriteRenderer warningEffectRenderer_1;
    public SpriteRenderer warningEffectRenderer_2;
    public SpriteRenderer warningEffectRenderer_Arch;

    [Header("Arch")]
    public bool isArch = false;

    [Header("Parabola Attack (Arch)")]
    public bool isParabola = false;
    public float parabolaA = 0.05f; // y = ax^2 의 a 값
    public float parabolaAngle = 0f; // 회전 각도 (도 단위)

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
        get { return grabberHead; }
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

    public bool Up { get; set; } = true;


    public bool IsAttackTentacle { get; set; }

    public bool Attack { get; set; }

    public bool IsArchAttack { get; set; }

    public bool IsReturn { get; set; } = false;

    public Vector2 RootPos { get; set; } = Vector2.zero;

    public GameObject Target { get; set; }

    public float? GroundLimitY { get; set; } = null;
    
    public bool IsGroundHit { get; set; } = false;


    private void OnEnable()
    {
        // 상태 플래그 초기화
        _isDead = false;
        _isAttach = false;
        _isSearch = false;
        IsAttackTentacle = false;
        Attack = false;
        IsArchAttack = false;
        IsReturn = false;

        // OnEnable(), OnDisable() 내부
        isParabola = false;
        parabolaAngle = 0f;

        Target = null;
        isArch = false;
        IsGroundHit = false;

        if (warningEffectRenderer_1 != null) warningEffectRenderer_1.gameObject.SetActive(false);
        if (warningEffectRenderer_2 != null) warningEffectRenderer_2.gameObject.SetActive(false);
        if (warningEffectRenderer_Arch != null) warningEffectRenderer_Arch.gameObject.SetActive(false);

        if (Boss != null)
        {
            tentacleRoot = Boss.transform;
        }

        // 마디 좌표들 현재 소환된 RootPos로 순간 이동
        if (isTrap)
        {
            for (int i = 0; i < segmentLength; ++i)
            {
                if (_segmentPos != null && i < _segmentPos.Length)
                {
                    _segmentPos[i] = RootPos;
                }
            }
            IkTargetPosition = RootPos;
        }
        else if (tentacleRoot != null)
        {
            for (int i = 0; i < segmentLength; ++i)
            {
                if (_segmentPos != null && i < _segmentPos.Length)
                {
                    _segmentPos[i] = tentacleRoot.position;
                }
            }
            IkTargetPosition = tentacleRoot.position;
        }

        // 상태 머신 재초기화
        _monsterMachine = MonsterAiBrain.MakeMachine("BossTentacle", this);
    }

    private void OnDisable()
    {
        // 반환 시 초기화
        Target = null;
        isArch = false;
        isParabola = false;
        parabolaAngle = 0f;
        isTrap = false;
        Up = true;
        // OnEnable(), OnDisable() 내부
        isParabola = false;
        parabolaAngle = 0f;


        RootPos = Vector2.zero;

        if (warningEffectRenderer_1 != null) warningEffectRenderer_1.gameObject.SetActive(false);
        if (warningEffectRenderer_2 != null) warningEffectRenderer_2.gameObject.SetActive(false);
        if (warningEffectRenderer_Arch != null) warningEffectRenderer_Arch.gameObject.SetActive(false);

        // 길이를 원래대로 복구
        if (segmentLength != PrevSegmentLength && PrevSegmentLength > 0)
        {
            UpdateSegmentLength(PrevSegmentLength);
        }
    }

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
        Vector2 targetPos = IkTargetPosition;
        int mid = segmentLength / 2;

        if (isArch)
        {
            Vector2 basePosition = (Vector2)tentacleRoot.position;
            _segmentPos[segmentLength - 1] = basePosition; // 루트 위치 고정

            float currentX = 0f;
            for (int i = segmentLength - 2; i >= 0; i--)
            {
                float prevX = currentX;
                // y = ax^2 곡선 위에서 거리가 segmentDistance가 되도록 x를 증가
                float dx = segmentDistance / Mathf.Sqrt(1f + 4f * parabolaA * parabolaA * prevX * prevX);
                currentX += dx;

                float currentY = parabolaA * currentX * currentX;

                // 로컬 좌표 (1사분면)
                Vector2 localPos = new Vector2(currentX, currentY);

                Quaternion rotation = Quaternion.Euler(0, 0, parabolaAngle);
                Vector2 rotatedPos = rotation * localPos;

                _segmentPos[i] = basePosition + rotatedPos;
            }


            for (int i = 0; i < segmentLength; i++)
            {
                _lineRend.SetPosition(i, _segmentPos[i]);
            }
            if (grabberHead != null)
            {
                grabberHead.position = _segmentPos[0];
            }

            return;
        }


        for (int iter = 0; iter < iterations; iter++)
        {

            // ==========================================
            // [Phase 1] Backward Reaching (끝단 -> 루트 방향)
            // ==========================================
            _segmentPos[0] = targetPos;
            if (GroundLimitY.HasValue)
            {
                _segmentPos[0].y = Mathf.Max(_segmentPos[0].y, GroundLimitY.Value);
            }

            for (int i = 1; i < segmentLength; i++)
            {
                Vector2 dir = (_segmentPos[i] - _segmentPos[i - 1]).normalized;

                _segmentPos[i] = _segmentPos[i - 1] + dir * segmentDistance;

                if (GroundLimitY.HasValue)
                {
                    _segmentPos[i].y = Mathf.Max(_segmentPos[i].y, GroundLimitY.Value);
                }
            }

            // ==========================================
            // [Phase 2] Forward Reaching (루트 -> 끝단 방향)
            // ==========================================
            Vector2 basePosition = isTrap ? RootPos : (Vector2)tentacleRoot.position;
            _segmentPos[segmentLength - 1] = basePosition;

            for (int i = segmentLength - 2; i >= 0; i--)
            {
                Vector2 dir = (_segmentPos[i] - _segmentPos[i + 1]).normalized;

                _segmentPos[i] = _segmentPos[i + 1] + dir * segmentDistance;

                if (GroundLimitY.HasValue)
                {
                    _segmentPos[i].y = Mathf.Max(_segmentPos[i].y, GroundLimitY.Value);
                }

            }
        }

        // 2. 렌더러 업데이트
        for (int i = 0; i < segmentLength; i++)
        {
            _lineRend.SetPosition(i, _segmentPos[i]);
        }

        // 3. Grabber 오브젝트(끝단 또는 중간) 위치 동기화
        if (grabberHead != null)
        {
            if (IsArchAttack)
            {
                grabberHead.position = _segmentPos[segmentLength / 2];
            }
            else
            {
                grabberHead.position = _segmentPos[0];
            }
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

    public Vector2 GetSegmentPos(int index)
    {
        if (_segmentPos != null && index >= 0 && index < segmentLength)
        {
            return _segmentPos[index];
        }
        return tentacleRoot != null ? (Vector2)tentacleRoot.position : Vector2.zero;
    }

    public void SlashAnimation(bool up = false)
    {
        int count = 0;
        for (int i = 0; i < segmentLength; i++)
        {
            ++count;
            if (count % 3 == 0)
            {
                GameObject obj = ObjectPoolManager.Instance.SlashEffectPop();
                if (obj == null) { return; }
                obj.transform.position = _segmentPos[i];
                if (up)
                {
                    obj.transform.rotation = Quaternion.Euler(0, 0, 90);
                }
            }
        }


    }

}
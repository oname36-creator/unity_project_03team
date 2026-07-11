using UnityEngine;
using System.Collections;


public class BossController : MonoBehaviour
{


    #region Serialized Fields
    [Header("Monster Data")] // 인스펙터에 제목 표시
    public BaseMonsterData MonsterData;

    [Header("References")]
    public MainSpine mainSpine;

    [Header("Orbit Settings")]
    public float orbitRadius = 5f;      // 순회할 원의 반지름
    public float rotateSpeed = 120f;    // 순회 속도 (초당 도달 각도)

 

    #endregion


    #region Private Fields
    private int _hp;
    private int _damage;
    private int _searchRange; // 탐색 깊이
    private int _attackRange; // 공격 가능 거리
    private int _force; // 힘
    private int _nodeCount;

    private float _angle;   // 탐색 각도
    private float _cosValue; // 각도의 cos 값
    private float _currentAngle = 0f;

    private float _speed;
    private float _maxSpeed; // 최대 속도

    private bool _isChase;
    private bool _isDead;
    private bool _isGround;


    private Vector2 _frontVector;

    private Vector3[] _ableTargetVectors;


    private MainSpine _mainSpine; // BodyLine 대신 MainSpine으로 관리
    private MonsterStateMachine _monsterMachine;

    #endregion


    public bool Chase
    {
        get { return _isChase; }
        set { _isChase = value; }
    }
    public bool IsDead
    {
        get { return _isDead; }
        set { _isDead = value; }
    }

    public bool Ground
    {
        get { return _isGround; }
        set { _isGround = value; }
    }

    //public bool Attached
    //{
        //get { return _body.IsAttached; }
    //}



    public Vector2 Front
    {
        get { return _frontVector; }
        set
        {
            if (value.magnitude != 1)
            {
                _frontVector = value.normalized;
            }
            else
            {
                _frontVector = value;
            }
        }
    }

    #region Unity Lifecycle

    void Start()
    {
        Application.targetFrameRate = 120;

        _hp = MonsterData.hp;
        _damage = MonsterData.damage;
        _searchRange = MonsterData.searchRange;
        _attackRange = MonsterData.attackRange;
        _angle = MonsterData.angle;
        _speed = MonsterData.Speed;
        _maxSpeed = MonsterData.MaxSpeed;
        _force = MonsterData.Force;

        _frontVector.x = 1;

        _isDead = false;
        _isChase = true;
        _isGround = true;


        _mainSpine = GetComponent<MainSpine>();
        //_nodeCount = _body.GetNodeCount;

        _ableTargetVectors = new Vector3[_nodeCount];

        // 위아래로 최대 몇 도까지 퍼질지 설정 _angle

        for (int i = 0; i < _nodeCount; i++)
        {

            // 1. -maxSpreadAngle + 20 ~ +maxSpreadAngle + 20 사이의 랜덤한 X축 회전각 생성
            float randomXAngle = Random.Range(-_angle, _angle);

            // 2. 앞 방향(Vector3.forward)을 기준으로 위아래(X축 회전)만 적용
            Quaternion spreadRotation = Quaternion.Euler(randomXAngle, 0f, 0f);

            // 3. 회전값을 정방향 벡터에 곱해 최종 방향 벡터 생성
            _ableTargetVectors[i] = spreadRotation * Vector3.forward;
        }


        _monsterMachine = MonsterAiBrain.MakeMachine("Boss", this);

    }


    void Update()
    {
        if (_isDead) { return; }
        // 1. 상태 머신 판단 (이동할 목표점, 공격 여부 등 연산)
        _monsterMachine.Update();

        // 1. 보스의 이동 로직 처리 (예: 전진, 플레이어 추적 등)
        MoveBoss();

        // 2. 갱신된 현재 위치를 스파인의 targetPosition으로 전달하여 관절 업데이트
        if (_mainSpine != null)
        {
            _mainSpine.UpdateSpineProcess(transform.position); //[cite: 1]
        }
        ;
        
    }

    #endregion


    private void MoveBoss()
    {
        // 1. 시간에 따라 각도를 지속적으로 증가시킵니다.
        _currentAngle += rotateSpeed * Time.deltaTime;

        // 2. 삼각함수를 이용해 현재 위치(중심)를 기준으로 원둘레 상의 타겟 위치를 계산합니다.
        float radian = _currentAngle * Mathf.Deg2Rad;

        // 2D 평면(XY) 기준 회전. 만약 3D(XZ 평면) 회전이 필요하다면 y와 z의 위치를 바꿔주세요.
        Vector3 offset = new Vector3(Mathf.Cos(radian), Mathf.Sin(radian), 0) * orbitRadius;

        // 최종적으로 머리가 쫓아가야 할 궤도 상의 목표 위치
        transform.position += offset;
        
    }





    //public void SetTarget()
    //{
    //    if (Attached) {  return; }

    //    // 피셔-예이츠 셔플 알고리즘으로 _ableTargetVectors 배열 섞기
    //    for (int i = _ableTargetVectors.Length - 1; i > 0; i--)
    //    {
    //        // 0부터 i 사이의 랜덤한 인덱스 선택
    //        int randomIndex = Random.Range(0, i + 1);

    //        // 값 바꾸기 (Swap)
    //        Vector3 temp = _ableTargetVectors[i];
    //        _ableTargetVectors[i] = _ableTargetVectors[randomIndex];
    //        _ableTargetVectors[randomIndex] = temp;
    //    }

    //    _body.SetHandTargetDir(_ableTargetVectors);

    //}

    //public void OnHandAttached(int nodeIndex, Vector3 target, int layer)
    //{

    //    _body.IsAttached = true;
    //    _body.SetHeadNode = nodeIndex;
    //    _body.Target = target;

    //}







}
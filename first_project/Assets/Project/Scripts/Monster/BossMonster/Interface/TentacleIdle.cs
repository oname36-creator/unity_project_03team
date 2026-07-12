using System.Collections;
using UnityEngine;

public class TentacleIdle : IMonsterState
{
    private TentacleController _owner;
    private Transform _ownerTransform;

    private Coroutine _swayCoroutine;
    private Vector2 _idleBasePosition;


    private float _rayDistance = 10f; // 아래로 쏘는 Ray의 길이
    private LayerMask _playerLayer;   // 감지할 대상의 레이어 (예: Player)
    private LayerMask _groundLayer;   // 감지할 대상의 레이어 (예: Player)

    public TentacleIdle(TentacleController owner)
    {
        this._owner = owner;
        _ownerTransform = owner.GetComponent<Transform>();

        _playerLayer = LayerMask.GetMask("Player");
        _groundLayer = LayerMask.GetMask("Ground");
    }

    public void Enter()
    {
        // 촉수가 대기할 기본 위치 설정 (뿌리에서 약간 위쪽으로 뻗은 상태)
        _idleBasePosition = _owner.tentacleRoot.position + Vector3.up * 3f;

        // Owner(MonoBehaviour)를 통해 코루틴 실행
        _swayCoroutine = _owner.StartCoroutine(SwayRoutine());
        Debug.Log("TentacleIdle");
    }

    public void Update()
    {
        // 1. Ray 쏘기 (촉수 끝단에서 아래 방향으로)
        Vector2 rayOrigin = _owner.grabberHead.position;
        Debug.DrawRay(rayOrigin, Vector2.down * _rayDistance, Color.red);

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, _rayDistance, _playerLayer);

        // 2. 타겟 감지 및 상태 전이
        if (hit.collider != null)
        {
            // 보스의 타겟을 감지된 오브젝트로 변경
            _owner.Boss.Target = hit.transform;

            // 상태를 Stretch(공격/뻗기)로 전이 
            // (사용하시는 상태 머신의 전환 메서드 이름에 맞게 수정하세요)
            // 예시: _owner.ChangeState("TentacleStretch"); 
        }

        hit = Physics2D.Raycast(rayOrigin, Vector2.down, _rayDistance, _groundLayer);

        if (hit.collider != null) 
        {
            if(hit.collider.gameObject.transform.position.x < _ownerTransform.position.x)
            { return; }
            // 보스의 타겟을 감지된 오브젝트로 변경
            _owner.Boss.Target = hit.transform;
        }
    }

    public void Exit()
    {
        // 상태를 빠져나갈 때 흔들거리는 코루틴 정지
        if (_swayCoroutine != null)
        {
            _owner.StopCoroutine(_swayCoroutine);
            _swayCoroutine = null;
        }
    }

    // 촉수를 자연스럽게 흔들기 위한 코루틴
    private IEnumerator SwayRoutine()
    {
        float time = 0f;

        // 랜덤한 흔들림 시작점 (여러 촉수가 있을 경우 똑같이 안 움직이게)
        float randomOffset = Random.Range(0f, 100f);

        while (true)
        {
            time += Time.deltaTime;

            // Sin, Cos을 이용해 부드러운 8자 또는 타원 궤도 생성
            float xOffset = Mathf.Sin(time * 2f + randomOffset) * 2f;
            float yOffset = Mathf.Cos(time * 3f + randomOffset) * 0.5f;

            // 앞서 구현한 TentacleController의 IK 타겟 좌표를 계속 업데이트
            _owner.IkTargetPosition = _idleBasePosition + new Vector2(xOffset, yOffset);

            yield return null;
        }
    }
}
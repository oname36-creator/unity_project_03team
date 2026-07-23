using System;
using System.Collections;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("이동 설정")]
    // 오프셋 설정
    public Vector2 moveOffset;
    public float speed = 2.0f;

    [Header("관성")]
    public float waitTime = 0.5f;

    // [교정] private 필드 명명 규칙(_camelCase) 적용
    private Vector2 _localStartPos;
    private Vector2 _localEndPos;
    private Vector2 _localTargetPos;

    private Rigidbody2D _rb;
    private bool _isWaiting = false;
    private bool _isInitialized = false;
    public Vector2 Velocity { get; private set; }


    void OnEnable()
    {
        _rb = GetComponent<Rigidbody2D>();
        _isWaiting = false;
        _isInitialized = false;

        // 기존 rb.position(월드 좌표) 대신 로컬 좌표(transform.localPosition)를 사용하여 물리 딜레이 및 맵 스폰 시의 오작동 방지
        _localStartPos = transform.localPosition;
        _localEndPos = _localStartPos + moveOffset;

        // 처음 : 끝점을 향해 출발
        _localTargetPos = _localEndPos;
        
    }
    void FixedUpdate()
    {
        if(!_isInitialized)
        {
            InitializePosition();
        }
        // 물리 엔진 처리시 FixedUpdate에서 처리하면 덜컹거리는 현상이 없음
        if (_isWaiting)
            return;

        // [교정] 로컬 타겟 좌표를 부모 기준으로 월드 좌표로 변환
        Vector2 worldTargetPos = transform.parent != null 
            ? (Vector2)transform.parent.TransformPoint(_localTargetPos) 
            : _localTargetPos;

        Vector2 newPos = Vector3.MoveTowards(_rb.position, worldTargetPos, speed * Time.fixedDeltaTime);
        _rb.MovePosition(newPos);

        Velocity = (newPos - _rb.position) / Time.fixedDeltaTime;
        // 목표 지점에 거의 도달했으면 코루틴 실행
        if (Vector3.Distance(_rb.position, worldTargetPos) < 0.01f)
        {
            StartCoroutine(WaitAndchangeDirection());
        }
    }



    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerControll player = collision.gameObject.GetComponent<PlayerControll>();
            if (player != null)
            {
                // 플레이어에게 내 정보 전달하기
                player.SetActivePlatform(this);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerControll player = collision.gameObject.GetComponent<PlayerControll>();
            if (player != null)
            {
                // 발판에서 벗어났으니 해제 요청
                player.ClearActivePlatform(this);
            }
        }
    }


    // 씬에서 시각적으로 시작점과 끝점 보여주게 하는 함수
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3 start, end;
        if (Application.isPlaying)
        {
            start = transform.parent != null ? transform.parent.TransformPoint(_localStartPos) : (Vector3)_localStartPos;
            end = transform.parent != null ? transform.parent.TransformPoint(_localEndPos) : (Vector3)_localEndPos;
        }
        else
        {
            start = transform.position;
            end = transform.position + (Vector3)moveOffset;
        }

        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireCube(end, transform.localScale);
    }

    private void InitializePosition()
    {
        // [교정] 로컬 좌표 기반 초기화로 변경
        _localStartPos = transform.localPosition;
        _localEndPos = _localStartPos + moveOffset;
        _localTargetPos = _localEndPos;
        _isInitialized = true;
    }

    IEnumerator WaitAndchangeDirection()
    {
        _isWaiting = true;

        yield return new WaitForSeconds(waitTime);  // 설정한 시간만큼 sleep

        // [유지] 기존 방향 왕복 전환 논리 유지
        _localTargetPos = (_localTargetPos == _localEndPos) ? _localStartPos : _localEndPos;

        _isWaiting = false;
    }
}

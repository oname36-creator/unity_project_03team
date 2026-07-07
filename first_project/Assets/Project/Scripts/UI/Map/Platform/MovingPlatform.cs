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

    private Vector2 startPos;
    private Vector2 endPos;
    private Vector2 targetPos;

    private Rigidbody2D rb;
    private bool isWaiting = false;

    void OnEnable()
    {
        rb = GetComponent<Rigidbody2D>();

        // 1. 스폰된 순간의 위치--> 시작점, +offset을 끝점으로 지정
        startPos = rb.position;
        endPos = startPos + moveOffset;

        // 처음 : 끝점을 향해 출발
        targetPos = endPos;
        isWaiting = false;
    }
    void FixedUpdate()
    {
        // 물리 엔진 처리시 FixedUpdate에서 처리하면 덜컹거리는 현상이 없음
        if (isWaiting)
            return;
        Vector2 newPos = Vector3.MoveTowards(rb.position, targetPos, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        // 목표 지점에 거의 도달했으면 코루틴 실행
        if(Vector3.Distance(rb.position, targetPos) < 0.01f)
        {
            StartCoroutine(WaitAndchangeDirection());
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);   // 플레이어 발판 위에 있을 시 같이 움직이게 함
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
    IEnumerator WaitAndchangeDirection()
    {
        isWaiting = true;

        yield return new WaitForSeconds(waitTime);  // 설정한 시간만큼 sleep

        targetPos = (targetPos == endPos) ? startPos : endPos;

        isWaiting = false;
    }

    // 씬에서 시각적으로 시작점과 끝점 보여주게 하는 함수
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3 start = Application.isPlaying ? startPos : (Vector2)transform.position;
        Vector3 end = Application.isPlaying ? endPos : (Vector2)transform.position + moveOffset;

        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireCube(end, transform.localScale);
    }
}

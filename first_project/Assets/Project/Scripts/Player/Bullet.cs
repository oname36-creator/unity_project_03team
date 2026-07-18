using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 15f;       // 총알 속도
    public float damage = 100f;      // 총알 데미지
    public float lifeTime = 3f;     // 화면에 머무를 최대 시간 (예외 처리용)

    private float directionX = 1f;
    private float timer = 0f;

    // PlayerControll에서 총알을 쏠 때 방향을 정해줍니다.
    public void Launch(float direction)
    {
        directionX = direction;
        timer = 0f; // 타이머 초기화

        // 플레이어가 왼쪽을 보고 쏘면 총알 이미지도 왼쪽으로 뒤집기
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * direction, transform.localScale.y, transform.localScale.z);
    }

    void Update()
    {
        // 지정된 방향으로 매 프레임 이동
        transform.Translate(Vector2.right * directionX * speed * Time.deltaTime);

        // 일정 시간이 지나면 자동으로 풀에 반환 (메모리 누수 방지)
        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            ObjectPoolManager.Instance.ReturnBullet(gameObject);
        }
    }

    // 몬스터나 벽에 부딪혔을 때 처리
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // PlayerControll에서 사용한 enemyTag("Monster")와 일치시킵니다.
        if (collision.CompareTag("Monster"))
        {
            Debug.Log($"총알 적중: {collision.name}");

           
            ObjectPoolManager.Instance.ReturnBullet(gameObject);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Debug.Log("벽이나 바닥에 부딪혀 총알 소멸");
            ObjectPoolManager.Instance.ReturnBullet(gameObject);
        }
       
        
    }
}

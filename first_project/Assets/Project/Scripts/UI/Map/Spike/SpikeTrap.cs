using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [Header("Trap Settings")]
    public float damage = 10f;          // 가시 대미지 수치

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 충돌한 오브젝트가 "Player" 태그를 가졌는지 확인
        if (collision.CompareTag("Player"))
        {
            PlayerStatus playerStatus = collision.GetComponent<PlayerStatus>();
            PlayerControll playerControll = collision.GetComponent<PlayerControll>();

            // 2. 컴포넌트가 존재하고, 플레이어가 살아있는 상태인지 체크
            if (playerStatus != null && !playerStatus.isDead)
            {
                // 3. 무적 상태가 아닐 때만 대미지 및 넉백 효과를 적용합니다.
                if (!playerStatus.isInvincible)
                {
                    Debug.Log($"가시에 찔림! 대미지: {damage}");

                    // 체력 차감 (음수값 전달)
                     playerStatus.ChangeHp(-damage);

                    // 유니티 데이터 매니저와 HP 연동
                    if (DataManager.Instance != null)
                    {
                        DataManager.Instance.PlayerHp = (int)playerStatus.currentHp;
                    }

                    // 4. 가시의 위치를 기준으로 플레이어의 넉백&무적 코루틴을 호출합니다.
                    if (playerControll != null)
                    {
                        playerControll.StartCoroutine("KnockbackAndInvincibleRoutine", transform.position);
                    }
                }
            }
        }
    }
}

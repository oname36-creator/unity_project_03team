using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private PlayerStatus playerStatus;

    void Awake()
    {
        // 부모의 Status 컴포넌트 가져오기
        playerStatus = GetComponentInParent<PlayerStatus>();
    }

    // 💡 내 무기 상자가 활성화되어 있을 때, 몬스터와 부딪히면 발동!
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 상대방이 "Monster" 레이어인지 검사
        if (collision.gameObject.layer == LayerMask.NameToLayer("Monster"))
        {
            float damage = (playerStatus != null) ? playerStatus.currentDamage : 10f;

            // 2. 🚨 [에러 해결책] 아직 MonsterStatus가 없으므로 로그만 띄우고 기본 기능 처리!
            Debug.Log($"⚔️ [타격 성공] 무기가 {collision.name}을 때렸습니다! (가상 데미지: {damage})");

            // 3. (옵션) 타격감을 눈으로 확인하고 싶다면 몬스터를 파괴하거나 비활성화해 봅니다.
            // 나중에 몬스터 팀원의 코드가 오면 이 자리에 데미지 주는 코드를 넣으면 됩니다.
            // Destroy(collision.gameObject); // ⬅️ 필요하면 주석을 해제해서 몬스터가 죽는 걸 테스트해 보세요!
        }
    }
}
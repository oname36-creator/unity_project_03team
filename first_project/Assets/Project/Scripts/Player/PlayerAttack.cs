using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private PlayerStatus playerStatus;

    // 🔔 [수정] 복잡한 내부 변수와 오타를 다 지우고, 
    // 몬스터가 읽으러 올 때 부모(PlayerStatus)의 최신 대미지를 다이렉트로 넘겨줍니다.
    public int Damage
    {
        get
        {
            if (playerStatus != null)
            {
                // PlayerStatus의 currentDamage(예: 50f)를 반올림해서 int로 변환
                return Mathf.RoundToInt(playerStatus.currentDamage);
            }

            // 만약 부모 컴포넌트를 못 찾았다면 기본값 10 반환 (안전장치)
            return 10;
        }
    }

    void Awake()
    {
        // 부모의 Status 컴포넌트 가져오기
        playerStatus = GetComponentInParent<PlayerStatus>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Monster"))
        {
            // 로그에서도 실제 주입되는 대미지(Damage)를 찍도록 변경
            Debug.Log($"⚔️ [타격 성공] {collision.name}에게 {Damage} 대미지를 꽂았습니다!");
        }
    }
}
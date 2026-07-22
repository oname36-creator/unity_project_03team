using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Hit Stop Settings")]
    [Tooltip("하이어라키에 배치한 HitStopManager 오브젝트를 여기에 드래그 앤 드롭 하세요.")]
    public HitStopManager hitStopManager;
    [Tooltip("타격 시 멈출 시간입니다.")]
    public float hitStopDuration = 0.07f;

    [Header("Hit Effect Settings")]
    [Tooltip("타격 성공 시 나타날 스파크나 파티클 프리팹을 넣어주세요.")]
    public GameObject hitEffectPrefab; 
    [Tooltip("이펙트가 화면에서 사라질 시간(초)입니다.")]
    public float effectDestroyTime = 0.5f;

    private PlayerStatus playerStatus;

    public int Damage
    {
        get
        {
            if (playerStatus != null)
            {
                return Mathf.RoundToInt(playerStatus.currentDamage);
            }
            return 100;
        }
    }

    void Awake()
    {
        playerStatus = GetComponentInParent<PlayerStatus>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Monster"))
        {
            //Debug.Log($"⚔️ [타격 성공] {collision.name}에게 {Damage} 대미지를 꽂았습니다!");

            // 1. 히트 스톱 실행 (오브젝트가 존재하고, '활성화'되어 있을 때만 실행)
            if (hitStopManager != null && hitStopManager.gameObject.activeInHierarchy)
            {
                hitStopManager.TriggerHitStop(hitStopDuration);
            }
            else
            {
                //Debug.LogWarning("HitStopManager가 인스펙터에 없거나 비활성화 상태입니다. 히트스톱을 건너뜁니다.");
            }

            // 2. 피격 이펙트 생성 (이제 히트 스톱 에러에 막히지 않고 무조건 실행됩니다)
            if (hitEffectPrefab != null)
            {
                Vector2 hitPoint = collision.ClosestPoint(transform.position);
                GameObject effect = Instantiate(hitEffectPrefab, hitPoint, Quaternion.identity);
                Destroy(effect, effectDestroyTime);
            }
        }
    }
}
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
            Debug.Log($"⚔️ [타격 성공] {collision.name}에게 {Damage} 대미지를 꽂았습니다!");

           
            if (hitStopManager != null)
            {
                hitStopManager.TriggerHitStop(hitStopDuration);
            }

            
            if (hitEffectPrefab != null)
            {
                // 현재 히트박스 콜라이더와 몬스터 콜라이더가 만나는 가장 가까운 접점 좌표 구하기
                Vector2 hitPoint = collision.ClosestPoint(transform.position);

                // 해당 위치에 이펙트 생성
                GameObject effect = Instantiate(hitEffectPrefab, hitPoint, Quaternion.identity);

                // 생성 후 일정 시간이 지나면 자동으로 메모리에서 삭제
                Destroy(effect, effectDestroyTime);
            }
        }
    }
}
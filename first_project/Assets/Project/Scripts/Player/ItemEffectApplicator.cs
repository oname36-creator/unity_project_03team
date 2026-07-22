using UnityEngine;
using System.Collections;

public class ItemEffectApplicator : MonoBehaviour
{
    private PlayerStatus status;
    private SpriteRenderer playerSprite;
    private Material playerMaterial;

    void Awake()
    {
        status = GetComponent<PlayerStatus>();
        playerSprite = GetComponentInChildren<SpriteRenderer>();
    }

    public void ApplyItemEffect(ItemData data)
    {
        if (data == null) return;

        Debug.Log($"{data.itemName} 효과 발동!");

        switch (data.effectType)
        {
            case ItemEffectType.HealHP:
                status.ChangeHp(data.effectValue);
                break;

            case ItemEffectType.Stealth:
                StartCoroutine(StealthRoutine(data.duration));
                break;

            case ItemEffectType.Sword:
                // ★ [수정] 범위 인자(duration) 제거 -> (추가 공격력, 사용 횟수)
                status.EnableSwordBuff(data.effectValue, (int)data.usageCount);
                break;

            case ItemEffectType.Gun:
                status.EnableGunBuff((int)data.usageCount);
                break;
        }
    }

    public void ExecuteItemEffectByID(int itemNumber)
    {
        Debug.Log($"ItemEffectApplicator: {itemNumber}번 아이템 효과 실행");

        switch (itemNumber)
        {
            case 1:
                if (status != null) status.ChangeHp(50f);
                if (DataManager.Instance != null && status != null)
                {
                    DataManager.Instance.PlayerHp = (int)status.currentHp;
                }
                break;

            case 2:
                StartCoroutine(StealthRoutine(5f));
                break;

            case 3:
                if (status != null && !status.hasSword)
                {
                   
                    status.EnableSwordBuff(50f, 6);
                    Debug.Log(" 검 장착 완료! 6회 공격 가능.");
                }
                break;

            case 4:
                if (status != null && !status.hasGun)
                {
                    status.EnableGunBuff(6);
                    Debug.Log(" 총기 버프 활성화 완료! 이제 J키로 발사 가능.");
                }
                break;

            default:
                Debug.LogWarning($"정의되지 않은 아이템 번호입니다: {itemNumber}");
                break;
        }
    }

    private IEnumerator StealthRoutine(float duration)
    {
        status.isStealth = true;
        status.isInvincible = true;

        if (playerSprite != null)
        {
            playerSprite.color = new Color(1f, 1f, 1f, 0.6f);
        }

        yield return new WaitForSeconds(duration);

        status.isStealth = false;
        status.isInvincible = false;

        if (playerSprite != null)
        {
            playerSprite.color = new Color(1f, 1f, 1f, 1f);
        }

        Debug.Log("은신 및 은신 무적 종료!");
    }
}
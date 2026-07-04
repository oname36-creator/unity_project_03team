using UnityEngine;
using System.Collections.Generic;

public class ItemEffectApplicator : MonoBehaviour
{
    private PlayerStatus status;
    private SpriteRenderer playerSprite;
    // 재질을 저장할 변수 추가
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
        }
    }

    public void ExecuteItemEffectByID(int itemNumber)
    {
        Debug.Log($"ItemEffectApplicator: {itemNumber}번 아이템 효과 실행");

        switch (itemNumber)
        {
            case 1:
                DataManager.Instance.PlayerHp += 50;
                break;

            case 2:
                StartCoroutine(StealthRoutine(5f));
                break;
        }
    }

    private System.Collections.IEnumerator StealthRoutine(float duration)
    {
        status.isStealth = true;
        float elapsed = 0f;

        
        while (elapsed < duration)
        {
            if (playerSprite != null)
            {
                
                playerSprite.color = new Color(1f, 1f, 1f, 0.6f);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        status.isStealth = false;

        // 은신 종료 시 원래대로 원상복구
        if (playerSprite != null)
        {
            playerSprite.color = new Color(1f, 1f, 1f, 1f);
        }
    }
}
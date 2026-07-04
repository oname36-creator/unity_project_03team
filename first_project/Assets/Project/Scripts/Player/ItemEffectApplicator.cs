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
            case ItemEffectType.Sword:
                // ItemData에 적힌 기획 데이터를 플레이어 상태창으로 전달
                // effectValue = 공격력 증가량, duration = 범위 증가량, usageCount = 6회
                status.EnableSwordBuff(data.effectValue, data.duration, (int)data.usageCount);
                break;
            case ItemEffectType.Gun:
                // usageCount(예: 6발)만큼 총 버프 활성화
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
                DataManager.Instance.PlayerHp += 50;
                break;

            case 2:
                StartCoroutine(StealthRoutine(5f));
                break;
            case 3:
                // (추가공격력, 추가범위, 횟수)
                status.EnableSwordBuff(15f, 2.0f, 6);
                break;
        }
    }

    private System.Collections.IEnumerator StealthRoutine(float duration)
    {
        status.isStealth = true;

        // 시작할 때 딱 한 번만 알파값 변경
        if (playerSprite != null)
        {
            playerSprite.color = new Color(1f, 1f, 1f, 0.6f);
        }

        // 매 프레임 돌릴 필요 없이 duration만큼 통째로 대기
        yield return new WaitForSeconds(duration);

        status.isStealth = false;

        // 종료할 때 딱 한 번만 원상복구
        if (playerSprite != null)
        {
            playerSprite.color = new Color(1f, 1f, 1f, 1f);
        }
    }
}
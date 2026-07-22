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

        //Debug.Log($"{data.itemName} 효과 발동!");

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
        //Debug.Log($"ItemEffectApplicator: {itemNumber}번 아이템 효과 실행");

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
                // 💡 [안전장치] 이미 검을 들고 있다면 중복 장착으로 횟수가 초기화되는 것을 막아줍니다.
                if (status != null && !status.hasSword)
                {
                    status.EnableSwordBuff(15f, 2.0f, 6);
                    //Debug.Log("⚔️ 검 장착 완료! 6회 공격 가능.");
                }
                break;

            case 4:
                // 💡 [안전장치] 이미 총을 들고 있다면 중복 장착 방지
                if (status != null && !status.hasGun)
                {
                    status.EnableGunBuff(6);
                    //Debug.Log("🔫 총기 버프 활성화 완료! 이제 J키로 발사 가능.");
                }
                break;

            default:
                //Debug.LogWarning($"정의되지 않은 아이템 번호입니다: {itemNumber}");
                break;
        }
    }

    private System.Collections.IEnumerator StealthRoutine(float duration)
    {
        status.isStealth = true;
        // 💡 [추가] 은신이 시작되면 무적 상태도 함께 켜줍니다!
        status.isInvincible = true;

        // 시작할 때 딱 한 번만 알파값 변경 (반투명)
        if (playerSprite != null)
        {
            playerSprite.color = new Color(1f, 1f, 1f, 0.6f);
        }

        // 지정된 시간(duration) 동안 대기합니다.
        yield return new WaitForSeconds(duration);

        status.isStealth = false;
        // 💡 [추가] 은신이 끝나면 무적 상태도 함께 꺼줍니다!
        status.isInvincible = false;

        // 종료할 때 딱 한 번만 원상복구 (불투명)
        if (playerSprite != null)
        {
            playerSprite.color = new Color(1f, 1f, 1f, 1f);
        }

        //Debug.Log("은신 및 은신 무적 종료!");
    }
}
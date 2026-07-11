using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("4개의 슬롯에 들어있는 ItemImage 컴포넌트들")]
    public Image[] slotImages;

    [Header("아이템 번호(ID)별 매칭할 스프라이트 에셋")]
    public Sprite redPotionSprite; // 예: ID 1번
    public Sprite swordSprite;     // 예: ID 2번
    public Sprite gunSprite;       // 예: ID 3번
    public Sprite transparentPotionSprite; // 예: ID 4번 (스크린샷 투명포션)

    void Start()
    {
        // 💡 수정: Start 대신 조금 더 안전하게 예외 처리를 하거나, UpdateInventoryUI 내부에서 체크하도록 둡니다.
        UpdateInventoryUI();
    }

    public void UpdateInventoryUI()
    {
       
        if (DataManager.Instance == null)
        {
            Debug.LogWarning("DataManager가 아직 준비되지 않아 UI 갱신을 잠시 미룹니다.");
            return;
        }

        // 4개의 슬롯을 전부 검사합니다.
        for (int i = 0; i < slotImages.Length; i++)
        {
            // 💡 혹시 인스펙터에서 slotImages 배열 칸을 비워두었는지도 체크 (에러 2중 차단)
            if (slotImages[i] == null) continue;

            int itemID = DataManager.Instance.PlayerInventory[i];

            if (itemID == 0)
            {
                slotImages[i].sprite = null;
                slotImages[i].color = new Color(1f, 1f, 1f, 0f);
            }
            else
            {
                Sprite targetSprite = GetSpriteByID(itemID);

                if (targetSprite != null)
                {
                    slotImages[i].sprite = targetSprite;
                    slotImages[i].color = new Color(1f, 1f, 1f, 1f);
                }
            }
        }
    }

    // 🆔 아이템 ID 번호를 스프라이트 에셋으로 바꿔주는 징검다리 함수
    private Sprite GetSpriteByID(int id)
    {
        switch (id)
        {
            case 1: return redPotionSprite; // 1번은 빨간 물약
            case 2: return swordSprite;     // 2번은 검
            case 3: return gunSprite;       // 3번은 총
            case 4: return transparentPotionSprite; // 4번은 투명 물약
            default:
                Debug.LogWarning($"ID {id}번에 지정된 아이템 이미지가 없습니다!");
                return null;
        }
    }
}
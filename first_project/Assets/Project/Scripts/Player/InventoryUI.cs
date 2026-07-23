using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("4개의 슬롯에 들어있는 ItemImage 컴포넌트들")]
    public Image[] slotImages;

    [Header("아이템 번호(ID)별 매칭할 스프라이트 에셋")]
    public Sprite redPotionSprite; // ID 1
    public Sprite swordSprite;     // ID 2
    public Sprite gunSprite;       // ID 3
    public Sprite transparentPotionSprite; // ID 4

    void Start()
    {
        // 게임 시작할 때 한 번 UI를 깨끗하게 세팅합니다.
        UpdateInventoryUI();
    }

    // 🔄 데이터 매니저의 배열을 읽어서 UI를 새로고침하는 함수
    public void UpdateInventoryUI()
    {
        if (DataManager.Instance == null) return;

        for (int i = 0; i < slotImages.Length; i++)
        {
            if (slotImages[i] == null) continue;

            int itemID = DataManager.Instance.PlayerInventory[i];

            // 0번이면 빈 칸이므로 이미지를 지우고 오브젝트를 숨깁니다.
            if (itemID == 0)
            {
                slotImages[i].sprite = null;
                slotImages[i].gameObject.SetActive(false);
            }
            // 아이템이 들어있다면 ID에 맞는 이미지를 꽂고 오브젝트를 켭니다!
            else
            {
                Sprite targetSprite = GetSpriteByID(itemID);

                if (targetSprite != null)
                {
                    slotImages[i].gameObject.SetActive(true);
                    slotImages[i].sprite = targetSprite;
                    slotImages[i].color = Color.white; // 투명도 100% (불투명)

                    // 크기가 쪼그라들지 않도록 부모 슬롯에 꽉 차게 강제 정렬
                    RectTransform rect = slotImages[i].GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        rect.anchorMin = Vector2.zero;
                        rect.anchorMax = Vector2.one;
                        rect.offsetMin = Vector2.zero;
                        rect.offsetMax = Vector2.zero;
                    }
                }
            }
        }
    }

    private Sprite GetSpriteByID(int id)
    {
        switch (id)
        {
            case 1: return redPotionSprite;
            case 2: return swordSprite;
            case 3: return gunSprite;
            case 4: return transparentPotionSprite;
            default: return null;
        }
    }
}
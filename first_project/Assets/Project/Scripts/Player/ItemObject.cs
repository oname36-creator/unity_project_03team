using UnityEngine;

public class ItemObject : MonoBehaviour
{
    // 유니티 인스펙터에서 아까 만든 에셋(RedPotion이나 StealthItem 등)을 여기에 쏙 넣어줍니다.
    public ItemData itemData;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 부딪힌 대상이 플레이어라면
        if (collision.CompareTag("Player"))
        {
            // [삭제] 플레이어 스크립트를 가져올 필요가 없습니다.
            // PlayerControll player = collision.GetComponent<PlayerControll>();

            // 데이터 매니저에게 이 아이템의 데이터를 주며 추가하라고 명령합니다!
            // 아이템 데이터에 itemNumber가 있다고 가정합니다 (ItemData.itemNumber)
            DataManager.Instance.AddItem(itemData.itemNumber);

            // 아이템 먹었으니 필드에서 삭제
            Destroy(gameObject);
        }
    }
}
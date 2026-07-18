using UnityEngine;

public class BoxObject : MonoBehaviour
{
    private ItemData[] rewardItems;

    // 스폰 지점에 보상 아이템 목록을 주입받음
    public void SetRewardItems(ItemData[] Items)
    {
        rewardItems = Items;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 디버깅용 로그: 어떤 오브젝트와 충돌했는지 콘솔에 출력합니다.
        Debug.Log($"[BoxObject] 충돌 감지됨: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");

        if(collision.CompareTag("Player"))
        {
            Debug.Log("[BoxObject] 플레이어 충돌 확인 -> 상자 열기 실행");
            OpenBox();
        }
    }

    private void OpenBox()
    {
        #region AddItem
        if(rewardItems != null && rewardItems.Length > 0)
        {
            // 보상 목록 중 무작위로 아이템 1개 선택
            ItemData selectedItem = rewardItems[Random.Range(0, rewardItems.Length)];
            if(selectedItem != null)
            {
                // DataManager에 아이템 추가
                DataManager.Instance.AddItem(selectedItem.itemNumber);
                Debug.Log($"[BoxObject] 상자를 열어 아이템을 획득했습니다! 이름: {selectedItem.itemName} (번호: {selectedItem.itemNumber})");
            }
        }
        #endregion
        else 
        {
            Debug.LogWarning("보상 아이템 목록이 없음");
        }

        // 획득 후 오브젝트 풀로 반환
        ObjectPoolManager.Instance.BoxPush(gameObject);
    }
}

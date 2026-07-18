using UnityEngine;

public class BoxObject : MonoBehaviour
{
    private ItemData[] _rewardItems;
    private Collider2D _triggerCollider;
    private bool _isTriggered = false;

    private void Awake()
    {
        _triggerCollider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        _isTriggered = false;
        if(_triggerCollider)
        {
            _triggerCollider.enabled = true;
        }
    }
    // 스폰 지점에 보상 아이템 목록을 주입받음
    public void SetRewardItems(ItemData[] Items)
    {
        _rewardItems = Items;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 디버깅용 로그: 어떤 오브젝트와 충돌했는지 콘솔에 출력합니다.
        Debug.Log($"[BoxObject] 충돌 감지됨: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");

        // 중복 실행 방지를 위해 리턴
        if (_isTriggered) return;
        if(collision.CompareTag("Player"))
        {
            // 플레이어 접촉 즉시 콜라이더를 비활성화
            _isTriggered = true;
            if(_triggerCollider != null)
            {
                _triggerCollider.enabled = false;
            }
            Debug.Log("[BoxObject] 플레이어 충돌 확인 -> 상자 열기 실행");
            OpenBox();
        }
    }

    private void OpenBox()
    {
        #region AddItem
        if(_rewardItems != null && _rewardItems.Length > 0)
        {
            // 보상 목록 중 무작위로 아이템 1개 선택
            ItemData selectedItem = _rewardItems[Random.Range(0, _rewardItems.Length)];
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

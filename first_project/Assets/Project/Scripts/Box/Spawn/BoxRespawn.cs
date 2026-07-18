using System;
using UnityEngine;
using UnityEngine.UIElements;
public class BoxRespawn : MonoBehaviour
{
    #region Event
    private void OnEnable()
    {
        MapEvent.onRequestBoxSpawn += HandleSpawnRequest;
    }
    private void OnDisable()
    {
        MapEvent.onRequestBoxSpawn -= HandleSpawnRequest;
    }
    #endregion

    #region CreateBox
    private void HandleSpawnRequest(Vector3 pos, ItemData[] items, Action<GameObject> onSpawned)
    {
        GameObject box = Respawn(pos, items);
        onSpawned?.Invoke(box);
    }
    #endregion

    private GameObject Respawn(Vector3 pos, ItemData[] items)
    {
        
        GameObject box = ObjectPoolManager.Instance.BoxPop();
        if (box == null) return null;

        // 2D 물리 충돌을 위해 Z축 좌표를 0f로 강제 고정하여 스폰합니다. (타일맵 Z축 오프셋에 영향받지 않도록 방지)
        box.transform.position = new Vector3(pos.x, pos.y, 0f);

        // 상자 내부 컴포넌트에 아이템 리스트 세팅
        if(box.TryGetComponent<BoxObject>(out var boxObject))
        {
            boxObject.SetRewardItems(items);
        }
        box.SetActive(true);
        return box;
    }

    


   
}

using UnityEngine;

public class MapChunkBoundaryTrigger : MonoBehaviour
{
    private MapChunk parentChunk;

    void Start()
    {
        parentChunk = GetComponentInParent<MapChunk>();
        
        // 예외 처리
        if(parentChunk == null )
        {
            Debug.LogError($"{gameObject.name}: 부모 오브젝트에서 MapChunk를 찾을 수 없습니다.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            if(parentChunk != null && parentChunk.cameraBoundaryCollider != null)
            {
                // 플레이어가 진입한 맵 청크의 카메라 경계 콜라이더를 매니저에 전달합니다.
                CameraConfinerManager.Instance.UpdateBoundary(parentChunk.cameraBoundaryCollider, parentChunk.enableYTracking);
            }
            else
            {
                Debug.LogWarning($"{gameObject.name}: 전환할 cameraBoundaryCollider가 지정되지 않았습니다.");
            }
        }
    }
}

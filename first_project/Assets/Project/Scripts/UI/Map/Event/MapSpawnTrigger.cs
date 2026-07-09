using UnityEngine;

public class MapSpawnTrigger : MonoBehaviour
{
    private Collider2D triggerCollider;
    private void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            // 매니저에게 알림
            MapEvent.onPlayerHitSpawnTrigger?.Invoke();

            triggerCollider.enabled = false;
        }
    }

    private void OnEnable()
    {
        if (triggerCollider != null)
        {
            triggerCollider.enabled = true;
        }
    }

}

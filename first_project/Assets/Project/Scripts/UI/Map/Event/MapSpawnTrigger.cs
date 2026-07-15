using UnityEngine;

public class MapSpawnTrigger : MonoBehaviour
{
    private Collider2D triggerCollider;
    private bool _isTriggered = false;
    private void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 트리거가 한 번이라도 작동했다면 중복 실행 방지
        if (_isTriggered) return;

        if(other.CompareTag("Player"))
        {
            _isTriggered = true;

            // 매니저에게 알림
            MapEvent.onPlayerHitSpawnTrigger?.Invoke();

            triggerCollider.enabled = false;
        }
    }

    private void OnEnable()
    {
        _isTriggered = false;

        if (triggerCollider != null)
        {
            triggerCollider.enabled = true;
        }
    }

}

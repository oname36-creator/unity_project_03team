using UnityEngine;

public class SlowPad : MonoBehaviour
{
    private PlayerStatus playerStatus;

    void Start()
    {
        playerStatus = FindAnyObjectByType<PlayerStatus>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && playerStatus != null)
        {
            // 플레이어의 이동속도 감소
            playerStatus.isSlow = true;
        }
    }
}
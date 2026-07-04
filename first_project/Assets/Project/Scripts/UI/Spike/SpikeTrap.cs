using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    public int damage = 10;

    // Trigger 발동될 시 실행
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            Debug.Log("가시에 찔림!" + damage);

            // 실전에서는 아래처럼 플레이어의 체력을 깎는 코드를 연결함
            // collision.GetComponent<PlayerHealth>().TakeDamage(damage);
        }
    }
}

using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    [Header("Player Stats")]
    public float maxHp = 100f;
    public float currentHp = 100f;

    [Header("Player States")]
    public bool isStealth = false;
    public bool isGrounded = false;
    public bool isDead = false;

    // HP 변경을 안전하게 처리하는 함수
    public void ChangeHp(float amount)
    {
        if (isDead) return;

        currentHp += amount;
        currentHp = Mathf.Clamp(currentHp, 0f, maxHp); // 0 ~ MaxHP 사이로 고정

        if (currentHp <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("플레이어가 사망했습니다.");
        // 여기에 사망 애니메이션 트리거 등을 넣습니다.
    }
}

using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    [Header("Base Status")]
    public float baseDamage = 10f;       // 기본 공격력
    public float baseAttackRange = 1.5f; // 기본 공격 범위
    [Header("Player Stats")]
    public float maxHp = 100f;
    public float currentHp = 100f;

    [Header("Player States")]
    public bool isStealth = false;
    public bool isGrounded = false;
    public bool isDead = false;
    public bool isAerial = false;
    [HideInInspector] public bool hasGun = false; // 현재 총을 들고 있는가?
    private int gunAttackCount = 0;               // 총 남은 총알 수

    public float currentDamage { get; private set; }
    public float currentAttackRange { get; private set; }

    private int swordAttackCount = 0; // 검 남은 사용 횟수
    [HideInInspector] public float speedMultiplier = 1f;
    private bool _isSlow = false;
    public bool isSlow
    {
        get => _isSlow;
        set
        {
            _isSlow = value;
            // isSlow가 true면 0.4배속, false면 1배속(정상)
            speedMultiplier = _isSlow ? 0.4f : 1f;
            Debug.Log($"isSlow 상태 변경: {_isSlow} -> 현재 속도 배율: {speedMultiplier}");
        }
    }

    void Awake()
    {
        ResetAttackStatus();
    }

    // 검 버프 활성화
    public void EnableSwordBuff(float bonusDamage, float bonusRange, int count)
    {
        swordAttackCount = count;
        currentDamage = baseDamage + bonusDamage;
        currentAttackRange = baseAttackRange + bonusRange;

        Debug.Log($"검 장착! 현재 공격력: {currentDamage}, 현재 범위: {currentAttackRange}, 남은 횟수: {swordAttackCount}");
    }

    // 플레이어가 공격 행동을 '실행(Execute)'할 때 이 메서드를 호출해야 합니다!
    public void OnAttackExecute()
    {
        if (swordAttackCount > 0)
        {
            swordAttackCount--;
            Debug.Log($"검 사용됨! 남은 횟수: {swordAttackCount}");

            if (swordAttackCount <= 0)
            {
                ResetAttackStatus();
                Debug.Log("검의 내구도가 다하여 기본 상태로 돌아갑니다.");
            }
        }
    }

    // 능력치 원상 복구
    private void ResetAttackStatus()
    {
        swordAttackCount = 0;
        currentDamage = baseDamage;
        currentAttackRange = baseAttackRange;
    }

    // 총 버프 활성화
    public void EnableGunBuff(int count)
    {
        gunAttackCount = count;
        hasGun = true;
        Debug.Log($"총 장착! 장탄수: {gunAttackCount}발");
    }

    // 총 쏠 때마다 카운트 차감
    public void OnGunAttackExecute()
    {
        if (gunAttackCount > 0)
        {
            gunAttackCount--;
            Debug.Log($"탕! 남은 총알: {gunAttackCount}발");

            if (gunAttackCount <= 0)
            {
                hasGun = false;
                Debug.LogWarning("⚠️ [알림] 총알을 모두 소모하여 기본 상태로 돌아갑니다.");
            }
        }
    }

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

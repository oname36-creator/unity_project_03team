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

    [HideInInspector] public bool hasGun = false;   // 현재 총을 들고 있는가?
    [HideInInspector] public bool hasSword = false;  // ★ [추가] 현재 검을 들고 있는가?

    private int gunAttackCount = 0;                 // 총 남은 총알 수
    private int swordAttackCount = 0;               // 검 남은 사용 횟수

    [Header("New States (피격 및 무적)")]
    public bool isHurt = false;         // 현재 넉백/피격 중인가? (이동안 조작 불가)
    public bool isInvincible = false;   // 현재 무적 상태인가?

    public float currentDamage { get; private set; }
    public float currentAttackRange { get; private set; }

    [HideInInspector] public float speedMultiplier = 1f;
    private bool _isSlow = false;
    public bool isSlow
    {
        get => _isSlow;
        set
        {
            _isSlow = value;
            speedMultiplier = _isSlow ? 0.4f : 1f;
            Debug.Log($"isSlow 상태 변경: {_isSlow} -> 현재 속도 배율: {speedMultiplier}");
        }
    }

    void Awake()
    {
        ResetAttackStatus();
    }

    // ★ 검 버프 활성화 (총 해제 로직 포함)
    public void EnableSwordBuff(float bonusDamage, float bonusRange, int count)
    {
        // 새로운 무기를 들면 기존 무기 상태는 해제해주는 것이 안전합니다.
        hasGun = false;
        gunAttackCount = 0;

        hasSword = true;
        swordAttackCount = count;
        currentDamage = baseDamage + bonusDamage;
        currentAttackRange = baseAttackRange + bonusRange;

        Debug.Log($"검 장착! 현재 공격력: {currentDamage}, 현재 범위: {currentAttackRange}, 남은 횟수: {swordAttackCount}");
    }

    // ★ 검 공격을 실행할 때 호출 (OnAttackExecute에서 이름을 분리하여 명확하게 변경)
    public void OnSwordAttackExecute()
    {
        if (hasSword && swordAttackCount > 0)
        {
            swordAttackCount--;
            Debug.Log($"스윙! 검 사용됨! 남은 횟수: {swordAttackCount}");

            if (swordAttackCount <= 0)
            {
                ResetAttackStatus();
                Debug.LogWarning("⚠️ [알림] 검의 내구도가 다하여 기본 상태로 돌아갑니다.");
            }
        }
    }

    // ★ 맨손 공격을 실행할 때 호출할 함수 (기존 함수는 맨손용으로 유지)
    public void OnAttackExecute()
    {
        Debug.Log("맨손 공격 실행됨.");
        // 맨손 공격 시 추가적인 내구도 차감 등은 없음
    }

    // 능력치 원상 복구 (검 해제)
    private void ResetAttackStatus()
    {
        hasSword = false; // ★ 상태 해제
        swordAttackCount = 0;
        currentDamage = baseDamage;
        currentAttackRange = baseAttackRange;
    }

    // 총 버프 활성화 (검 해제 로직 포함)
    public void EnableGunBuff(int count)
    {
        // 새로운 무기를 들면 기존 무기 상태는 해제
        ResetAttackStatus();

        gunAttackCount = count;
        hasGun = true;
        Debug.Log($"총 장착! 장탄수: {gunAttackCount}발");
    }

    // 총 쏠 때마다 카운트 차감
    public void OnGunAttackExecute()
    {
        if (hasGun && gunAttackCount > 0)
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
        currentHp = Mathf.Clamp(currentHp, 0f, maxHp);

        if (currentHp <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("플레이어가 사망했습니다.");

        if(SceneManagerEx.Instance != null)
        {
            SceneManagerEx.Instance.GameOver();
        }
    }
}
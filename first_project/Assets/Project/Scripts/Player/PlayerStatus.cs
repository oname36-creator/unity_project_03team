using UnityEngine;
using System.Collections;

public class PlayerStatus : MonoBehaviour
{
    [Header("Base Status")]
    public float baseDamage = 50f;       // 기본 공격력
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
            //Debug.Log($"isSlow 상태 변경: {_isSlow} -> 현재 속도 배율: {speedMultiplier}");
        }
    }
    private Coroutine slowCoroutine;
    void Awake()
    {
        ResetAttackStatus();
    }

    public void ApplySlow(float duration = 3f)
    {
        if (isDead) return;

        
        if (slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
        }

        
        slowCoroutine = StartCoroutine(SlowTimer(duration));
    }

    
    private IEnumerator SlowTimer(float duration)
    {
        isSlow = true; // 슬로우 활성화

        
        yield return new WaitForSeconds(duration);

        isSlow = false; // 슬로우 해제
        slowCoroutine = null;
        //Debug.Log("슬로우 상태가 해제되어 원래 속도로 복구되었습니다.");
    }
    public void EnableSwordBuff(float bonusDamage, float bonusRange, int count)
    {
      
        hasGun = false;
        gunAttackCount = 0;

        hasSword = true;
        swordAttackCount = count;
        currentDamage = baseDamage + bonusDamage;
        currentAttackRange = baseAttackRange + bonusRange;

        //Debug.Log($"검 장착! 현재 공격력: {currentDamage}, 현재 범위: {currentAttackRange}, 남은 횟수: {swordAttackCount}");
    }
    public void OnSwordAttackExecute()
    {
        if (hasSword && swordAttackCount > 0)
        {
            swordAttackCount--;
            //Debug.Log($"스윙! 검 사용됨! 남은 횟수: {swordAttackCount}");

            if (swordAttackCount <= 0)
            {
                ResetAttackStatus();
                //Debug.LogWarning("⚠️ [알림] 검의 내구도가 다하여 기본 상태로 돌아갑니다.");

                Object.FindAnyObjectByType<InventoryUI>()?.UpdateInventoryUI();
            }
        }
    }

  
    public void OnAttackExecute()
    {
        //Debug.Log("맨손 공격 실행됨.");
        
    }

    // 능력치 원상 복구 (검 해제)
    private void ResetAttackStatus()
    {
        hasSword = false; 
        swordAttackCount = 0;
        currentDamage = baseDamage;
        currentAttackRange = baseAttackRange;
    }

    
    public void EnableGunBuff(int count)
    {

        ResetAttackStatus();

        gunAttackCount = count;
        hasGun = true;
        //Debug.Log($"총 장착! 장탄수: {gunAttackCount}발");
    }


    public void OnGunAttackExecute()
    {
        if (hasGun && gunAttackCount > 0)
        {
            gunAttackCount--;
            //Debug.Log($"탕! 남은 총알: {gunAttackCount}발");

            if (gunAttackCount <= 0)
            {
                hasGun = false;
                //Debug.LogWarning(" [알림] 총알을 모두 소모하여 기본 상태로 돌아갑니다.");

                Object.FindAnyObjectByType<InventoryUI>()?.UpdateInventoryUI();
            }
        }
    }

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
        //Debug.Log("플레이어가 사망했습니다.");

        if(SceneManagerEx.Instance != null)
        {
            SceneManagerEx.Instance.GameOver();
        }
    }
}
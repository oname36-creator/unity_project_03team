using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class PlayerControll : MonoBehaviour, PlayerAction.IPlayerActions
{
    [Header("Player Stat")]
    public int jumpCount = 0;
    public float JumpForce = 10f;
    public float MoveSpeed = 5f;

    [Header("Footstep Sound Settings (발소리 설정)")]
    [Tooltip("발소리가 재생될 간격(초)입니다. 짧을수록 빠르게 납니다.")]
    public float footstepInterval = 0.35f;
    private float footstepTimer = 0f;

    [Header("Aerial Damping (공중 감쇠)")]
    [Range(0f, 1f)]
    public float aerialDampingAmount = 0.5f;

    [Header("Player Attack (공격 설정)")]
    public Vector2 attackBoxSize = new Vector2(1.5f, 1f);
    public string enemyTag = "Monster";

    [Header("Weapon Settings (무기 설정)")]
    [Tooltip("총알이 발사될 위치(총구)에 배치한 빈 오브젝트를 넣어주세요.")]
    public UnityEngine.Transform muzzlePoint;

    [Header("Ground Check Settings (바닥 체크 설정)")]
    [Tooltip("플레이어 발밑에 배치한 빈 오브젝트를 넣어주세요.")]
    public UnityEngine.Transform groundCheckPoint;
    [Tooltip("바닥을 감지할 박스의 크기입니다.")]
    public Vector2 groundCheckSize = new Vector2(0.6f, 0.15f);
    [Tooltip("바닥으로 인식할 레이어(예: Ground)를 선택하세요.")]
    public LayerMask groundLayer;

    [Header("Player Attack Cooldown")]
    [Tooltip("공격 연속 사용 제한 시간(초)입니다.")]
    public float attackCooldown = 0.5f;
    private float lastAttackTime = -99f;

    [Header("Player Attack Trigger Settings")]
    public GameObject attackHitboxObj;
    public GameObject swordAttackHitboxObj;
    public float attackDuration = 0.5f;

    public PlayerPos playerPosData;

    private PlayerStatus status;
    private ItemEffectApplicator itemApplicator;
    private Rigidbody2D rb;

    private Animator animator;

    private PlayerAction controls;
    private Vector2 moveInput;

    private System.Collections.Generic.List<MovingPlatform> _activePlatforms = new System.Collections.Generic.List<MovingPlatform>();
    private UnityEngine.Transform _currentPlatformTransform = null;

    private float facingDirectionX = 1f;

    private float originalScaleX;
    private float originalScaleY;

    void Start()
    {
    }

    void Awake()
    {
        status = GetComponent<PlayerStatus>();
        itemApplicator = GetComponent<ItemEffectApplicator>();
        rb = GetComponent<Rigidbody2D>();

        animator = GetComponentInChildren<Animator>();

        originalScaleX = transform.localScale.x;
        originalScaleY = transform.localScale.y;
    }

    void OnEnable()
    {
        if (controls == null)
        {
            controls = new PlayerAction();
            controls.Player.SetCallbacks(this);
        }
        controls.Player.Enable();
    }

    void OnDisable()
    {
        controls.Player.Disable();
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;
        if (status == null || status.isDead) return;

        if (moveInput.x != 0f)
        {
            facingDirectionX = Mathf.Sign(moveInput.x);
            transform.localScale = new Vector3(facingDirectionX * originalScaleX, originalScaleY, 1f);
        }

        // ==================== [수정된 발소리 재생 로직] ====================
        // 땅에 서 있고(isGrounded), 좌우 입력(moveInput.x)이 있을 때만 타이머 가동
        if (status.isGrounded && Mathf.Abs(moveInput.x) > 0.1f)
        {
            footstepTimer += Time.deltaTime;

            if (footstepTimer >= footstepInterval)
            {
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX("_MoveSound");
                }
                footstepTimer = 0f; // 타이머 리셋
            }
        }
        else
        {
            // 멈추거나 공중에 떠 있을 때는 타이머를 간격 값으로 채워두어
            // 다시 움직이는 즉시 첫 발소리가 나도록 함
            footstepTimer = footstepInterval;
        }
        // ===================================================================

        if (animator != null)
        {
            float inputSpeed = Mathf.Abs(moveInput.x);
            float finalAnimSpeed = inputSpeed;

            // 이동 발판의 순수 x 속도 구하기
            float platformXVelocity = 0f;
            if (transform.parent != null && transform.parent.TryGetComponent<MovingPlatform>(out var platform))
            {
                platformXVelocity = platform.Velocity.x;
            }

            // 플레이어의 실제 속도에서 발판의 속도를 뺀 '순수 플레이어 속도' 계산
            float purePlayerVelocityX = rb.linearVelocity.x - platformXVelocity;

            // 입력이 없는데도 플레이어 순수 속도가 남아있을 때만 달리기 모션 유지
            if (inputSpeed == 0f && Mathf.Abs(purePlayerVelocityX) > 1.5f)
            {
                finalAnimSpeed = 1f;
            }
            else if (inputSpeed == 0f)
            {
                // 입력이 없고 발판 위에 가만히 있다면 확실하게 아이들(0)로 고정
                finalAnimSpeed = 0f;
            }

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            bool attacking = stateInfo.IsTag("Attacking");

            animator.SetBool("isAttacking", attacking);
            animator.SetBool("isGrounded", status.isGrounded);
            animator.SetFloat("VelocityY", rb.linearVelocity.y);
            animator.SetFloat("Speed", finalAnimSpeed);
        }

        if (playerPosData != null)
        {
            playerPosData.x = Mathf.RoundToInt(transform.position.x);
            playerPosData.y = Mathf.RoundToInt(transform.position.y);
        }
    }

    void FixedUpdate()
    {
        if (status == null || status.isDead) return;

        if (groundCheckPoint != null)
        {
            status.isGrounded = Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0f, groundLayer);
        }

        if (status.isGrounded)
        {
            jumpCount = 0;
        }

        status.isAerial = !status.isGrounded;

        if (status.isHurt) return;

        float currentMoveX = rb.linearVelocity.x;
        float currentVelocityY = rb.linearVelocity.y;

        if (status.isGrounded)
        {
            currentMoveX = moveInput.x * MoveSpeed;
        }
        else
        {
            if (moveInput.x != 0f)
            {
                if (currentMoveX != 0f && Mathf.Sign(currentMoveX) != Mathf.Sign(moveInput.x))
                {
                    currentMoveX += moveInput.x * MoveSpeed * (1f - aerialDampingAmount) * Time.fixedDeltaTime * 20f;
                    currentMoveX = Mathf.Clamp(currentMoveX, -MoveSpeed, MoveSpeed);
                }
                else
                {
                    currentMoveX = moveInput.x * MoveSpeed;
                }
            }
            else
            {
                currentMoveX = Mathf.MoveTowards(currentMoveX, 0f, MoveSpeed * Time.fixedDeltaTime * 8f);
            }
        }

        if (!status.isGrounded)
        {
            if (currentVelocityY > 0f)
            {
                currentVelocityY += Physics2D.gravity.y * 1.5f * Time.fixedDeltaTime;
            }
            else if (currentVelocityY < 0f)
            {
                currentVelocityY += Physics2D.gravity.y * 4.0f * Time.fixedDeltaTime;
            }
        }
        currentMoveX *= status.speedMultiplier;

        float platformXVelocity = 0f;

        if (transform.parent != null)
        {
            if (transform.parent.TryGetComponent<MovingPlatform>(out var platform))
            {
                platformXVelocity = platform.Velocity.x;
            }
        }

        rb.linearVelocity = new Vector2(currentMoveX + platformXVelocity, currentVelocityY);
    }

    public void SetActivePlatform(MovingPlatform platform)
    {
        if (platform != null)
        {
            _currentPlatformTransform = platform.transform;
            transform.SetParent(_currentPlatformTransform);
        }
    }

    public void ClearActivePlatform(MovingPlatform platform)
    {
        if (platform != null && _currentPlatformTransform == platform.transform)
        {
            if (transform.parent == _currentPlatformTransform)
            {
                transform.SetParent(null);
            }
            _currentPlatformTransform = null;
        }
    }

    #region 인트로용 플레이어 조작

    public void SetInputActive(bool active)
    {
        if (active)
        {
            controls?.Player.Enable();
        }
        else
        {
            controls?.Player.Disable();
            moveInput = Vector2.zero;
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
        }
    }
    #endregion

    public void OnMove(InputAction.CallbackContext context)
    {
        
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (Time.timeScale == 0f) return;
        if (status == null || status.isDead) return;

        if (context.started && jumpCount < 1 && status.isGrounded)
        {
            rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
            jumpCount++;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundCheckPoint.position, groundCheckSize);
        }

        if (muzzlePoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(muzzlePoint.position, 0.1f);
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (Time.timeScale == 0f) return;

        if (context.started)
        {
            if (status == null || status.isDead) return;

            if (Time.time < lastAttackTime + attackCooldown)
            {
                //Debug.Log($"공격 쿨타임 중입니다! 남은 시간: {(lastAttackTime + attackCooldown) - Time.time:F2}초");
                return;
            }

            lastAttackTime = Time.time;

            if (status.hasGun)
            {
                if (animator != null)
                {
                    SoundManager.Instance.PlaySFX("_GunSound");
                    animator.SetTrigger("OnGunAttack");
                }

                StartCoroutine(GunAttackRoutine());
                return;
            }

            if (status.hasSword)
            {
                if (animator != null)
                {
                    animator.SetTrigger("OnSwordAttack");
                }
                //Debug.Log("검 공격 발동! (검 히트박스 활성화)");
                StartCoroutine(SwordAttackHitboxRoutine());
                status.OnSwordAttackExecute();
                return;
            }

            if (animator != null)
            {
                animator.SetTrigger("OnAttack");
            }

            //Debug.Log("맨손 공격 발동! (기본 히트박스 활성화)");
            StartCoroutine(AttackHitboxRoutine());
            status.OnAttackExecute();
        }
    }

    public void OnItemUse(InputAction.CallbackContext context)
    {
        if (Time.timeScale == 0f) return;

        if (context.started)
        {
            string pressedKey = context.control.name;

            switch (pressedKey)
            {
                case "1": DataManager.Instance.UseItemSlot(0); break;
                case "2": DataManager.Instance.UseItemSlot(1); break;
                case "3": DataManager.Instance.UseItemSlot(2); break;
                case "4": DataManager.Instance.UseItemSlot(3); break;
                default:
                    //Debug.Log($"지정되지 않은 키 입력: {pressedKey}");
                    break;
            }
        }
    }

    public void ExecuteItemEffectByID(int itemNumber)
    {
        switch (itemNumber)
        {
            case 1:
                if (status != null)
                {
                    status.ChangeHp(50f);
                    //Debug.Log($"빨간포션 사용! 현재 체력: {status.currentHp}");
                }
                break;

            case 2:
                if (itemApplicator != null)
                {
                    itemApplicator.ExecuteItemEffectByID(itemNumber);
                }
                break;

            default:
                //Debug.LogWarning($"아직 효과가 정의되지 않은 아이템 번호입니다: {itemNumber}");
                break;
        }
    }

    public void UseItem(ItemData data)
    {
        if (itemApplicator != null)
        {
            itemApplicator.ApplyItemEffect(data);
        }
    }

    public void RequestItemEffectByID(int itemNumber)
    {
        if (itemApplicator != null)
        {
            itemApplicator.ExecuteItemEffectByID(itemNumber);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (status == null || status.isDead) return;

        if (collision.CompareTag("DeadZone") || collision.gameObject.layer == LayerMask.NameToLayer("Boss") || collision.CompareTag("Boss"))
        {
            if (DataManager.Instance != null)
            {
                DataManager.Instance.PlayerHp = 0;
                if (animator != null)
                {
                    animator.SetTrigger("OnDead");
                }
            }
            status.ChangeHp(-status.currentHp);

            if (transform.parent != null) transform.SetParent(null);
            return;
        }

        if (status.isInvincible) return;
        bool isCurrentlyAttacking = (attackHitboxObj != null && attackHitboxObj.activeSelf) ||
                                    (swordAttackHitboxObj != null && swordAttackHitboxObj.activeSelf);
        if (isCurrentlyAttacking) return;

        if (collision.gameObject.layer == LayerMask.NameToLayer("Monster") && collision.CompareTag(enemyTag))
        {
            status.ChangeHp(-10f);
            if (DataManager.Instance != null) DataManager.Instance.PlayerHp = (int)status.currentHp;

            //Debug.Log($" [진짜 피격] 플레이어 몸통이 피격당함. 현재 HP: {status.currentHp}");

            if (!status.isDead)
            {
                StartCoroutine(KnockbackAndInvincibleRoutine(collision.transform.position));
            }
        }
    }

    private System.Collections.IEnumerator KnockbackAndInvincibleRoutine(Vector3 enemyPosition)
    {
        if (animator != null)
        {
            animator.SetTrigger("OnHit");
        }

        status.isHurt = true;
        status.isInvincible = true;

        if (transform.parent != null)
        {
            transform.SetParent(null);
        }
        _currentPlatformTransform = null;

        Collider2D myCollider = GetComponent<Collider2D>();
        if (myCollider != null)
        {
            myCollider.enabled = false;
        }

        if (rb != null)
        {
            float knockbackDirection = transform.position.x > enemyPosition.x ? 1f : -1f;
            rb.linearVelocity = Vector2.zero;

            rb.AddForce(new Vector2(knockbackDirection * JumpForce * 0.2f, JumpForce * 0.1f), ForceMode2D.Impulse);
        }

        yield return new WaitForFixedUpdate();

        if (myCollider != null)
        {
            myCollider.enabled = true;
        }

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        float invincibleTime = 1.0f;
        float blinkInterval = 0.1f;
        float timer = 0f;
        float knockbackDuration = 0.2f;

        while (timer < invincibleTime)
        {
            if (sr != null)
            {
                float currentAlpha = sr.color.a;
                float nextAlpha = (currentAlpha == 1f) ? 0.2f : 1f;
                sr.color = new Color(1f, 1f, 1f, nextAlpha);
            }

            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;

            if (status.isHurt && timer >= knockbackDuration)
            {
                status.isHurt = false;
            }
        }

        if (sr != null)
        {
            sr.color = Color.white;
        }

        status.isInvincible = false;
        //Debug.Log("무적 상태 종료! 완벽하게 안전화 완료.");
    }

    private System.Collections.IEnumerator AttackHitboxRoutine()
    {
        Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("Player"),
            LayerMask.NameToLayer("Monster"),
            true
        );

        if (attackHitboxObj != null) attackHitboxObj.SetActive(true);

        yield return new WaitForSeconds(0.3f);

        if (attackHitboxObj != null) attackHitboxObj.SetActive(false);

        Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("Player"),
            LayerMask.NameToLayer("Monster"),
            false
        );
    }

    private System.Collections.IEnumerator SwordAttackHitboxRoutine()
    {
        Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("Player"),
            LayerMask.NameToLayer("Monster"),
            true
        );

        if (swordAttackHitboxObj != null) swordAttackHitboxObj.SetActive(true);

        yield return new WaitForSeconds(0.3f);

        if (swordAttackHitboxObj != null) swordAttackHitboxObj.SetActive(false);

        Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("Player"),
            LayerMask.NameToLayer("Monster"),
            false
        );
    }

    private System.Collections.IEnumerator GunAttackRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        if (status == null || status.isDead || Time.timeScale == 0f) yield break;

        //Debug.Log("총기 발사!");

        Vector2 firePosition = (muzzlePoint != null) ? (Vector2)muzzlePoint.position : (Vector2)transform.position + new Vector2(facingDirectionX * 0.5f, 0f);
        GameObject bulletGo = ObjectPoolManager.Instance.GetBullet(firePosition, Quaternion.identity);

        if (bulletGo != null)
        {
            Bullet bulletScript = bulletGo.GetComponent<Bullet>();
            if (bulletScript != null) bulletScript.Launch(facingDirectionX);
        }

        status.OnGunAttackExecute();
    }
}
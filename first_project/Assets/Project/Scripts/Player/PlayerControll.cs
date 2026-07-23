using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    public string enemyTag = "Monster";

    [Header("Weapon Settings (무기 설정)")]
    [Tooltip("총알이 발사될 위치(총구)에 배치한 빈 오브젝트를 넣어주세요.")]
    public Transform muzzlePoint;

    [Header("Ground Check Settings (바닥 체크 설정)")]
    [Tooltip("플레이어 발밑에 배치한 빈 오브젝트를 넣어주세요.")]
    public Transform groundCheckPoint;
    [Tooltip("바닥을 감지할 박스의 크기입니다.")]
    public Vector2 groundCheckSize = new Vector2(0.4f, 0.15f);
    [Tooltip("바닥으로 인식할 레이어(예: Ground)를 선택하세요.")]
    public LayerMask groundLayer;

    [Header("Player Attack Cooldown")]
    [Tooltip("공격 연속 사용 제한 시간(초)입니다.")]
    public float attackCooldown = 0.5f;
    private float lastAttackTime = -99f;

    [Header("Player Attack Trigger Settings")]
    public GameObject attackHitboxObj;
    public GameObject swordAttackHitboxObj;

    public PlayerPos playerPosData;

    private PlayerStatus status;
    private ItemEffectApplicator itemApplicator;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private PlayerAction controls;
    private Vector2 moveInput;

    private Transform _currentPlatformTransform = null;
    private float facingDirectionX = 1f;

    private float originalScaleX;
    private float originalScaleY;
    private readonly Collider2D[] groundCheckResults = new Collider2D[1];

    void Awake()
    {
        status = GetComponent<PlayerStatus>();
        itemApplicator = GetComponent<ItemEffectApplicator>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        originalScaleX = transform.localScale.x;
        originalScaleY = transform.localScale.y;

        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.RegisterPlayer(gameObject);
        }
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
        if (controls != null)
        {
            controls.Player.Disable();
        }
        ResetSpriteColor();
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;
        if (status == null || status.isDead) return;

        // 방향 전환 (Flip)
        if (moveInput.x != 0f)
        {
            facingDirectionX = Mathf.Sign(moveInput.x);
            transform.localScale = new Vector3(facingDirectionX * originalScaleX, originalScaleY, 1f);
        }

        // ==================== [발소리 재생 로직] ====================
        if (status.isGrounded && Mathf.Abs(moveInput.x) > 0.1f)
        {
            footstepTimer += Time.deltaTime;

            if (footstepTimer >= footstepInterval)
            {
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX("_MoveSound");
                }
                footstepTimer = 0f;
            }
        }
        else
        {
            footstepTimer = footstepInterval;
        }

        // ==================== [애니메이션 처리] ====================
        if (animator != null)
        {
            float inputSpeed = Mathf.Abs(moveInput.x);
            float finalAnimSpeed = inputSpeed;

            float platformXVelocity = 0f;
            if (transform.parent != null && transform.parent.TryGetComponent<MovingPlatform>(out var platform))
            {
                platformXVelocity = platform.Velocity.x;
            }

            float purePlayerVelocityX = rb.linearVelocity.x - platformXVelocity;

            if (inputSpeed == 0f && Mathf.Abs(purePlayerVelocityX) > 1.5f)
            {
                finalAnimSpeed = 1f;
            }
            else if (inputSpeed == 0f)
            {
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

        // 1. 접지 체크
        if (groundCheckPoint != null)
        {
            int count = Physics2D.OverlapBoxNonAlloc(groundCheckPoint.position, groundCheckSize, 0f, groundCheckResults, groundLayer);
            bool previouslyGrounded = status.isGrounded;
            status.isGrounded = count > 0;

            // 착지하는 순간 점프 카운트 리셋
            if (status.isGrounded && rb.linearVelocity.y <= 0.01f)
            {
                jumpCount = 0;
            }
        }

        status.isAerial = !status.isGrounded;

        if (status.isHurt) return;

        // 2. 이동 속도 계산
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

        // 중력 보정 (가속)
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
        float platformYVelocity = 0f;

        if (transform.parent != null && transform.parent.TryGetComponent<MovingPlatform>(out var platform))
        {
            platformXVelocity = platform.Velocity.x;
            platformYVelocity = platform.Velocity.y;
        }

        if (status.isGrounded && transform.parent != null)
        {
            // 키를 입력할 때 플레이어의 기본 이동 속도에 '발판의 X 속도'를 더해줌으로써 상대 속도 문제 해결
            if (Mathf.Abs(moveInput.x) > 0.01f)
            {
                currentMoveX = (moveInput.x * MoveSpeed * status.speedMultiplier) + platformXVelocity;
            }
            else
            {
                // 가만히 있을 때는 발판의 X 속도를 그대로 받아와서 미끄러짐 방지
                currentMoveX = platformXVelocity;
            }
        }
        else
        {
            // 공중이거나 발판에 없을 때 발판 속도가 남아있는 것 방지
            currentMoveX += platformXVelocity;
        }

        float finalVelocityY = currentVelocityY;
        if (status.isGrounded && transform.parent != null && currentVelocityY <= 0.1f)
        {
            // Y축 역시 발판의 상승 속도를 자연스럽게 타도록 보정
            finalVelocityY = platformYVelocity;
        }

        rb.linearVelocity = new Vector2(currentMoveX, finalVelocityY);
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

    #region 인트로 및 외부 통제
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

    #region InputSystem Callbacks
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

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (Time.timeScale == 0f) return;

        if (context.started)
        {
            if (status == null || status.isDead) return;

            if (Time.time < lastAttackTime + attackCooldown) return;

            lastAttackTime = Time.time;

            if (status.hasGun)
            {
                if (animator != null)
                {
                    if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("_GunSound");
                    animator.SetTrigger("OnGunAttack");
                }
                StartCoroutine(GunAttackRoutine());
                return;
            }

            if (status.hasSword)
            {
                if (animator != null)
                {
                    if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("_SwordSound");
                    animator.SetTrigger("OnSwordAttack");
                }
                StartCoroutine(SwordAttackHitboxRoutine());
                status.OnSwordAttackExecute();
                return;
            }

            if (animator != null)
            {
                animator.SetTrigger("OnAttack");
            }

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
                case "1": DataManager.Instance?.UseItemSlot(0); break;
                case "2": DataManager.Instance?.UseItemSlot(1); break;
                case "3": DataManager.Instance?.UseItemSlot(2); break;
                case "4": DataManager.Instance?.UseItemSlot(3); break;
            }
        }
    }
    #endregion

    #region Item Handlers
    public void ExecuteItemEffectByID(int itemNumber)
    {
        switch (itemNumber)
        {
            case 1:
                if (status != null) status.ChangeHp(50f);
                break;
            case 2:
                if (itemApplicator != null) itemApplicator.ExecuteItemEffectByID(itemNumber);
                break;
        }
    }

    public void UseItem(ItemData data)
    {
        if (itemApplicator != null) itemApplicator.ApplyItemEffect(data);
    }

    public void RequestItemEffectByID(int itemNumber)
    {
        if (itemApplicator != null) itemApplicator.ExecuteItemEffectByID(itemNumber);
    }
    #endregion

    #region Collision & Hit
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (status == null || status.isDead) return;

        if (collision.CompareTag("DeadZone") || collision.gameObject.layer == LayerMask.NameToLayer("Boss") || collision.CompareTag("Boss"))
        {
            if (DataManager.Instance != null)
            {
                DataManager.Instance.PlayerHp = 0;
            }
            if (animator != null)
            {
                animator.SetTrigger("OnDead");
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

            if (!status.isDead)
            {
                StartCoroutine(KnockbackAndInvincibleRoutine(collision.transform.position));
            }
        }
    }

    private IEnumerator KnockbackAndInvincibleRoutine(Vector3 enemyPosition)
    {
        if (animator != null)
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("_HitSound");
            animator.SetTrigger("OnHit");
        }

        status.isHurt = true;
        status.isInvincible = true;

        if (transform.parent != null) transform.SetParent(null);
        _currentPlatformTransform = null;

        if (rb != null)
        {
            float knockbackDirection = transform.position.x > enemyPosition.x ? 1f : -1f;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(new Vector2(knockbackDirection * JumpForce * 0.1f, JumpForce * 0.05f), ForceMode2D.Impulse);
        }

        float invincibleTime = 1.0f;
        float blinkInterval = 0.1f;
        float timer = 0f;
        float knockbackDuration = 0.2f;

        while (timer < invincibleTime)
        {
            if (spriteRenderer != null)
            {
                float currentAlpha = spriteRenderer.color.a;
                float nextAlpha = (currentAlpha >= 0.9f) ? 0.2f : 1f;
                spriteRenderer.color = new Color(1f, 1f, 1f, nextAlpha);
            }

            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;

            if (status.isHurt && timer >= knockbackDuration)
            {
                status.isHurt = false;
            }
        }

        ResetSpriteColor();
        status.isHurt = false;
        status.isInvincible = false;
    }

    private void ResetSpriteColor()
    {
        if (spriteRenderer != null)
        {
            Color col = spriteRenderer.color;
            col.a = 1f;
            spriteRenderer.color = col;
        }
    }
    #endregion

    #region Coroutines (Attacks)
    private IEnumerator AttackHitboxRoutine()
    {
        if (attackHitboxObj != null) attackHitboxObj.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        if (attackHitboxObj != null) attackHitboxObj.SetActive(false);
    }

    private IEnumerator SwordAttackHitboxRoutine()
    {
        if (swordAttackHitboxObj != null) swordAttackHitboxObj.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        if (swordAttackHitboxObj != null) swordAttackHitboxObj.SetActive(false);
    }

    private IEnumerator GunAttackRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        if (status == null || status.isDead || Time.timeScale == 0f) yield break;

        Vector2 firePosition = (muzzlePoint != null) ? (Vector2)muzzlePoint.position : (Vector2)transform.position + new Vector2(facingDirectionX * 0.5f, 0f);
        GameObject bulletGo = ObjectPoolManager.Instance?.GetBullet(firePosition, Quaternion.identity);

        if (bulletGo != null)
        {
            Bullet bulletScript = bulletGo.GetComponent<Bullet>();
            if (bulletScript != null) bulletScript.Launch(facingDirectionX);
        }

        status.OnGunAttackExecute();
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
    #endregion
}
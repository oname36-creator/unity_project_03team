using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControll : MonoBehaviour, PlayerAction.IPlayerActions
{
    [Header("Player Stat")]
    public int jumpCount = 0;
    public float JumpForce = 10f;
    public float MoveSpeed = 5f;

    [Header("Aerial Damping (공중 감쇠)")]
    [Range(0f, 1f)]
    public float aerialDampingAmount = 0.5f;

    [Header("Player Attack (공격 설정)")]
    public Vector2 attackBoxSize = new Vector2(1.5f, 1f);
    public string enemyTag = "Monster";

    [Header("Ground Check Settings (바닥 체크 설정)")]
    [Tooltip("플레이어 발밑에 배치한 빈 오브젝트를 넣어주세요.")]
    public Transform groundCheckPoint;
    [Tooltip("바닥을 감지할 박스의 크기입니다.")]
    public Vector2 groundCheckSize = new Vector2(0.6f, 0.1f);
    [Tooltip("바닥으로 인식할 레이어(예: Ground)를 선택하세요.")]
    public LayerMask groundLayer;

    [Header("Player Attack Trigger Settings")]
    public GameObject attackHitboxObj; 
    public float attackDuration = 0.5f;

    public PlayerPos playerPosData;

    private PlayerStatus status;
    private ItemEffectApplicator itemApplicator;
    private Rigidbody2D rb;

    private PlayerAction controls;
    private Vector2 moveInput;

    private float facingDirectionX = 1f;

    void Awake()
    {
        status = GetComponent<PlayerStatus>();
        itemApplicator = GetComponent<ItemEffectApplicator>();
        rb = GetComponent<Rigidbody2D>();
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
        // 게임이 일시정지(정지화면) 상태면 입력을 처리하지 않고 리턴
        if (Time.timeScale == 0f) return;

        if (status == null || status.isDead) return;

        // [핵심 변경] 실시간으로 발밑에 groundLayer를 가진 콜라이더가 있는지 체크합니다.
        if (groundCheckPoint != null)
        {
            status.isGrounded = Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0f, groundLayer);
        }

        // 바닥에 닿아있다면 점프 카운트를 리셋합니다.
        if (status.isGrounded)
        {
            jumpCount = 0;
        }

        status.isAerial = !status.isGrounded;

        if (moveInput.x != 0f)
        {
            facingDirectionX = Mathf.Sign(moveInput.x);
            // transform.localScale = new Vector3(facingDirectionX, 1f, 1f);
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

        
        if (status.isHurt) return;

        float currentMoveX = rb.linearVelocity.x;
        float currentVelocityY = rb.linearVelocity.y;

        // 1. 좌우 이동 제어
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

        // 2. 스피디한 중력 제어
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

        rb.linearVelocity = new Vector2(currentMoveX, currentVelocityY);
    }

    // -----------------------------------------------------------------
    // Input Action 콜백 메서드
    // -----------------------------------------------------------------

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (status == null || status.isDead) return;
        // 이제 정확한 isGrounded 판정 덕분에 점프가 씹히지 않습니다.
        if (context.started && jumpCount < 1 && status.isGrounded)
        {
            rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
            jumpCount++;
            
        }
    }

    // [팁] 에디터 뷰에서 바닥 체크 상자의 크기와 위치를 빨간 선으로 시각화해 줍니다.
    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundCheckPoint.position, groundCheckSize);
        }
    }

    // 기존의 OnCollision 계열 메서드들은 중복 판정 및 꼬임 방지를 위해 모두 삭제했습니다.

   
       public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // 1단계 테스트: J키 입력 자체가 들어오는가?
            Debug.Log($"[공격 테스트] J키 누름 판정 들어옴! 현재 status 상태: {(status != null ? "존재함" : "Null!!")}");

            if (status == null || status.isDead) return;

            // 2단계 테스트: 총을 가졌다고 판정되는가?
            Debug.Log($"[공격 테스트] 현재 플레이어의 hasGun 상태: {status.hasGun}");

            if (status.hasGun)
            {
                Debug.Log("총기 발사!");
                Vector2 firePosition = (Vector2)transform.position + new Vector2(facingDirectionX * 0.5f, 0f);

                // 1. 풀에서 총알을 가져옵니다.
                GameObject bulletGo = ObjectPoolManager.Instance.GetBullet(firePosition, Quaternion.identity);

                // 2. 총알의 Bullet 스크립트 컴포넌트를 가져와 Launch를 호출합니다. ★★★
                if (bulletGo != null)
                {
                    Bullet bulletScript = bulletGo.GetComponent<Bullet>();
                    if (bulletScript != null)
                    {
                        bulletScript.Launch(facingDirectionX); // 플레이어가 보는 방향을 전달!
                    }
                }

                status.OnGunAttackExecute();
                return; // 총을 쐈으므로 아래 근접 공격 코드는 실행하지 않고 리턴
            }

            Debug.Log("근접 공격 발동! (히트박스 활성화)");

            // 공격 히트박스를 잠깐 켰다 끄는 코루틴을 시작합니다.
            StartCoroutine(AttackHitboxRoutine());

            status.OnAttackExecute();
        }
    }

    public void OnItemUse(InputAction.CallbackContext context)
    {
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
                    Debug.Log($"지정되지 않은 키 입력: {pressedKey}");
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
                    Debug.Log($"빨간포션 사용! 현재 체력: {status.currentHp}");
                }
                break;

            case 2:
                if (itemApplicator != null)
                {
                    itemApplicator.ExecuteItemEffectByID(itemNumber);
                }
                break;

            default:
                Debug.LogWarning($"아직 효과가 정의되지 않은 아이템 번호입니다: {itemNumber}");
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

        // 1. 이미 무적 상태라면 충돌을 무시합니다.
        if (status.isInvincible) return;

        if (collision.CompareTag(enemyTag))
        {
            // 2. PlayerStatus에 만들어두신 ChangeHp 함수로 안전하게 체력을 깎습니다.
            status.ChangeHp(-10f);

            // 데이터 매니저의 HP도 동기화해 줍니다 (UI 반영용)
            if (DataManager.Instance != null)
            {
                DataManager.Instance.PlayerHp = (int)status.currentHp;
            }

            Debug.Log($"[피격] 몬스터 충돌! 현재 HP: {status.currentHp}");

            // 사망하지 않았다면 넉백 및 무적 루틴 시작
            if (!status.isDead)
            {
                StartCoroutine(KnockbackAndInvincibleRoutine(collision.transform.position));
            }
        }
    }

    // 넉백과 무적 처리를 한 번에 관리하는 코루틴
    private System.Collections.IEnumerator KnockbackAndInvincibleRoutine(Vector3 enemyPosition)
    {
        // 1. 넉백 시작 (조작 불가 상태 돌입) 및 무적 상태 설정
        status.isHurt = true;
        status.isInvincible = true;

        if (rb != null)
        {
            float knockbackDirection = transform.position.x > enemyPosition.x ? 1f : -1f;
            rb.linearVelocity = Vector2.zero;
            // 아까 조절한 수치 (가로 0.3f, 세로 0.2f 예시)
            rb.AddForce(new Vector2(knockbackDirection * JumpForce * 0.3f, JumpForce * 0.2f), ForceMode2D.Impulse);
        }

        // 💡 SpriteRenderer 컴포넌트를 가져옵니다.
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        // [깜빡거림 연출 시작]
        // 무적 상태인 1초 동안 아주 빠르게 깜빡거리게 합니다.
        float invincibleTime = 1.0f; // 총 무적 시간
        float blinkInterval = 0.1f; // 깜빡거리는 간격 (0.1초마다)
        float timer = 0f;

        // 넉백 지속 시간 (0.2초)
        float knockbackDuration = 0.2f;

        // 무적 시간이 다 될 때까지 반복
        while (timer < invincibleTime)
        {
            if (sr != null)
            {
                // 현재 알파값(투명도)을 가져와서 반전시킵니다. (1 -> 0.2 -> 1 -> 0.2...)
                float currentAlpha = sr.color.a;
                float nextAlpha = (currentAlpha == 1f) ? 0.2f : 1f; // 0.2f는 거의 투명

                sr.color = new Color(1f, 1f, 1f, nextAlpha);
            }

            // 0.1초 대기
            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;

            // 💡 넉백 시간이 지나면 조작 가능 상태로 돌려줍니다.
            if (status.isHurt && timer >= knockbackDuration)
            {
                status.isHurt = false;
            }
        }

        // [깜빡거림 연출 종료 및 원상복구]
        if (sr != null)
        {
            sr.color = Color.white; // 색상 및 투명도 원상복구
        }

        // 최종 무적 해제
        status.isInvincible = false;

        Debug.Log("무적 상태 및 깜빡거림 종료!");
    }
    private System.Collections.IEnumerator AttackHitboxRoutine()
    {
        if (attackHitboxObj != null)
        {
            // 1. 공격 시작할 때 히트박스를 켭니다.
            attackHitboxObj.SetActive(true);

            // 2. 설정한 시간(예: 0.5초) 동안 플레이어가 공격 자세를 취하듯 대기합니다.
            yield return new WaitForSeconds(attackDuration);

            // 3. 시간이 지나면 히트박스를 다시 끕니다.
            attackHitboxObj.SetActive(false);
        }
        else
        {
            Debug.LogError("PlayerControll에 AttackHitboxObj가 연결되지 않았습니다!");
        }
    }
}
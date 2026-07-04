using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControll : MonoBehaviour, PlayerAction.IPlayerActions
{
    [Header("Player Stat")]
    public int jumpCount = 0;
    public float JumpForce = 7.5f;
    public float MoveSpeed = 5f;

    [Header("Aerial Damping (공중 감쇠)")]
    [Range(0f, 1f)]
    public float aerialDampingAmount = 0.5f;

    [Header("Player Attack (공격 설정)")]
    public float attackDamage = 10f;
    public float attackRange = 1.2f;     
    public Vector2 attackBoxSize = new Vector2(1.5f, 1f);
    public string enemyTag = "Monster";

    public PlayerPos playerPosData;

    // 컴포넌트 참조 (상태 관리와 아이템 적용 분리)
    private PlayerStatus status;
    private ItemEffectApplicator itemApplicator;
    private Rigidbody2D rb;

    private PlayerAction controls;
    private Vector2 moveInput;
    private float aerialDirectionX = 0f;

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
        if (status == null || status.isDead) return;

        status.isAerial = !status.isGrounded;

        if (moveInput.x != 0f)
        {
            facingDirectionX = Mathf.Sign(moveInput.x);

            // 시각적으로 캐릭터 이미지를 뒤집고 싶다면 아래 주석을 해제하세요.
            // transform.localScale = new Vector3(facingDirectionX, 1f, 1f);
        }
        // 데이터 저장
        if (playerPosData != null)
        {
            playerPosData.x = Mathf.RoundToInt(transform.position.x);
            playerPosData.y = Mathf.RoundToInt(transform.position.y);
        }
    }

    void FixedUpdate()
    {
        if (status == null || status.isDead) return;

        float currentMoveX = 0f;
        float currentVelocityY = rb.linearVelocity.y;

        if (status.isGrounded)
        {
            currentMoveX = moveInput.x * MoveSpeed;
        }
        else
        {
            if (moveInput.x != 0f)
            {
                currentMoveX = moveInput.x * MoveSpeed;

                if (aerialDirectionX != 0f && Mathf.Sign(aerialDirectionX) != Mathf.Sign(moveInput.x))
                {
                    currentMoveX *= (1f - aerialDampingAmount);
                }
            }
            else
            {
                currentMoveX = aerialDirectionX * MoveSpeed;
            }

            if (currentVelocityY <= 0.1f)
            {
                currentVelocityY += Physics2D.gravity.y * Time.fixedDeltaTime * 2f;
            }
        }

        rb.linearVelocity = new Vector2(currentMoveX, currentVelocityY);
    }

    // [유지] 입력 판단 기준을 0.01f로 낮춘 최적의 버전 하나만 남겨둡니다.
    private void SetAerialDirection()
    {
        aerialDirectionX = moveInput.x > 0.01f ? 1f : (moveInput.x < -0.01f ? -1f : 0f);
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
        if (context.started && jumpCount < 1 && status.isGrounded)
        {
            rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
            jumpCount++;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckGrounded(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        CheckGrounded(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (status.isGrounded)
        {
            // [교정] 바닥에서 벗어나는 순간 점프 관성 방향을 안전하게 저장해 줍니다.
            SetAerialDirection();
            status.isGrounded = false;
        }
    }

    private void CheckGrounded(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.7f)
            {
                status.isGrounded = true;
                jumpCount = 0;
                aerialDirectionX = 0f;
                return;
            }
        }
    }

    // ★ 하단에 있던 중복된 SetAerialDirection() 메서드는 깨끗하게 지웠습니다.

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (status == null || status.isDead) return;

            Debug.Log("공격 발동!");

            
            Vector2 attackPosition = (Vector2)transform.position + new Vector2(facingDirectionX * attackRange, 0f);

            
            Collider2D[] hitColliders = Physics2D.OverlapBoxAll(attackPosition, attackBoxSize, 0f);

            foreach (Collider2D col in hitColliders)
            {
                if (col.CompareTag(enemyTag))
                {
                    Debug.Log($"몬스터 적중: {col.name}");

                    // [연동 예시] 몬스터에게 데미지 스크립트가 있다면 호출
                    // MonsterStatus monster = col.GetComponent<MonsterStatus>();
                    // if (monster != null) { monster.TakeDamage(attackDamage); }
                }
            }
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
}

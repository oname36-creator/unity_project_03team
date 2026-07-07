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

            Debug.Log("근접 공격 발동!");
            float currentRange = status.currentAttackRange;
            Vector2 attackPosition = (Vector2)transform.position + new Vector2(facingDirectionX * currentRange, 0f);

            Collider2D[] hitColliders = Physics2D.OverlapBoxAll(attackPosition, attackBoxSize, 0f);
            foreach (Collider2D col in hitColliders)
            {
                if (col.CompareTag(enemyTag))
                {
                    Debug.Log($"몬스터 적중: {col.name}");
                }
            }

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
}
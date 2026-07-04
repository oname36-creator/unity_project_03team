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
    public Vector2 attackBoxSize = new Vector2(1.5f, 1f);
    public string enemyTag = "Monster";

    public PlayerPos playerPosData;

    // 컴포넌트 참조 (상태 관리와 아이템 적용 분리)
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

        float currentMoveX = rb.linearVelocity.x;
        float currentVelocityY = rb.linearVelocity.y;

        // 1. 좌우 이동 제어 (기존 유지)
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

        // 2. [수정] 스피디한 중력 제어 (지루한 둥실거림 완벽 해결)
        if (!status.isGrounded)
        {
            if (currentVelocityY > 0f)
            {
                // [상승 중] 상승할 때도 중력을 살짝 더 주어(1.5배) 붕 뜨는 느낌 없이 팍 치고 올라가게 합니다.
                currentVelocityY += Physics2D.gravity.y * 1.5f * Time.fixedDeltaTime;
            }
            else if (currentVelocityY < 0f)
            {
                // [하강 중] 최고점을 찍고 떨어질 때는 중력을 훨씬 강하게(4.0배) 주어 자석처럼 빠르게 착지시킵니다.
                currentVelocityY += Physics2D.gravity.y * 4.0f * Time.fixedDeltaTime;
            }
        }

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

        // [핵심] 오직 스페이스바를 누른 '순간'(started)에만 힘을 줍니다.
        // 키를 언제 떼든(canceled) 속도를 건드리는 로직을 완전히 제거했습니다.
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
        // 벼랑 끝에서 떨어질 때 IsGrounded를 해제하는 깔끔한 예외 처리
        // 이전의 SetAerialDirection() 호출부를 제거하여 관성이 꼬이는 문제를 방지합니다.
        if (status != null && status.isGrounded)
        {
            status.isGrounded = false;
        }
    }

    private void CheckGrounded(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // 경사면 판정 완화 (0.7f -> 0.6f) : 경사로에서 점프가 씹히는 현상 방지
            if (contact.normal.y > 0.6f)
            {
                status.isGrounded = true;
                jumpCount = 0;
                return;
            }
        }
    }



    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (status == null || status.isDead) return;

            // -------------------------------------------------------------
            // 원거리 공격 (총을 들고 있을 때)
            // -------------------------------------------------------------
            if (status.hasGun)
            {
                Debug.Log("총기 발사!");

                // 플레이어 위치에서 약간 앞쪽 발사 지점 설정 (원한다면 FirePoint 오브젝트를 따로 선언해도 좋음)
                Vector2 firePosition = (Vector2)transform.position + new Vector2(facingDirectionX * 0.5f, 0f);

                // 풀 매니저에서 총알 땡겨오기
                GameObject bullet = ObjectPoolManager.Instance.GetBullet(firePosition, Quaternion.identity);

                // [참고] 총알 스크립트에 방향을 넘겨주는 컴포넌트가 있다면 아래처럼 호출합니다.
                // Bullet bulletScript = bullet.GetComponent<Bullet>();
                // if (bulletScript != null) { bulletScript.Launch(facingDirectionX); }

                // 총알 차감 및 로그 실행
                status.OnGunAttackExecute();

                return; // 총을 쐈으니 아래의 근접 공격 로직은 건너뜁니다.
            }

            // -------------------------------------------------------------
            // 근접 공격 (기본 상태 또는 소드 상태일 때)
            // -------------------------------------------------------------
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

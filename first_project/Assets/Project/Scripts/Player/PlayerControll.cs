using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControll : MonoBehaviour, PlayerAction.IPlayerActions
{
    [Header("Player Stat")]
    public int jumpCount = 0;
    public float JumpForce = 5f;
    public float MoveSpeed = 5f;

    public PlayerPos playerPosData;

    // 컴포넌트 참조 (상태 관리와 아이템 적용 분리)
    private PlayerStatus status;
    private ItemEffectApplicator itemApplicator;

    private PlayerAction controls;
    private Vector2 moveInput;

    void Awake()
    {
        status = GetComponent<PlayerStatus>();
        itemApplicator = GetComponent<ItemEffectApplicator>();
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
        if (status == null) return;
        // PlayerStatus의 상태를 확인
        if (status.isDead) return;

        Vector3 direction = new Vector3(moveInput.x, 0f, 0f);

        // 좌우 이동 처리
        if (direction != Vector3.zero)
        {
            transform.Translate(direction * MoveSpeed * Time.deltaTime);
        }

        // 데이터 저장
        if (playerPosData != null)
        {
            playerPosData.x = Mathf.RoundToInt(transform.position.x);
            playerPosData.y = Mathf.RoundToInt(transform.position.y);
        }
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
        // 중복 조건문을 지우고 깔끔하게 하나로 합쳤습니다.
        if (context.started && jumpCount < 1 && status.isGrounded)
        {
            GetComponent<Rigidbody2D>().AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
            jumpCount++;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts[0].normal.y > 0.7f)
        {
            status.isGrounded = true;
            jumpCount = 0;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        status.isGrounded = false;
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Debug.Log("공격!");
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

    // 아이템 매니저나 외부 데이터 매니저가 이 함수를 호출할 때의 브릿지 역할들
    public void ExecuteItemEffectByID(int itemNumber)
    {
        switch (itemNumber)
        {
            case 1: // 1번 빨간 포션 사용 시 데이터 매니저가 아닌 실제 실시간 스탯을 올리도록 수정
                if (status != null)
                {
                    status.ChangeHp(50f);
                    Debug.Log($"빨간포션 사용! 현재 체력: {status.currentHp}");
                }
                break;

            case 2: // 2번 은신 물약
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


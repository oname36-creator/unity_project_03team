using UnityEngine;
using UnityEngine.InputSystem;

// PlayerAction 자산에서 자동 생성된 IPlayerActions 인터페이스를 구현합니다.
public class PlayerControll : MonoBehaviour, PlayerAction.IPlayerActions
{
    [Header("Player Stat")]
    public float hp = 100f;
    public bool isStealth = false; // 은신 상태 변수 추가


    public float JumpForce = 5f;
    public float MoveSpeed = 5f;
    private int JumpCount = 0;

    public PlayerPos playerPosData;

    private PlayerAction controls;
    private Vector2 moveInput;

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
        // 2D 게임이므로 Vector2를 그대로 활용하거나, Vector3의 x축에만 매핑합니다.
        // 플랫포머에서 좌우 이동은 x축만 사용하고, 상하(y)는 중력과 점프로 제어하는 경우가 많습니다.
        Vector3 direction = new Vector3(moveInput.x, 0f, 0f);

        // 좌우 이동 처리
        if (direction != Vector3.zero)
        {
            transform.Translate(direction * MoveSpeed * Time.deltaTime);
        }

        // 데이터 저장 (2D 플랫포머이므로 x와 y 좌표를 매핑)
        if (playerPosData != null)
        {
            playerPosData.x = Mathf.RoundToInt(transform.position.x);
            playerPosData.y = Mathf.RoundToInt(transform.position.y); // 이제 y축이 하늘 방향입니다!
        }
    }

    // -----------------------------------------------------------------
    // Input Action 콜백 메서드
    // -----------------------------------------------------------------

    // 키보드 A/D, 왼쪽/오른쪽 화살표를 누르면 이 메서드가 실행됩니다.
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    // 만약 Input Action 에디터에서 'Jump' 액션(Button 타입)을 추가했다면 
    // 아래와 같이 점프 로직을 구현할 수 있습니다.
    public void OnJump(InputAction.CallbackContext context)
    {
        // 버튼을 '누른 순간'에만 점프가 발동하도록 설정
        if (context.started)
        {
            // 예: Rigidbody2D가 있다면 위쪽으로 힘을 줍니다.
            GetComponent<Rigidbody2D>().AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);

            Debug.Log("점프!");

        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        // 버튼을 누른 순간에 공격 로직 실행
        if (context.started)
        {
            Debug.Log("공격!");
            // 여기에 공격 애니메이션 재생이나 발사체 생성 코드를 넣으시면 됩니다.
        }
    }


    public void OnItemUse(InputAction.CallbackContext context)
    {
        // 1. 키를 누른 '시점'에만 딱 한 번 실행되도록 필터링
        if (context.started)
        {
            // 2. 현재 어떤 키가 눌렸는지 이름을 가져옴 (예: "1", "2", "3", "4")
            string pressedKey = context.control.name;

            // 3. 눌린 키에 따라 데이터 매니저에게 각기 다른 슬롯 번호를 요청
            switch (pressedKey)
            {
                case "1":
                    Debug.Log("1번 키 누름 -> 0번 인덱스 슬롯 사용");
                    DataManager.Instance.UseItemSlot(0);
                    break;

                case "2":
                    Debug.Log("2번 키 누름 -> 1번 인덱스 슬롯 사용");
                    DataManager.Instance.UseItemSlot(1);
                    break;

                case "3":
                    Debug.Log("3번 키 누름 -> 2번 인덱스 슬롯 사용");
                    DataManager.Instance.UseItemSlot(2);
                    break;

                case "4":
                    Debug.Log("4번 키 누름 -> 3번 인덱스 슬롯 사용");
                    DataManager.Instance.UseItemSlot(3);
                    break;

                default:
                    Debug.Log($"지정되지 않은 키 입력: {pressedKey}");
                    break;
            }
        }
    }

    public void ExecuteItemEffectByID(int itemNumber)
    {
        Debug.Log($"PlayerController: {itemNumber}번 아이템 효과 실행 요청받음");

        // 아이템 번호별로 효과를 분기 처리합니다. (예시 번호)
        switch (itemNumber)
        {
            case 1: // 1번이 빨간 포션이라면
                DataManager.Instance.PlayerHp += 50;
                Debug.Log($"빨간포션 사용! 현재 체력: {DataManager.Instance.PlayerHp}");
                break;

            case 2: // 2번이 은신 물약이라면
                // 기존에 만들어두었던 은신 코루틴을 실행합니다 (5초 동안 은신)
                StartCoroutine(StealthRoutine(5f));
                break;

            // 3번, 4번 등 다른 특수 아이템이 있다면 여기에 case를 추가하면 됩니다!
            default:
                Debug.LogWarning($"아직 효과가 정의되지 않은 아이템 번호입니다: {itemNumber}");
                break;
        }
    }




    public void ApplyItemEffect(ItemData data)
    {
        if (data == null) return;

        Debug.Log($"{data.itemName} 효과 발동!");

        switch (data.effectType)
        {
            case ItemEffectType.HealHP:
                hp += data.effectValue;
                break;

            case ItemEffectType.Stealth:
                StartCoroutine(StealthRoutine(data.duration));
                break;
        }
    }

    private System.Collections.IEnumerator StealthRoutine(float duration)
    {
        isStealth = true;
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f); // 반투명

        yield return new WaitForSeconds(duration);

        isStealth = false;
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1f); // 원래대로
    }



}



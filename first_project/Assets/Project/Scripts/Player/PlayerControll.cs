using UnityEngine;
using UnityEngine.InputSystem;

// PlayerAction 자산에서 자동 생성된 IPlayerActions 인터페이스를 구현합니다.
public class PlayerControll : MonoBehaviour, PlayerAction.IPlayerActions
{
    public float JumpForce = 0.01f;
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
}
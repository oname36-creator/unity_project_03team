using UnityEngine;
// 1. 상단에 반드시 새 입력 시스템 네임스페이스를 추가해야 합니다.
using UnityEngine.InputSystem;

public class PlayerControll : MonoBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        // 방향을 저장할 벡터
        Vector3 direction = Vector3.zero;

        // 2. 현재 연결된 키보드가 있는지 체크하고 입력을 받습니다.
        if (Keyboard.current != null)
        {
            // A키나 왼쪽 화살표가 눌리면 왼쪽(-1)으로
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                direction.x = -1f;

            // D키나 오른쪽 화살표가 눌리면 오른쪽(1)으로
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                direction.x = 1f;

            // W키나 위쪽 화살표가 눌리면 위로(1)
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                direction.z = 1f; // 3D 환경이라면 z축, 2D라면 y축으로 변경하세요.

            // S키나 아래쪽 화살표가 눌리면 아래로(-1)
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                direction.z = -1f;
        }

        // 3. 계산된 방향과 속도로 캐릭터 이동
        transform.Translate(direction.normalized * moveSpeed * Time.deltaTime);
    }
}
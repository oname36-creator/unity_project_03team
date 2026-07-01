using JetBrains.Annotations;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    public int PlayerHp = 100;
    public int[] PlayerInventory = new int[4];


    public bool AddItem(int itemNumber)
    {
        // 1. 인벤토리 배열을 돌면서 빈 칸(0)이 있는지 찾습니다.
        for (int i = 0; i < PlayerInventory.Length; i++)
        {
            if (PlayerInventory[i] == 0) // 빈 칸을 발견했다면!
            {
                PlayerInventory[i] = itemNumber; // 아이템 번호 저장
                Debug.Log($"인벤토리 {i}번 슬롯에 아이템 {itemNumber} 추가 완료!");
                return true; // 습득 성공!
            }
        }

        // 2. 루프를 다 돌았는데도 0인 칸이 없다면 가득 찬 것입니다.
        Debug.Log("인벤토리가 가득 차서 아이템을 먹을 수 없습니다!");
        return false; // 습득 실패!
    }

    public void UseItemSlot(int slotIndex)
    {
        // 1. 배열 범위를 벗어나는 예외 처리 (0~3번 슬롯만 가능)
        if (slotIndex < 0 || slotIndex >= PlayerInventory.Length) return;

        // 2. 해당 슬롯에 들어있는 아이템 번호 확인
        int itemNumber = PlayerInventory[slotIndex];

        // 3. 빈 슬롯(0)이면 아무것도 하지 않고 함수 종료
        if (itemNumber == 0)
        {
            Debug.Log($"{slotIndex + 1}번 슬롯이 비어있습니다.");
            return;
        }

        // 4. 아이템이 있다면 플레이어 스크립트를 찾아서 효과 적용 요청!
        PlayerControll player = Object.FindAnyObjectByType<PlayerControll>();
        if (player != null)
        {
            // 플레이어에게 아이템 번호를 넘겨주며 효과 실행 요청
            player.ExecuteItemEffectByID(itemNumber);

            // 5. 사용 완료 후 해당 인벤토리 슬롯을 다시 빈칸(0)으로 만듭니다!
            PlayerInventory[slotIndex] = 0;
            Debug.Log($"{slotIndex + 1}번 슬롯의 아이템(ID: {itemNumber}) 사용 및 소모 완료.");
        }
    }


    public void PrintInventoryLog()
    {
        string inventoryStatus = "현재 인벤토리: [ ";

        for (int i = 0; i < PlayerInventory.Length; i++)
        {
            // 빈 칸(0)이면 "빈칸", 아이템이 있으면 아이템 번호를 출력
            if (PlayerInventory[i] == 0)
            {
                inventoryStatus += "빈칸";
            }
            else
            {
                inventoryStatus += $"ID:{PlayerInventory[i]}";
            }

            // 마지막 슬롯이 아니면 쉼표(,)로 구분
            if (i < PlayerInventory.Length - 1)
            {
                inventoryStatus += " | ";
            }
        }

        inventoryStatus += " ]";

        // 콘솔창에 녹색(또는 원하는 색상)으로 돋보이게 출력
        Debug.Log($"<color=green>{inventoryStatus}</color>");
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using UnityEngine;
using UnityEngine.UI; // 💡 UI 컴포넌트(Slider)를 제어하기 위해 꼭 필요해요!

public class HPBarController : MonoBehaviour
{
    // 유니티 에디터에서 드래그앤드롭으로 연결해 줄 슬라이더 변수입니다.
    public Slider hpSlider;

    void Start()
    {
        // 1. 게임이 시작되면 이 슬라이더의 최대치를 100으로 설정합니다.
        if (hpSlider != null)
        {
            hpSlider.maxValue = 100;
        }
    }

    void Update()
    {
        // 2. 매 순간(매 프레임)마다 DataManager에 저장된 체력 값을 슬라이더의 현재 값에 대입합니다.
        if (hpSlider != null && DataManager.Instance != null)
        {
            hpSlider.value = DataManager.Instance.PlayerHp;
        }
    }
}
using UnityEngine;
using UnityEngine.UI; 

public class HPBarController : MonoBehaviour
{
    
    public Slider hpSlider;

    void OnEnable()
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = 100;

            // 홈을 거쳐서 새 씬이 로드될 때 확실하게 값을 먼저 주입
            if (DataManager.Instance != null)
            {
                hpSlider.value = DataManager.Instance.PlayerHp;
                Debug.Log($"[HP UI OnEnable] 씬 로드 즉시 체력 주입: {DataManager.Instance.PlayerHp}");
            }
        }
    }
    void Start()
    {
        // 혹시 모를 타이밍을 위해 Start에서도 한 번 더 안전장치로 주입
        if (hpSlider != null && DataManager.Instance != null)
        {
            hpSlider.value = DataManager.Instance.PlayerHp;
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
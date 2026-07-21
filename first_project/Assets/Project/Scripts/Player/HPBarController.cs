using UnityEngine;
using UnityEngine.UI;

public class HPBarController : MonoBehaviour
{
    [Header("UI 요소")]
    public Slider hpSlider;
    public Image fillImage; // Fill Area 내부의 Fill 이미지 컴포넌트

    [Header("색상 설정")]
    public Gradient hpGradient; // 체력 비율별 색상 설정 (0 = 0%, 1 = 100%)

    private float lastHp = -1f; // 이전 프레임 체력 저장용 (불필요한 색상 계산 방지)

    void OnEnable()
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = 100;

            // 홈을 거쳐서 새 씬이 로드될 때 확실하게 값을 먼저 주입
            if (DataManager.Instance != null)
            {
                hpSlider.value = DataManager.Instance.PlayerHp;
                UpdateHPBarColor();
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
            UpdateHPBarColor();
        }
    }

    void Update()
    {
        // 매 프레임마다 DataManager의 체력 반영
        if (hpSlider != null && DataManager.Instance != null)
        {
            float currentHp = DataManager.Instance.PlayerHp;
            hpSlider.value = currentHp;

            // 체력 값이 바뀌었을 때만 색상 업데이트
            if (!Mathf.Approximately(currentHp, lastHp))
            {
                UpdateHPBarColor();
                lastHp = currentHp;
            }
        }
    }

    // 체력 비율에 맞게 색상을 변경하는 함수
    private void UpdateHPBarColor()
    {
        if (fillImage != null && hpSlider != null)
        {
            // Slider의 0.0 ~ 1.0 비율값 가져오기
            float normalizedHp = hpSlider.normalizedValue;

            // Gradient 색상 적용
            fillImage.color = hpGradient.Evaluate(normalizedHp);
        }
    }
}
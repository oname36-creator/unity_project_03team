using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SurvivalTimer : MonoBehaviour
{
    [Header("Time Settings (초 단위)")]
    [Tooltip("목표 생존 시간 (3분 = 180초)")]
    public float targetSurvivalTime = 180f;
    private float currentTimer = 0f;

    [Header("UI Reference")]
    [Tooltip("시간을 표시할 TextMeshProUGUI (TimerText를 여기에 넣어주세요)")]
    public TextMeshProUGUI timerText;


    private bool isGameOver = false;

    void Start()
    {
        currentTimer = targetSurvivalTime;
        UpdateTimerUI();
    }

    void Update()
    {
        if (isGameOver) return;

        // 1. 시간 차감 (Time.deltaTime은 일시정지(Time.timeScale=0) 시 자동으로 멈춥니다)
        if (currentTimer > 0f)
        {
            currentTimer -= Time.deltaTime;
            UpdateTimerUI();
        }
        // 2. 3분이 지나 처리가 완료되었을 때 (클리어!)
        else
        {
            currentTimer = 0f;
            UpdateTimerUI();
            TriggerGameClear();
        }
    }

    // 시간을 '분:초' 형태로 예쁘게 UI에 업데이트합니다.
    void UpdateTimerUI()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(currentTimer / 60f);
        int seconds = Mathf.FloorToInt(currentTimer % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void TriggerGameClear()
    {
        isGameOver = true;
        SceneManager.LoadScene("GameEnding");
    }
}
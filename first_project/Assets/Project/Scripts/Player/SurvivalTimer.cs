using UnityEngine;
using TMPro;

public class SurvivalTimer : MonoBehaviour
{
    [Header("Time Settings (초 단위)")]
    [Tooltip("목표 생존 시간 (3분 = 180초)")]
    public float targetSurvivalTime = 180f;
    private float currentTimer = 0f;

    [Header("UI Reference")]
    [Tooltip("시간을 표시할 TextMeshProUGUI (TimerText를 여기에 넣어주세요)")]
    public TextMeshProUGUI timerText;

    [Header("Ending Credit Reference")]
    [Tooltip("씬에 배치한 PlaySceneEndingCredits 스크립트를 넣어주세요.")]
    public PlaySceneEndingCredits endingCredits;

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
        //Debug.Log("🎉 3분 버티기 성공! 게임 클리어!");

        //  [안전장치] 플레이어가 클리어 시점에 죽거나 맞지 않도록 안전하게 만듭니다.
        PlayerControll player = FindAnyObjectByType<PlayerControll>();
        if (player != null)
        {
            // 플레이어 움직임 정지 및 무적 처리
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;

            // PlayerStatus가 있다면 가져와서 무적 상태로 만듭니다.
            PlayerStatus status = player.GetComponent<PlayerStatus>();
            if (status != null)
            {
                status.isInvincible = true;
            }

            // 플레이어 조작을 위해 입력 기능을 꺼줍니다.
            player.enabled = false;
        }

        // 3. 엔딩 크레딧 연출을 시작합니다.
        if (endingCredits != null)
        {
            endingCredits.StartEndingCredits();
        }
        else
        {
            Debug.LogError("엔딩 크레딧 스크립트가 타이머에 연결되지 않았습니다!");
        }
    }
}
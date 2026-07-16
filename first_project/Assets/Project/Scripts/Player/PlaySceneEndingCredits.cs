using UnityEngine;
using UnityEngine.SceneManagement;

public class PlaySceneEndingCredits : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("엔딩 크레딧 전체를 담고 있는 최상위 부모 오브젝트")]
    public GameObject creditCanvasGroup;
    [Tooltip("실제 올라갈 텍스트들의 부모(CreditContent) RectTransform")]
    public RectTransform creditContent;
    [Tooltip("배경 검은 화면 (페이드인 연출용)")]
    public UnityEngine.UI.Image blackBackground;

    [Header("Post-Credit UI (크레딧 종료 후 띄울 버튼들)")]
    [Tooltip("크레딧이 끝난 뒤 활성화할 버튼 그룹 (다시하기/종료 등)")]
    public GameObject finishButtonsGroup;

    [Header("Movement Settings")]
    [Tooltip("글자가 올라가는 속도")]
    public float scrollSpeed = 40f;
    [Tooltip("이 Y 좌표까지 올라가면 크레딧이 끝납니다.")]
    public float endYPosition = 1200f;

    [Header("Fade Settings")]
    [Tooltip("검은 화면이 서서히 어두워지는 시간(초)")]
    public float fadeDuration = 1.5f;

    private bool isPlaying = false;
    private bool isFinished = false;
    private float currentFadeTime = 0f;

    void Start()
    {
        // 시작할 때는 엔딩 크레딧과 버튼들을 확실히 꺼둡니다.
        if (creditCanvasGroup != null) creditCanvasGroup.SetActive(false);
        if (finishButtonsGroup != null) finishButtonsGroup.SetActive(false);
    }

    /// <summary>
    /// 3분 타이머 등에서 호출하여 크레딧 연출을 시작합니다.
    /// </summary>
    public void StartEndingCredits()
    {
        if (isPlaying) return;
        isPlaying = true;

        if (creditCanvasGroup != null) creditCanvasGroup.SetActive(true);
        if (finishButtonsGroup != null) finishButtonsGroup.SetActive(false);

        // 자막 위치 및 배경 초기화
        if (creditContent != null)
        {
            creditContent.anchoredPosition = new Vector2(creditContent.anchoredPosition.x, 0f);
        }

        if (blackBackground != null)
        {
            Color color = blackBackground.color;
            color.a = 0f;
            blackBackground.color = color;
        }

        // 💡 물리/몬스터 시스템 등을 일시정지 시킵니다.
        Time.timeScale = 0f;
    }

    void Update()
    {
        if (!isPlaying || isFinished) return;

        // 1. 검은 배경 페이드인
        if (blackBackground != null && blackBackground.color.a < 1f)
        {
            currentFadeTime += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(currentFadeTime / fadeDuration);
            Color color = blackBackground.color;
            color.a = alpha;
            blackBackground.color = color;
        }

        // 2. 자막 올리기 (unscaledDeltaTime 사용으로 일시정지 상태에서도 움직임)
        if (creditContent != null)
        {
            creditContent.anchoredPosition += Vector2.up * scrollSpeed * Time.unscaledDeltaTime;

            // 3. 목표 위치 도달 시 종료 연출 실행
            if (creditContent.anchoredPosition.y >= endYPosition)
            {
                isFinished = true;
                OnCreditsFinished();
            }
        }
    }

    private void OnCreditsFinished()
    {
        Debug.Log("엔딩 크레딧 종료! 선택 버튼들을 화면에 표시합니다.");

        // 씬을 이동하는 대신, 화면 중앙에 [다시 하기 / 종료] 버튼을 띄웁니다.
        if (finishButtonsGroup != null)
        {
            finishButtonsGroup.SetActive(true);
        }
    }

    // ----------------------------------------------------
    // 버튼 연결용 함수들 (인스펙터의 Button -> OnClick에 연결해서 사용하세요!)
    // ----------------------------------------------------

    /// <summary>
    /// 게임을 처음부터 다시 시작합니다. (현재 씬 재로드)
    /// </summary>
    public void OnClickRestart()
    {
        Time.timeScale = 1f; // 일시정지 해제 필수!
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// 빌드된 게임인 경우 게임을 완전히 종료합니다.
    /// </summary>
    public void OnClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
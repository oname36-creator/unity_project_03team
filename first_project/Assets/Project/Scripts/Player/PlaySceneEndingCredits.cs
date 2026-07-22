using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlaySceneEndingCredits : MonoBehaviour
{
    [Header("Cutscene Image (새로 추가)")]
    [Tooltip("크레딧 직전에 띄울 연출 이미지 (Image 컴포넌트)")]
    public Image cutsceneImage;
    [Tooltip("이미지 페이드 인 -> 대기 -> 페이드 아웃에 걸리는 총 시간 (기본 8초)")]
    public float cutsceneTotalDuration = 8.0f;

    [Header("UI References")]
    public GameObject creditCanvasGroup;
    public RectTransform creditContent;
    public Image blackBackground;

    [Header("Post-Credit UI (크레딧 종료 후 띄울 버튼들)")]
    public GameObject finishButtonsGroup;

    [Header("Movement Settings")]
    public float scrollSpeed = 40f;
    public float endYPosition = 1200f;

    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;

    private bool isPlaying = false;

    void Start()
    {
        if (creditCanvasGroup != null) creditCanvasGroup.SetActive(false);
        if (finishButtonsGroup != null) finishButtonsGroup.SetActive(false);
        if (cutsceneImage != null) cutsceneImage.gameObject.SetActive(false);
    }

    public void StartEndingCredits()
    {
        if (isPlaying) return;
        isPlaying = true;

        Time.timeScale = 0f; // 게임 일시정지

        // 전체 코루틴 프로세스 시작
        StartCoroutine(EndingSequenceRoutine());
    }


    private IEnumerator EndingSequenceRoutine()
    {
        // -----------------------------------------------------------
        // 1단계: 컷씬 이미지 페이드 인 & 유지 (페이드 아웃 제거)
        // -----------------------------------------------------------
        if (cutsceneImage != null)
        {
            cutsceneImage.gameObject.SetActive(true);
            cutsceneImage.transform.SetAsLastSibling(); // 최상단으로 이동

            Color imgColor = cutsceneImage.color;
            imgColor.a = 0f;
            cutsceneImage.color = imgColor;

            float fadeInDuration = 1.5f;
            float holdDuration = 6.5f; // 총 8초 중 페이드 인 1.5초 + 유지 6.5초

            // 1-1. Cutscene Image Fade In (0 -> 1)
            float timer = 0f;
            while (timer < fadeInDuration)
            {
                timer += Time.unscaledDeltaTime;
                imgColor.a = Mathf.Clamp01(timer / fadeInDuration);
                cutsceneImage.color = imgColor;
                yield return null;
            }

            // 1-2. 이미지 그대로 유지 (6.5초 대기)
            yield return new WaitForSecondsRealtime(holdDuration);
        }

        // -----------------------------------------------------------
        // 2단계: 이미지 위로 검은 배경(또는 크레딧)을 바로 페이드 인으로 덮기
        // -----------------------------------------------------------
        if (creditCanvasGroup != null) creditCanvasGroup.SetActive(true);

        // 자막 초기 위치 설정
        if (creditContent != null)
        {
            creditContent.anchoredPosition = new Vector2(creditContent.anchoredPosition.x, -300f);
        }

        // 검은 배경을 컷씬 이미지 위로 페이드 인 시켜 자연스럽게 전환
        if (blackBackground != null)
        {
            // 검은 배경의 Canvas/UI 레이어를 컷씬 이미지보다 앞으로 가져옴
            blackBackground.transform.SetAsLastSibling();

            float timer = 0f;
            Color bgCol = blackBackground.color;
            bgCol.a = 0f; // 투명하게 시작
            blackBackground.color = bgCol;

            while (timer < fadeDuration)
            {
                timer += Time.unscaledDeltaTime;
                bgCol.a = Mathf.Clamp01(timer / fadeDuration);
                blackBackground.color = bgCol;
                yield return null;
            }
        }

        // 검은 배경 뒤에 숨은 컷씬 이미지는 굳이 보이지 않으므로 비활성화 정리
        if (cutsceneImage != null)
        {
            cutsceneImage.gameObject.SetActive(false);
        }

        // -----------------------------------------------------------
        // 3단계: 자막 스크롤 시작
        // -----------------------------------------------------------
        if (creditContent != null)
        {
            // 글자 UI를 맨 앞으로 끌어올림
            creditContent.transform.SetAsLastSibling();

            while (creditContent.anchoredPosition.y < endYPosition)
            {
                creditContent.anchoredPosition += Vector2.up * scrollSpeed * Time.unscaledDeltaTime;
                yield return null;
            }
        }


        OnCreditsFinished();
    }

    private void OnCreditsFinished()
    {
        //Debug.Log("엔딩 연출 완료! 버튼 표시");
        if (finishButtonsGroup != null)
        {
            finishButtonsGroup.SetActive(true);
        }
    }

    public void OnClickRestart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
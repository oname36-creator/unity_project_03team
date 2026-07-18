using UnityEngine;
using System.Collections;

public class ControlGuide : MonoBehaviour
{
    [Header("UI Settings")]
    [Tooltip("Canvas Group이 붙은 조작키 가이드 UI 오브젝트를 넣어주세요.")]
    public CanvasGroup guideCanvasGroup;

    [Header("Timer Settings")]
    [Tooltip("UI를 보여줄 시간(초)입니다.")]
    public float displayDuration = 10f;
    [Tooltip("UI가 사라지는 데 걸리는 시간(초)입니다.")]
    public float fadeDuration = 1f;

    void Start()
    {
        if (guideCanvasGroup != null)
        {
            // 게임 시작 시 UI를 확실하게 보이도록 초기화
            guideCanvasGroup.alpha = 1f;
            guideCanvasGroup.gameObject.SetActive(true);

            // 10초 대기 후 사라지는 코루틴 시작
            StartCoroutine(FadeOutGuideRoutine());
        }
        else
        {
            Debug.LogWarning("Guide Canvas Group이 지정되지 않았습니다! 인스펙터를 확인하세요.");
        }
    }

    private IEnumerator FadeOutGuideRoutine()
    {
      
        yield return new WaitForSecondsRealtime(displayDuration);

      
        float currentTime = 0f;
        while (currentTime < fadeDuration)
        {
            currentTime += Time.unscaledDeltaTime;
            guideCanvasGroup.alpha = Mathf.Lerp(1f, 0f, currentTime / fadeDuration);
            yield return null;
        }

     
        guideCanvasGroup.alpha = 0f;
        guideCanvasGroup.gameObject.SetActive(false);
    }
}

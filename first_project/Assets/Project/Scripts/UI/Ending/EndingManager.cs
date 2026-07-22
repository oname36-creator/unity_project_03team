using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro 사용 시

public class EndingManager : MonoBehaviour
{
    [Header("UI Canvas Group")]
    [SerializeField] private CanvasGroup creditCanvasGroup; // 크레딧 UI 전체 그룹
    [SerializeField] private CanvasGroup endingImageCanvasGroup; // 엔딩 이미지 UI 그룹
    [SerializeField] private CanvasGroup endTextCanvasGroup; // END 텍스트 UI 그룹

    [Header("Credit BackGround")]
    [SerializeField] private GameObject backGround;


    [Header("Ending Image & END Text")]
    [SerializeField] private float endingImageDuration = 8f; // 엔딩 이미지 유지 시간 (8초)
    [SerializeField] private float fadeDuration = 1f; // 페이드 인/아웃 시간

    [Header("Scene Transition")]
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // 메인 메뉴 씬 이름


    private bool _credit = true;

    private void Start()
    {
        // 초기 UI 상태 설정 (모두 숨김)
        SetCanvasGroupAlpha(creditCanvasGroup, 1f); // 크레딧은 처음부터 표시
        SetCanvasGroupAlpha(endingImageCanvasGroup, 0f);
        SetCanvasGroupAlpha(endTextCanvasGroup, 0f);

        // 엔딩 연출 시퀀스 시작
        StartCoroutine(PlayEndingSequence());
    }

    private void Update()
    {
        if (backGround.activeSelf) 
        {
            backGround.SetActive(false);
        }
    
    }

    public void CreditEnd() 
    {
        _credit = false;
    }


    private IEnumerator PlayEndingSequence()
    {
        // 1. 크레딧 진행 (10초간 스크롤 대기)
        SoundManager.Instance.PlayBGM("EndingSceneBGM");
        creditCanvasGroup.gameObject.SetActive(true);

        while (_credit)
        {
            yield return null;
        }

        // 2. 크레딧 서서히 사라짐 & 엔딩 이미지 페이드 인
        yield return new WaitForSeconds(endingImageDuration);
        yield return StartCoroutine(FadeCanvasGroup(endingImageCanvasGroup, 0f, 1f, fadeDuration));

        // 3. 엔딩 이미지 8초간 유지
        yield return new WaitForSeconds(endingImageDuration);

        // 4. END 텍스트 나타남 (이미지 위에 겹치거나 이미지 대체)
        yield return StartCoroutine(FadeCanvasGroup(endTextCanvasGroup, 0f, 1f, fadeDuration));

        // 5. 3초 정도 END 문구를 더 보여준 후 메인 메뉴로 이동
        SoundManager.Instance.PauseBGM();
        yield return new WaitForSeconds(3f);

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        if (cg == null) yield break;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, timer / duration);
            yield return null;
        }
        cg.alpha = end;
    }

    private void SetCanvasGroupAlpha(CanvasGroup cg, float alpha)
    {
        if (cg != null)
        {
            cg.alpha = alpha;
        }
    }
}
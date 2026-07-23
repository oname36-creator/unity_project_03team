using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(CanvasGroup))]
public class LoadingPanel : MonoBehaviour
{
    [Header("페이드 설정")]
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("인게임 UI 설정")]
    [SerializeField] private GameObject inGameUI;
    [SerializeField] private GameObject DistanceUI;
    [SerializeField] private GameObject ControlGuideUI;
    private CanvasGroup _canvasGroup;
    private CanvasGroup _canvasGuideUI;


    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGuideUI = GetComponent<CanvasGroup>();
        // 시작 시에는 로딩 화면이 완전히 불투명하게 보이도록 설정
        _canvasGroup.alpha = 1f;
        //_canvasGuideUI.alpha = 1f;
        _canvasGroup.blocksRaycasts = true; // 로딩 중 클릭 차단
        //_canvasGuideUI.blocksRaycasts = true;

        // 시작 시 인게임 플레이어 UI는 보이지 않도록 비활성화
        if(inGameUI != null)
        {
            inGameUI.SetActive(false);
            DistanceUI.SetActive(false);
            //ControlGuideUI.SetActive(false);
        }
    }

    #region Event
    private void OnEnable()
    {
        // 맵/배경 준비 완료 이벤트 구독
        MapManager.OnMapReady += StartFadeOut;
    }
    private void OnDisable()
    {
        // 이벤트 해제
        MapManager.OnMapReady -= StartFadeOut;
    }
    #endregion

    private void StartFadeOut()
    {
        // 페이드 아웃이 시작될 때 UI를 다시 킴
        if(inGameUI != null)
        {
            inGameUI.SetActive(true);
            DistanceUI.SetActive(true);
            // ControlGuideUI.SetActive(true);
            SoundManager.Instance.StopBGM();
            SoundManager.Instance.PlayBGM("GameSceneBGM");
        }
        StartCoroutine(CoFadeOutRoutine());
    }

    #region CoRoutine
    private IEnumerator CoFadeOutRoutine()
    {
        float elapsedTime = 0f;
        
        while(elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            // 시간에 비례하여 알파값 감소
            _canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        // 완전히 사라진 후의 처리
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }
    #endregion
}

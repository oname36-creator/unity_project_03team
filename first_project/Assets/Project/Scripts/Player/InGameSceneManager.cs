using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement; 

public class InGameSceneManager : MonoBehaviour
{
    [Header("이 씬에 배치된 일시정지 UI 오브젝트")]
    public GameObject localPauseMenuUI;
    public GameObject localGameOverUI;
    public GameObject localFinishGameUI;

    [Header("Setting Popup UI")]
    public GameObject settingPanel;
    void Start()
    {
      
            // VSync를 비활성화 (TargetFrameRate를 적용하기 위해 필요)
            QualitySettings.vSyncCount = 0;

            // 목표 프레임을 60FPS로 고정
            Application.targetFrameRate = 60;
        
        if (SceneManagerEx.Instance != null)
        {
            SceneManagerEx.Instance.pauseMenuUI = localPauseMenuUI;
            SceneManagerEx.Instance.gameOverUI = localGameOverUI;
            SceneManagerEx.Instance.gameFinishUI = localFinishGameUI;
           
            if (localPauseMenuUI != null)
            {
                localPauseMenuUI.SetActive(false);
            }
            if (localGameOverUI != null)
            {
                localGameOverUI.SetActive(false);
            }
            if (localFinishGameUI != null)
            {
                localFinishGameUI.SetActive(false);
            }
            if (settingPanel != null)
            {
                settingPanel.SetActive(false);
            }
        }
    }
    void Update()
    {
        // New Input System 키보드 감지
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            // 기존에 작성해두신 재시작 버튼 로직을 그대로 실행합니다.
            Btn_Restart();
        }
    }

    public void Btn_Resume()
    {
        if (SceneManagerEx.Instance != null)
        {
            SceneManagerEx.Instance.ResumeGame();
        }
    }

    
    public void Btn_GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameStartScene");
        //Debug.Log("첫 화면 씬으로 이동 완료!");
    }

    public void Btn_Restart()
    {
        if(SceneManagerEx.Instance != null)
        {
            SceneManagerEx.Instance.Btn_Restart();
           
        }
        if (DataManager.Instance != null)
        {
            DataManager.Instance.ResetGameData();
        }
    }
    public void Btn_OpenSetting()
    {
        if (settingPanel != null)
        {
            settingPanel.SetActive(true);
            settingPanel.transform.SetAsLastSibling(); // 팝업이 PauseMenu 내부 다른 요소들보다 위에 오도록 설정
        }
    }

   
    public void Btn_CloseSetting()
    {
        if (settingPanel != null)
        {
            settingPanel.SetActive(false);
        }
    }
}
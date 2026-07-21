using UnityEngine;
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
        Debug.Log("첫 화면 씬으로 이동 완료!");
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
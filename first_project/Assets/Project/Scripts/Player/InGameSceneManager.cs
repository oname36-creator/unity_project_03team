using UnityEngine;
using UnityEngine.SceneManagement; // 💡 씬 전환을 위해 필수!

public class InGameSceneManager : MonoBehaviour
{
    [Header("이 씬에 배치된 일시정지 UI 오브젝트")]
    public GameObject localPauseMenuUI;
    public GameObject localGameOverUI;

    void Start()
    {
        if (SceneManagerEx.Instance != null)
        {
            SceneManagerEx.Instance.pauseMenuUI = localPauseMenuUI;
            SceneManagerEx.Instance.gameOverUI = localGameOverUI;
            if (localPauseMenuUI != null)
            {
                localPauseMenuUI.SetActive(false);
            }
            if (localGameOverUI != null)
            {
                localGameOverUI.SetActive(false);
            }
        }
    }

    // 💡 버튼이 클릭되었을 때 실행될 함수 1 (Resume)
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
        SceneManager.LoadScene("GameStartScene"); // 본인의 첫 화면 씬 이름
        Debug.Log("첫 화면 씬으로 이동 완료!");
    }

    public void Btn_Restart()
    {
        if(SceneManagerEx.Instance != null)
        {
            SceneManagerEx.Instance.Btn_Restart();
        }
    }
}
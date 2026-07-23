using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx : Singleton<SceneManagerEx>
{
    private bool isPaused = false;

    [HideInInspector]
    public GameObject pauseMenuUI;
    [HideInInspector]
    public GameObject gameOverUI;
    [HideInInspector]
    public GameObject gameFinishUI;

    public override void Awake()
    {
        base.Awake();
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPaused) ResumeGame();
                else PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        SoundManager.Instance.PauseBGM();
        SoundManager.Instance.PauseSFX();
        Debug.Log("게임 일시정지");
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        SoundManager.Instance.ResumeBGM();
        SoundManager.Instance.ResumeSFX();
        Debug.Log("게임 재개");
    }

    public void LoadGameScene()
    {
      
        Time.timeScale = 1f;
        isPaused = false; // 일시정지 상태 변수도 거짓으로 초기화
        if (DataManager.Instance != null)
        {
            DataManager.Instance.ResetGameData();
        }
        Debug.Log("게임 시작");
        SceneManager.LoadScene("GameScene");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    public void GameOver()
    {
        Time.timeScale = 0f; // 게임 정지

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true); // 사망 창 켜기
            SoundManager.Instance.PauseBGM();
        }
        Debug.Log("게임 오버 UI 활성화");
    }

    // 다시 시작 버튼용 함수
    public void Btn_Restart()
    {
        Time.timeScale = 1f; 
     
        // 현재 씬("GameScenePlayer")을 다시 로드하여 처음부터 시작하게 만듭니다.
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        Debug.Log("게임 다시 시작");
    }
}
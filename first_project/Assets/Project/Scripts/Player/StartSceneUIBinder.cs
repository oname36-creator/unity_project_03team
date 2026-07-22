using UnityEngine;
using UnityEngine.UI; // 💡 버튼 컴포넌트를 제어하기 위해 필수!

public class StartSceneUIBinder : MonoBehaviour
{
    [Header("메인 화면의 버튼들")]
    public Button gameStartButton;
    public Button gameExitButton;

    void Start()
    {
        // 씬이 시작되면 DontDestroyOnLoad로 살아남은 싱글톤을 찾습니다.
        if (SceneManagerEx.Instance != null)
        {
           
            if (gameStartButton != null)
            {
                gameStartButton.onClick.RemoveAllListeners();
               
                gameStartButton.onClick.AddListener(SceneManagerEx.Instance.LoadGameScene);
            }

            if (gameExitButton != null)
            {
                gameExitButton.onClick.RemoveAllListeners();
             
                gameExitButton.onClick.AddListener(SceneManagerEx.Instance.QuitGame);
            }

            SoundManager.Instance.PlayBGM("StartSceneBGM");
            //Debug.Log("메인 화면 버튼들이 SceneManagerEx 싱글톤에 코드로 자동 연동되었습니다.");
        }
        else
        {
            Debug.LogError("SceneManagerEx 싱글톤을 찾을 수 없습니다!");
        }
    }
}


using UnityEngine;
using UnityEngine.SceneManagement;

// 클래스 이름을 완전히 고유한 GameSceneChanger로 변경합니다.
public class GameSceneChanger : Singleton<GameSceneChanger>
{
    public override void Awake()
    {
        // 부모 싱글톤의 Awake를 실행하여 DontDestroyOnLoad 등을 보장합니다.
        base.Awake();
    }

    // [게임 시작] 버튼용 함수
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // [게임 종료] 버튼용 함수
    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
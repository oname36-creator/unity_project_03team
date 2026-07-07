using UnityEngine;
using UnityEngine.SceneManagement;

// 클래스 이름을 MySceneChanger로 변경하고, 싱글톤 제네릭 타입도 맞춰줍니다.
public class MySceneChanger : Singleton<MySceneChanger>
{
    public override void Awake()
    {
        base.Awake();
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}

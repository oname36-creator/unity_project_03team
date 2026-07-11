using UnityEngine;
using UnityEngine.SceneManagement; // 씬 전환을 위해 반드시 필요해요!

// 이전에 만들어두신 Singleton을 상속받습니다.
public class SceneManagerEx : Singleton<SceneManagerEx>
{
    public override void Awake()
    {
        // 부모(Singleton)의 Awake 메소드를 실행해 싱글톤 초기화를 해줍니다.
        base.Awake();
    }

    // [게임 시작] 버튼을 누르면 호출할 함수
       public void LoadGameScene()
    {
        // 버튼이 눌리면 유니티 하단 콘솔(Console) 창에 이 문장이 뜹니다!
        Debug.Log("게임 시작 버튼이 눌렸습니다!");

        SceneManager.LoadScene("GameScenePlayer");
    }

    // [종료] 버튼을 누르면 호출할 함수
    public void QuitGame()
    {
#if UNITY_EDITOR
        // 유니티 에디터 상에서 테스트할 때 꺼지도록 하는 코드
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 실제 빌드된 게임이 종료되는 코드
        Application.Quit();
#endif
    }
}
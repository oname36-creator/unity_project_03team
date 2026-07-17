#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SoundManager))]
public class SoundManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 1. 기존 SoundManager의 인스펙터 UI(AudioClip, Volume 설정 등)를 그대로 출력합니다.
        base.OnInspectorGUI();

        SoundManager soundManager = (SoundManager)target;

        // 2. 시각적 구분을 위한 여백과 제목
        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("🎵 BGM 테스트 (플레이 모드 전용)", EditorStyles.boldLabel);

        // 3. 에디터가 플레이 모드일 때만 버튼을 활성화합니다.
        EditorGUI.BeginDisabledGroup(!Application.isPlaying);

        // 버튼 클릭 시 해당 키의 BGM을 재생하도록 설정합니다.
        if (GUILayout.Button("재생: Start Scene BGM", GUILayout.Height(30)))
        {
            soundManager.PlayBGM(SoundManager.StartSceneBGM);
        }

        if (GUILayout.Button("재생: Game Scene BGM", GUILayout.Height(30)))
        {
            soundManager.PlayBGM(SoundManager.GameSceneBGM);
        }
        if (GUILayout.Button("재생: Game Scene BGM1", GUILayout.Height(30)))
        {
            soundManager.PlayBGM(SoundManager.GameSceneBGM1);
        }
        if (GUILayout.Button("재생: Game Scene BGM2", GUILayout.Height(30)))
        {
            soundManager.PlayBGM(SoundManager.GameSceneBGM2);
        }
        if (GUILayout.Button("재생: Game Scene BGM3", GUILayout.Height(30)))
        {
            soundManager.PlayBGM(SoundManager.GameSceneBGM3);
        }
        if (GUILayout.Button("재생: Game Scene BGM4", GUILayout.Height(30)))
        {
            soundManager.PlayBGM(SoundManager.GameSceneBGM4);
        }
        if (GUILayout.Button("재생: Game Scene BGM5", GUILayout.Height(30)))
        {
            soundManager.PlayBGM(SoundManager.GameSceneBGM5);
        }
        if (GUILayout.Button("재생: Game Scene BGM6", GUILayout.Height(30)))
        {
            soundManager.PlayBGM(SoundManager.GameSceneBGM6);
        }

        if (GUILayout.Button("재생: Ending Scene BGM", GUILayout.Height(30)))
        {
            soundManager.PlayBGM(SoundManager.EndingSceneBGM);
        }
        if (GUILayout.Button("재생: Ending Scene BGM1", GUILayout.Height(30)))
        {
            soundManager.PlayBGM(SoundManager.EndingSceneBGM1);
        }

        EditorGUI.EndDisabledGroup();

        // 4. 플레이 모드가 아닐 때 안내 메시지 출력
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("플레이 모드에서만 BGM을 테스트할 수 있습니다.", MessageType.Info);
        }
    }
}
#endif
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor.U2D.Aseprite;


#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public struct SGimmickBakeSetting
{
    public string groupName;
    public EGimmickType gimmickType;
    public Transform parentTransform;
    public List<GameObject> prefabList;
}

public class MapBaker : MonoBehaviour
{
    [Header("데이터")]
    [SerializeField] private SOMapData mapData;     // 저장할 SO 파일

    // 기믹 종류 여기다가 넣으면 됨
    [Header("기믹 종류 세팅 목록")]
    [SerializeField] private List<SGimmickBakeSetting> bakeSettings;

    // 인스펙터 창에서 우클릭하면 나타나는 창
    [ContextMenu("내가 원하는 오브젝트로 생성된 타일 이동")]
    public void BakeAllMapData()
    {
        if(mapData == null)
        {
            Debug.LogError("[MapBaker] 저장할 SOMapData가 연결되지 않았습니다!");
            return;
        }

        // 초기화
        mapData.gimmicList.Clear();

        // 순회
        foreach(SGimmickBakeSetting setting in bakeSettings)
        {
            // 비어있지 않을때만 실행
            if (setting.parentTransform != null && setting.prefabList != null && setting.prefabList.Count > 0)
            { 
                BakeGroup(setting.parentTransform, setting.prefabList, setting.gimmickType);
            }
        }    
       
        
#if UNITY_EDITOR
        EditorUtility.SetDirty(mapData);
        AssetDatabase.SaveAssets();     // 변경된 에셋 파일 디스크에 즉시 물리 저장
#endif
        Debug.Log($"<color=cyan>[MapBaker] 베이킹 완료!");
    }

    private void BakeGroup(Transform parent, List<GameObject> prefabTypeList, EGimmickType type)
    {
        for(int i=0; i<parent.childCount; ++i)
        {
            GameObject child = parent.GetChild(i).gameObject;

#if UNITY_EDITOR
            // 이 씬 오브젝트가 원래 어떤 프리팹에서 나왔는지 찾기
            GameObject sourcePrefab = PrefabUtility.GetCorrespondingObjectFromSource(child);
#else
            GameObject sourcePrefab = null
#endif
            int matchedIndex = prefabTypeList.FindIndex(p => p == sourcePrefab);

            if (matchedIndex == -1)
            {
                Debug.LogWarning($"[MapBaker] '{child.name}'는 등록된 프리팹 종류와 일치하지 않아 건너뜁니다.");
                continue;
            }

            mapData.gimmicList.Add(new SGimmickSpawnData
            {
                gimmickType = type,
                position = child.transform.position,
                prefabIndex = matchedIndex
            });

        }
    }
}


// 예외 처리 (연결이 안 되어 있으면 중단)
//if (mapData == null || trapParent == null || trapPrefabTypes == null || tempTrapTilemap == null || trapPrefabTypes.Count == 0)
//{
//    Debug.LogError("[MapBaker] 연결되지 않은 오브젝트가 있습니다!");
//    return;
//}
//// 1. 초기화
//mapData.trapList.Clear();

//// 내가 옮기려는 오브젝트 밑에 있던 것들 청소 (다시 구울 때 중복 방지)
//for (int i = trapParent.childCount - 1; i >= 0; i--)
//{
//    DestroyImmediate(trapParent.GetChild(i).gameObject);
//}

//int count = 0;
//int childCount = trapParent.childCount;
//for (int i = tempTrapTilemap.childCount - 1; i >= 0; i--)
//{
//    GameObject child = tempTrapTilemap.GetChild(i).gameObject;

//#if UNITY_EDITOR
//    // 이 씬 오브젝트가 원래 어떤 프리팹에서 나왔는지 찾기
//    GameObject sourcePrefab = PrefabUtility.GetCorrespondingObjectFromSource(child);
//#else
//           GameObject sourcePrefab = null;
//#endif
//    int matchedIndex = trapPrefabTypes.FindIndex(p => p == sourcePrefab);

//    if (matchedIndex == -1)
//    {
//        Debug.LogWarning($"[MapBaker] '{child.name}'는 등록된 프리팹 종류와 일치하지 않아 건너뜁니다.");
//        continue;
//    }
//    mapData.trapList.Add(new STrapSpawnData
//    {
//        position = child.transform.position,
//        prefabIndex = matchedIndex
//    });

//    child.transform.SetParent(trapParent);

//    count++;
//}
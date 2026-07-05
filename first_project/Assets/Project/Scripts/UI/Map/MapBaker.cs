using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

// 타일과 프리팹을 1:1 매칭시키는 구조체
[System.Serializable]
public struct STrapTileMapping
{
    public TileBase Tile;
    public GameObject Prefab;
}
public class MapBaker : MonoBehaviour
{
    [Header("데이터 및 에셋 세팅")]
    [SerializeField] private SOMapData mapData;     // 저장할 SO 파일
    [SerializeField] private List<GameObject> trapPrefabTypes; // 함정 종류 등록

    [Header("씬 오브젝트 세팅")]
    [SerializeField] private Transform tempTrapTilemap;
    [SerializeField] private Transform trapParent; // 실제로 생성할 가시

    // 인스펙터 창에서 우클릭하면 나타나는 창
    [ContextMenu("내가 원하는 오브젝트로 생성된 타일 이동")]
    public void BakeMapData()
    {
        // 예외 처리 (연결이 안 되어 있으면 중단)
        if(mapData == null || trapParent == null || trapPrefabTypes == null || tempTrapTilemap == null|| trapPrefabTypes.Count == 0)
        {
            Debug.LogError("[MapBaker] 연결되지 않은 오브젝트가 있습니다!");
            return;
        }
        // 1. 초기화
        mapData.trapList.Clear();

        // 내가 옮기려는 오브젝트 밑에 있던 것들 청소 (다시 구울 때 중복 방지)
        for(int i= trapParent.childCount-1; i >= 0; i--)
        {
            DestroyImmediate(trapParent.GetChild(i).gameObject);
        }

        int count = 0;
        int childCount = trapParent.childCount;
        for(int i = tempTrapTilemap.childCount-1; i>=0; i--)
        {
            GameObject child = tempTrapTilemap.GetChild(i).gameObject;

#if UNITY_EDITOR
            // 이 씬 오브젝트가 원래 어떤 프리팹에서 나왔는지 찾기
            GameObject sourcePrefab = PrefabUtility.GetCorrespondingObjectFromSource(child);
#else
           GameObject sourcePrefab = null;
#endif
            int matchedIndex = trapPrefabTypes.FindIndex(p => p == sourcePrefab);

            if(matchedIndex == -1)
            {
                Debug.LogWarning($"[MapBaker] '{child.name}'는 등록된 프리팹 종류와 일치하지 않아 건너뜁니다.");
                continue;
            }
            mapData.trapList.Add(new STrapSpawnData
            {
                position = child.transform.position,
                prefabIndex = matchedIndex
            });

            child.transform.SetParent(trapParent);

            count++;
        }
        
#if UNITY_EDITOR
        EditorUtility.SetDirty(mapData);
        AssetDatabase.SaveAssets();     // 변경된 에셋 파일 디스크에 즉시 물리 저장
#endif
        Debug.Log($"<color=cyan>[MapBaker] 베이킹 완료!");
    }
}

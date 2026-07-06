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

    [Header("임시 타일맵")]
    [Tooltip("오브젝트 브러시 사용 시 프리펩이 저장되는 임시 부모 오브젝트를 넣어주세요")]
    [SerializeField] private Transform paintedObjectSource;

    [Header("기믹 종류 세팅 목록")]
    [SerializeField] private List<SGimmickBakeSetting> bakeSettings;

    // 인스펙터 창에서 우클릭하면 나타나는 창
    [ContextMenu("내가 원하는 오브젝트로 생성된 타일 이동")]
    public void BakeAllMapData()
    {
        // SO 예외처리
        if(mapData == null)
        {
            Debug.LogError("[MapBaker] 저장할 SOMapData가 연결되지 않았습니다!");
            return;
        }
        // 임시로부터 오브젝트 이동
        int organizeCount = OrganizeNewObjects();

        // SO로 데이터 보냄
        BakeAllData();

#if UNITY_EDITOR
        EditorUtility.SetDirty(mapData);
        EditorUtility.SetDirty(this.gameObject);
        AssetDatabase.SaveAssets();     // 변경된 에셋 파일 디스크에 즉시 물리 저장
#endif
        Debug.Log($"<color=cyan>[MapBaker] 베이킹 완료! 신규 {organizeCount}개 이동됨");
    }

   

    // 임시 부모 폴더를 내가 원하는 폴더로 이동
    private int OrganizeNewObjects()
    {
        int count = 0;
        for(int i= paintedObjectSource.childCount - 1; i >=0; --i)
        {
            GameObject child = paintedObjectSource.GetChild(i).gameObject;
            // BakeAllData와 겹치는 부분임 리펙토링 필요
            string cleanName = child.name.Replace("(Clone)", "").Trim();
            int bracketIndex = cleanName.LastIndexOf(" (");
            if(bracketIndex > 0)
            {
                cleanName = cleanName.Substring(0, bracketIndex);
            }

            bool isMoved = false;
            foreach(var setting in bakeSettings)
            {
                if(setting.prefabList.Exists(p => p != null && p.name == cleanName))
                {
                    child.transform.SetParent(setting.parentTransform);
                    count++;
                    isMoved = true;
                    break;
                }
            }

            // 예외처리
            if(!isMoved)
            {
                Debug.LogWarning($"[MapBaker] '{cleanName}'은(는) 인스펙터 프리팹 리스트에 등록되지 않아 이동되지 않았습니다!");

            }
        }
        return count;
    }

    private void BakeAllData()
    {
        mapData.gimmicList.Clear();

        foreach(SGimmickBakeSetting setting in bakeSettings)
        {
            if (setting.parentTransform == null)
                continue;
            for(int i=0; i < setting.parentTransform.childCount; ++i)
            {
                GameObject child = setting.parentTransform.GetChild(i).gameObject;

                // 리펙토링 할 수 있을지도?
                string cleanName = child.name.Replace("(Clone)", "").Trim();
                int bracketIndex = cleanName.LastIndexOf(" (");
                if(bracketIndex > 0)
                {
                    cleanName = cleanName.Substring(0, bracketIndex);
                }


                int matchedIndex = setting.prefabList.FindIndex(p => p != null && p.name == cleanName);

                if(matchedIndex != -1)
                {
                    mapData.gimmicList.Add(new SGimmickSpawnData
                    {
                        gimmickType = setting.gimmickType,
                        position = child.transform.position,
                        prefabIndex = matchedIndex
                    });
                }
            }
        }
    }
}

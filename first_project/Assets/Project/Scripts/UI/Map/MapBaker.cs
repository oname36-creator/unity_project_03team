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
    public SOMapData MapData => mapData;

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
#if UNITY_EDITOR
                    // [교정] 이동하려는 오브젝트가 프리팹 인스턴스의 하위 요소인지 확인
                    if (PrefabUtility.IsPartOfPrefabInstance(child))
                    {
                        // 가장 최상위 프리팹 루트를 찾아서 완전히 언팩(Unpack) 처리
                        GameObject prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(child);
                        if (prefabRoot != null)
                        {
                            PrefabUtility.UnpackPrefabInstance(prefabRoot, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                        }
                    }
#endif
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

#if UNITY_EDITOR
    #region TentacleSpawnSpawnPosition
    /// <summary>
    /// 하이라키 내에 존재하는 모든 MapChunk를 검색하여
    /// 낭떠러지 좌표를 일괄 스캔하고 각각의 SOHollowTrapData 에셋에 저장합니다.
    /// </summary>
    [ContextMenu("모든 청크의 낭떠러지 일괄 베이킹")]
    public void BakeAllHollowPoints()
    {
        // 1. 하이라키 상의 모든 MapChunk 탐색
        MapChunk[] chunks = FindObjectsByType<MapChunk>();
        if (chunks == null || chunks.Length == 0)
        {
            Debug.LogWarning("[MapBaker] 하이라키에서 MapChunk를 찾을 수 없습니다.");
            return;
        }
        int totalBakedCount = 0;
        foreach (var chunk in chunks)
        {
            // 2. 각 청크에 연결된 컨트롤러 확인
            MapChunkSpawnController controller = chunk.GetComponent<MapChunkSpawnController>();
            if (controller == null)
            {
                Debug.LogWarning($"[MapBaker] {chunk.gameObject.name}에 MapChunkSpawnController가 없어 낭떠러지 스캔을 스킵합니다.");
                continue;
            }
            // 3. 컨트롤러에 할당된 낭떠러지 데이터 에셋 확인
            SOHollowTrapData hollowData = controller.HollowTrapData;
            if (hollowData == null)
            {
                Debug.LogWarning($"[MapBaker] {chunk.gameObject.name}의 MapChunkSpawnController에 SOHollowTrapData 에셋이 할당되어 있지 않아 스킵합니다.");
                continue;
            }
            // 시작/끝 앵커 예외 처리
            if (chunk.startPosition == null || chunk.endPosition == null)
            {
                Debug.LogWarning($"[MapBaker] {chunk.gameObject.name}의 시작/끝 앵커가 누락되어 스킵합니다.");
                continue;
            }
            // 타일맵 컴포넌트 탐색
            Tilemap tilemap = chunk.GetComponentInChildren<Tilemap>();
            if (tilemap == null)
            {
                Debug.LogWarning($"[MapBaker] {chunk.gameObject.name} 내부에서 Tilemap 컴포넌트를 찾지 못해 스킵합니다.");
                continue;
            }
            Collider2D[] groundColliders = tilemap.GetComponents<Collider2D>();

            // 4. 기존 낭떠러지 데이터 초기화
            hollowData.hollowTrapList.Clear();
            float startWorldX = chunk.startPosition.position.x;
            float endWorldX = chunk.endPosition.position.x;
            float startWorldY = chunk.startPosition.position.y;

            // 5. 낭떠러지(구멍) 구간 정밀 탐색 시작 (0.2m 간격)
            float step = 0.2f;
            float checkY = -10.0f; // 캐릭터 기준 기본 Y 레벨 오프셋
            float minGapWidth = 0.8f; // 촉수 함정이 작동될 최소 구멍 너비
            List<Vector3> detectedCliffs = new List<Vector3>();
            bool inGap = false;
            float lastGroundX = startWorldX;
            if (!CheckHasGroundAtX(startWorldX, startWorldY, groundColliders))
            {
                inGap = true;
            }
            for (float x = startWorldX; x <= endWorldX; x += step)
            {
                bool hasGround = CheckHasGroundAtX(x, startWorldY, groundColliders);
                if (hasGround)
                {
                    if (inGap)
                    {
                        float gapWidth = x - lastGroundX;
                        if (gapWidth >= minGapWidth)
                        {
                            float centerX = (lastGroundX + x) / 2f;
                            detectedCliffs.Add(new Vector3(centerX, startWorldY + checkY, 0f));
                        }
                        inGap = false;
                    }
                    lastGroundX = x;
                }
                else
                {
                    if (!inGap)
                    {
                        inGap = true;
                    }
                }
            }
            if (inGap)
            {
                float gapWidth = endWorldX - lastGroundX;
                if (gapWidth >= minGapWidth)
                {
                    float centerX = (lastGroundX + endWorldX) / 2f;
                    detectedCliffs.Add(new Vector3(centerX, startWorldY + checkY, 0f));
                }
            }

            // 6. 감지된 낭떠러지 좌표를 '청크 기준의 로컬 좌표'로 변환하여 에셋에 기록
            int bakedCount = 0;
            foreach (Vector3 worldSpawnPos in detectedCliffs)
            {
                Vector3 localSpawnPos = chunk.transform.InverseTransformPoint(worldSpawnPos);
                hollowData.hollowTrapList.Add(new SHoolowTrapSapwnData
                {
                    position = localSpawnPos,
                    // prefabIndex = 0
                });
                bakedCount++;
            }

            // 에셋 변경점 마킹 및 디스크 저장
            EditorUtility.SetDirty(hollowData);
            totalBakedCount += bakedCount;
            Debug.Log($"[MapBaker] {chunk.gameObject.name} 낭떠러지 스캔 완료: 총 {bakedCount}개의 좌표가 저장되었습니다.");
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"<color=cyan>[MapBaker] 하이라키 내 모든 맵 청크의 낭떠러지 일괄 베이킹 완료! (총 {totalBakedCount}개 감지)</color>");
    }
    #endregion

    #region Gizmos
    /// <summary>
    /// 특정 X 좌표의 수직 범위 내에 땅 콜라이더가 존재하는지 검사
    /// </summary>
    private bool CheckHasGroundAtX(float x, float startWorldY, Collider2D[] groundColliders)
    {
        for (int yOffset = -5; yOffset <= 2; ++yOffset)
        {
            Vector2 scanPoint = new Vector2(x, startWorldY + yOffset);
            foreach (var col in groundColliders)
            {
                if (col != null && col.OverlapPoint(scanPoint))
                {
                    return true;
                }
            }
        }
        return false;
    }
    #endregion
#endif

}

using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEngine.UIElements;




#if UNITY_EDITOR
using UnityEditor;
#endif

// 타일과 프리팹을 1:1 매칭시키는 구조체
[System.Serializable]
public struct TrapTileMapping
{
    public TileBase drawnTile;
    public GameObject trapPrefab;
}
public class MapBaker : MonoBehaviour
{
    [Header("데이터 및 에셋 세팅")]
    [SerializeField] private MapData mapDataSO;     // 저장할 SO 파일

    [Header("타일 <-> 프리팹 목록")]
    [SerializeField] private Transform tempTrapTilemap; // 실제로 생성할 가시

    [Header("씬 오브젝트 세팅")]
    [SerializeField] private Tilemap tempTilemap;   // 저장할 SO 파일
    [SerializeField] private Transform trapParent; // 실제로 생성할 가시

    // 인스펙터 창에서 우클릭하면 나타나는 창
    [ContextMenu("임시 타일을 함정 프리팹으로 변환")]
    public void BakeMapData()
    {
        // 예외 처리 (연결이 안 되어 있으면 중단)
        if(mapDataSO == null || tempTilemap == null || trapParent == null)
        {
            Debug.LogError("[MapBaker] 연결되지 않은 오브젝트가 있습니다!");
            return;
        }

        // 1. 초기화
        mapDataSO.trapList.Clear();
        int count = 0;



        // 기존에 생성되어 있던 프리팹 자식들 모두 삭제
        for (int i = trapParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(trapParent.GetChild(i).gameObject);
        }

        // 2. 타일이 있는 위치 찾아내기
        BoundsInt bounds = tempTilemap.cellBounds;
        
        
        for(int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);

                // 해당 칸에 어떤 타일이 그려져 있는지 구체적으로 가져옴
                TileBase currenTile = tempTilemap.GetTile(cellPos);

                if (currenTile != null) 
                {
                    // 칠해진 타일이 매핑 리스트에 등록된 타일인지 확인
                    foreach(var mapping in trapMappings)
                    {
                        if(mapping.drawnTile == currenTile)
                        {
                            Vector3 worldPos = tempTilemap.CellToWorld(cellPos) + tempTilemap.tileAnchor;

                            // A. Hierarchy의 Traps 밑에 가시 프리팹 생성 Quaternion.identity = Default
                            GameObject spawnedTrap = Instantiate(mapping.trapPrefab, worldPos, Quaternion.identity, trapParent);

                            // B. 스크립터블 오브젝트(SO) 파일에 이 좌표 저장
                            TrapSaveData newSaveData = new TrapSaveData
                            { 
                                    trapName = mapping.trapPrefab.name,
                                    position = worldPos
                            };
                            mapDataSO.trapList.Add(newSaveData);

                            count++;
                            break;      // 짝을 찾았으니 안쪽 반복문을 멈추고 다음 칸 체크


                        }
                    }
                }

                
            }
        }

        // 3. 임시로 그렸던 도화지 초기화
        tempTilemap.ClearAllTiles();

        //4. 에디터에 변경 사항을 알려서 데이터 유지되게 만듦(백그라운드 데이터 유지)
#if UNITY_EDITOR
        EditorUtility.SetDirty(mapDataSO);
        AssetDatabase.SaveAssets();     // 변경된 에셋 파일 디스크에 즉시 물리 저장
#endif
        Debug.Log($"<color=cyan>[MapBaker] 베이킹 완료!");
    }
}

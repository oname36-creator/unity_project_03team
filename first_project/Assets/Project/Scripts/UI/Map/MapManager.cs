using UnityEngine;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    [Header("맵 데이터")]
    public SOMapPalette currentMapData;

    [Header("세팅")]
    public Transform Player;
    public float spawnTriggerDistance = 30f;

    // 프리팹 종류별로 관리
    private Dictionary<GameObject, Queue<GameObject>> mapPools = new Dictionary<GameObject, Queue<GameObject>>();

    // 화면에 깔려있는 맵들
    private Queue<GameObject> activeMaps = new Queue<GameObject>();

    // 테트리스처럼 골고루 뽑기 위한 주머니
    private List<GameObject> shuffleBag = new List<GameObject>();

    private Vector3 nextSpawnPosition = Vector3.zero;
    void Start()
    {
        if (currentMapData == null || currentMapData.chunkPrefabs.Count == 0)
        {
            Debug.LogError("MapManager에 SO 데이터가 없거나 비어있다!");
            return;
        }

        // 1. 게임 시작 시 각 프리팹마다 풀 바구니 생성 및 셔블 백 초기화
        foreach (var prefab in currentMapData.chunkPrefabs)
        {
            mapPools.Add(prefab, new Queue<GameObject>());
        }

        // 2. 초기 맵 4개 세팅
        for (int i = 0; i < 4; ++i)
        {
            SpawnNextMap();
        }

    }

    private void SpawnNextMap()
    {
        // 1. 셔블 백에서 다음 스폰할 프리팹 원본 결정
        GameObject selectedPrefab = GetNextPrefabFromShuffleBag();
        GameObject mapToSpawn = null;

        // 2. 해당 프리팹 전용 풀에 남은 거 있는지 확인
        if (mapPools[selectedPrefab].Count > 0)
        {
            mapToSpawn = mapPools[selectedPrefab].Dequeue();
            mapToSpawn.SetActive(true);
        }
        else
        {
            // 바구니가 null이면 새로 생성
            mapToSpawn = Instantiate(selectedPrefab);

            // 돌아갈 풀 알기 위해 이름꼬리표 유지
            mapToSpawn.name = selectedPrefab.name;
        }

        // 3. 맵 배치 및 EndPosition 갱신
        mapToSpawn.transform.position = nextSpawnPosition;
        activeMaps.Enqueue(mapToSpawn);

        Transform endPos = mapToSpawn.transform.Find("EndPosition");
        if (endPos != null)
        {
            nextSpawnPosition = endPos.position;
        }
        else
        {
            Debug.LogError($"{mapToSpawn.name}에 앤드Position 마커가 없음!");
        }
    }

    private void RecycleOldMap()
    {
        // 화면 밖으로 나간 첫 번째 맵 수거
        GameObject oldMap = activeMaps.Dequeue();
        oldMap.SetActive(false);

        // 자신이 있던 pool의 위치 찾아서 다시 돌아감
        foreach (var prefab in currentMapData.chunkPrefabs)
        {
            if (oldMap.name == prefab.name)
            {
                mapPools[prefab].Enqueue(oldMap);
                break;
            }
        }
    }

    // 중복 없이 랜덤 추출
    private GameObject GetNextPrefabFromShuffleBag()
    {
        // pool에 값 없으면 다시 채워 넣음
        if(shuffleBag.Count ==0)
        {
            shuffleBag.AddRange(currentMapData.chunkPrefabs);
        }

        // 랜덤 뽑기
        int randomIndex = Random.Range(0, shuffleBag.Count);
        GameObject pickedPrefab = shuffleBag[randomIndex];

        // 추출됬으면 풀에서 제거
        shuffleBag.RemoveAt(randomIndex);

        return pickedPrefab;
    }
}
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.PlayerLoop;
using System.Collections;

// 페이즈별로 시간을 설정하기 위한 구조체
[System.Serializable]
public struct PhaseData
{
    public string phaseName;
    [Tooltip("한 페이즈가 끝나는 시간")]
    public float timeThreshold;
    [Tooltip("해당 페이즈에 등장할 맵들이 들어있는 SO")]
    public SOMapPalette phasePalette;
}
public class MapManager : MonoBehaviour
{
    [Header("시작 설정")]
    public GameObject safeZonePrefab;
    public Transform Player;
    public float spawnTriggerDistance = 30f;

    [Header("난이도 페이즈 설정")]
    public List<PhaseData> phases;
    private int currentPhaseIndex = 0;
    private float gameTimer = 0f;
   

    // 프리팹 종류별로 관리
    private Dictionary<GameObject, Queue<GameObject>> mapPools = new Dictionary<GameObject, Queue<GameObject>>();

    // 화면에 깔려있는 맵들
    private Queue<GameObject> activeMaps = new Queue<GameObject>();

    // 테트리스처럼 골고루 뽑기 위한 주머니
    private List<GameObject> shuffleBag = new List<GameObject>();

    private Vector3 nextSpawnPosition = Vector3.zero;
    void Start()
    {
        InitializationPools();

        // 시작 위치 0으로 고정
        Vector3 spawnPos = Vector3.zero;

        // 안전지대 생성
        SpawnSpecificMap(safeZonePrefab);

        UpdateShuffleBagForCurrentPhase();

        spawnPos = SpawnMap(GetNextPrefabFromShuffleBag(), spawnPos);

        nextSpawnPosition = spawnPos;
        // 1페이즈 맵들로 셔플 백 채우기
        StartCoroutine(PhaseTimerRoutine());
    }

    void Update()
    {
        if(Player.position.x + spawnTriggerDistance > nextSpawnPosition.x)
        {
            nextSpawnPosition = SpawnMap(GetNextPrefabFromShuffleBag(), nextSpawnPosition);
            RecycleOldMap();
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
        foreach (var prefab in phases[currentPhaseIndex].phasePalette.chunkPrefabs)
        {
            if (oldMap.name == prefab.name)
            {
                mapPools[prefab].Enqueue(oldMap);
                break;
            }
        }
    }

    private void InitializationPools()
    {
        // 안전지대 바구니 생성
        mapPools.Add(safeZonePrefab, new Queue<GameObject>());
        // 모든 페이즈 맵들을 미리 세팅
        foreach(var phase in phases)
        {
            foreach(var prefab in phase.phasePalette.chunkPrefabs)
            {
                // 중복 방지
                if(!mapPools.ContainsKey(prefab))
                {
                    mapPools.Add(prefab, new Queue<GameObject>());
                }
            }
        }
    }

    // 특정 맵 강제 스폰
    private void SpawnSpecificMap(GameObject prefab)
    {
        GameObject mapToSpawn;
        if (mapPools[prefab].Count > 0)
        {
            mapToSpawn = mapPools[prefab].Dequeue();
            mapToSpawn.SetActive(true);
        }
        else 
        {
            mapToSpawn = Instantiate(prefab);
            mapToSpawn.name = prefab.name;
        }

        mapToSpawn.transform.position = nextSpawnPosition;
        activeMaps.Enqueue(mapToSpawn);

        // Find 쓰지말랬는데
        Transform endPos = mapToSpawn.transform.Find("EndPosition");
        nextSpawnPosition = endPos != null ? endPos.position : nextSpawnPosition;   // 이것도 간단한 문법있지 않나?
    }

    private void UpdateShuffleBagForCurrentPhase()
    {
        // 이전 페이즈 맵 정리 후 현재 페이즈 맵으로 교체
        shuffleBag.Clear();
        shuffleBag.AddRange(phases[currentPhaseIndex].phasePalette.chunkPrefabs);
    }

    // 중복 없이 랜덤 추출
    private GameObject GetNextPrefabFromShuffleBag()
    {
        // pool에 값 없으면 다시 채워 넣음
        if(shuffleBag.Count ==0)
        {
            shuffleBag.AddRange(phases[currentPhaseIndex].phasePalette.chunkPrefabs);
        }

        // 랜덤 뽑기
        int randomIndex = Random.Range(0, shuffleBag.Count);
        GameObject pickedPrefab = shuffleBag[randomIndex];

        // 추출됬으면 풀에서 제거
        shuffleBag.RemoveAt(randomIndex);

        return pickedPrefab;
    }

    // 스폰하고 나서 다음 스폰 위치를 반환하는 함수
    private Vector3 SpawnMap(GameObject prefab, Vector3 spawnPos)
    {
        GameObject map = null;

        if(mapPools.ContainsKey(prefab) && mapPools[prefab].Count > 0)
        {
            map = mapPools[prefab].Dequeue();
            map.SetActive(true);
        }
        else
        {
            map = Instantiate(prefab);
            map.name = prefab.name;
        }

        map.transform.position = spawnPos;
        activeMaps.Enqueue(map);

        // EndPosition 찾기
        Transform endPos = map.transform.Find("EndPosition");
        if (endPos != null)
        {
            return endPos.position;
        }
        else
        {
            Debug.LogError($"{prefab.name}에 EndPosition이 없습니다");
            return spawnPos;
        }
    }

    private IEnumerator PhaseTimerRoutine()
    {
        for(currentPhaseIndex = 0; currentPhaseIndex < phases.Count; currentPhaseIndex++)
        {
            PhaseData currentPhase = phases[currentPhaseIndex];

            // 테스트용 디버깅
            Debug.Log($"[{currentPhase.phaseName}] 돌입!, 맵 세팅 변경됨");

            // 현재 페이즈의 맵들로 셔플 백 갈아끼우기
            UpdateShuffleBagForCurrentPhase();

            // 마지막 페이즈 -> 무한 대기(영원히 지속됨)
            if(currentPhaseIndex < phases.Count-1)
            {
                yield return new WaitForSeconds(currentPhase.timeThreshold);
            }
            else 
            {
                yield break;
            }
        }
    }
}
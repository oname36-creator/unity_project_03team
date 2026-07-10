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

    [Header("난이도 페이즈 설정")]
    public List<PhaseData> phases;
    private int currentPhaseIndex = 0;
    private float gameTimer = 0f;

    #region DataStruct Fields
    // 프리팹 종류별로 관리
    private Dictionary<GameObject, Queue<GameObject>> mapPools = new Dictionary<GameObject, Queue<GameObject>>();

    // 화면에 깔려있는 맵들
    private Queue<GameObject> activeMaps = new Queue<GameObject>();

    // 테트리스처럼 골고루 뽑기 위한 주머니
    private List<GameObject> shuffleBag = new List<GameObject>();

    private Vector3 nextSpawnPosition = Vector3.zero;
    #endregion

    private void OnEnable()
    {
        MapEvent.onPlayerHitSpawnTrigger += HandleMapSpawnEvent;
    }

    private void OnDisable()
    {
        MapEvent.onPlayerHitSpawnTrigger -= HandleMapSpawnEvent;
    }


    void Start()
    {
        InitializationPools();

        // 안전지대 생성
        SpawnSpecificMap(safeZonePrefab);

       // UpdateShuffleBagForCurrentPhase();
        
       // nextSpawnPosition = SpawnMap(GetNextPrefabFromShuffleBag(), nextSpawnPosition);
        // 1페이즈 맵들로 셔플 백 채우기
        StartCoroutine(CoPhaseTimerRoutine());
    }
    private void HandleMapSpawnEvent()
    {
        // 1. 다음 맵 스폰
        GameObject nextPrefab = GetNextPrefabFromShuffleBag();
        nextSpawnPosition = SpawnMap(nextPrefab, nextSpawnPosition);

        // 2. 맵 수거 로직(거리 계산X, 화면에 맵 3개 이상 깔려있으면 제일 뒤에것 자르기)
        if (activeMaps.Count > 3)
        {
            RecycleOldMap();
        }
    }
    private void RecycleOldMap()
    {
        if (activeMaps.Count == 0) return;

        // 화면 밖으로 나간 첫 번째 맵 수거
        GameObject oldMap = activeMaps.Dequeue();
        oldMap.SetActive(false);

        if(oldMap.name == safeZonePrefab.name)
        {
            mapPools[safeZonePrefab].Enqueue(oldMap);
            return;
        }
        foreach(var phase in phases)
        {
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
        GameObject mapToSpawn = GetOrCreateMap(prefab);
        Vector3 startOffset = Vector3.zero;
        if(mapToSpawn.TryGetComponent<MapChunk>(out MapChunk chunk) && chunk.startPosition != null)
        {
            startOffset = chunk.startPosition.localPosition;
        }

        mapToSpawn.transform.position = nextSpawnPosition - startOffset;
        activeMaps.Enqueue(mapToSpawn);

        // 보정되어 실제 배치된 값 기준으로 EndPosition을 누적 연산.
        nextSpawnPosition = GetEndPosition(mapToSpawn, mapToSpawn.transform.position);
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
        GameObject map = GetOrCreateMap(prefab);
        Vector3 startOffset = Vector3.zero;
        if (map.TryGetComponent<MapChunk>(out MapChunk chunk) && chunk.startPosition != null)
        {
            startOffset = chunk.startPosition.localPosition;
        }

        map.transform.position = nextSpawnPosition - startOffset;
        activeMaps.Enqueue(map);

        // 보정되어 실제 배치된 값 기준으로 EndPosition을 누적 연산.
        return GetEndPosition(map, map.transform.position);
    }

    private GameObject GetOrCreateMap(GameObject prefab)
    {
        GameObject map;
        if(mapPools.ContainsKey(prefab) && mapPools[prefab].Count > 0)
        {
            map = mapPools[prefab].Dequeue();
            map.SetActive(true);
        }
        else
        {
            map = Instantiate(prefab);
            map.name = prefab.name; // 클론 방지
        }
        return map;
    }

    // Find 리펙토링 함수
    private Vector3 GetEndPosition(GameObject map, Vector3 fallbackPos)
    {
        if (map.TryGetComponent<MapChunk>(out MapChunk chunk) && chunk.endPosition != null)
        {
            // return chunk.endPosition.position; <-- 글로벌 좌표 사용
            return fallbackPos + chunk.endPosition.localPosition;
        }

        Debug.LogError($"{map.name}에 EndPosition 앵커가 없습니다");
        return fallbackPos;
    }

    private IEnumerator CoPhaseTimerRoutine()
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
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
    // 0.5 단위 반올림(Snap) 유틸리티 함수
    private float SnapToHalfGrid(float value)
    {
        return Mathf.Round(value * 2f) / 2f;
    }

    #region DataAttribute
    [Header("시작 설정")]
    public GameObject safeZonePrefab;
    [Tooltip("첫 세이프존이 스폰될 시작 위치 (Y축 높이 조절용)")]
    public Vector3 initialSpawnPosition = Vector3.zero; // [추가] 초기 스폰 위치 변수
   
    [Header("배경 지연 설정")]
    public GameObject backgroundGroup;
    [Tooltip("안전지대 생성 후 배경이 나타날 때까지의 대기 시간")]
    [Range(0,3)]public float backgroundDelayTime = 3.0f;
    [Header("난이도 페이즈 설정")]
    public List<PhaseData> phases;
    private int currentPhaseIndex = 0;



    [Header("오브젝트 풀 설정")]
    [SerializeField] private int initialChunkPoolSize = 2;

    [SerializeField] private int currentLogicalPhase = -1;
    public int CurrentLogicalPhase => currentLogicalPhase;
    public static System.Action OnMapReady;
    public int CurrentPhaseIndex => currentPhaseIndex;
    private float gameTimer = 0f;

    // 싱글톤 인스턴스(실제 페이즈에 접근할 수 있도록 선언)
    public static MapManager Instance { get; private set; }
    #region DataStruct Fields
    // 프리팹 종류별로 관리
    private Dictionary<GameObject, Queue<GameObject>> mapPools = new Dictionary<GameObject, Queue<GameObject>>();

    // 화면에 깔려있는 맵들
    private Queue<GameObject> activeMaps = new Queue<GameObject>();

    // 테트리스처럼 골고루 뽑기 위한 주머니
    private List<GameObject> shuffleBag = new List<GameObject>();

    private Vector3 nextSpawnPosition = Vector3.zero;
    #endregion
    #endregion

    #region Event
    private void OnEnable()
    {
        MapEvent.onPlayerHitSpawnTrigger += HandleMapSpawnEvent;
    }

    private void OnDisable()
    {
        MapEvent.onPlayerHitSpawnTrigger -= HandleMapSpawnEvent;
    }
    #endregion

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region Start
    void Start()
    {
        InitializationPools();

        // [추가] 시작 좌표를 인스펙터에서 설정한 initialSpawnPosition 값으로 지정
        nextSpawnPosition = initialSpawnPosition;

        // 안전지대 생성
        SpawnSpecificMap(safeZonePrefab);

        // 시작하자마자 배경을 끄고 카운터 코루틴 실행
        if(backgroundGroup != null)
        {
            backgroundGroup.SetActive(false);
            StartCoroutine(CoEnableBackgroundRoutine());
        }

        // 1페이즈 맵들로 셔플 백 채우기
        StartCoroutine(CoPhaseTimerRoutine());

        CameraConfinerManager.Instance?.InitizlizeYTracking();
    }


    #endregion

    #region Update
    private void Update()
    {
        gameTimer += Time.deltaTime;

        // 실제 시간에 맞춰 논리적 페이즈 구분
        int newLogicalPhase = 0;
        if(gameTimer < 60f)
        {
            newLogicalPhase = 0;
        }
        else if(gameTimer < 120f)
        {
            newLogicalPhase = 1;
        }
        else
        {
            newLogicalPhase = 2;
        }

        // 3. 페이즈가 변경된 '최초 1회'만 연출 및 설정 업데이트
        if(newLogicalPhase != currentLogicalPhase)
        {
            currentLogicalPhase = newLogicalPhase;
            Debug.Log("[MapManager] : 논리적 페이즈 변경됨");
            MapEvent.onRequestPhase?.Invoke(currentLogicalPhase + 1);
        }
    }
    #endregion

    #region HandleMapSpawnEvent
    private void HandleMapSpawnEvent()
    {
        Debug.Log("맵 생성됨");
        // 1. 다음 맵 스폰
        GameObject nextPrefab = GetNextPrefabFromShuffleBag();
        nextSpawnPosition = SpawnMap(nextPrefab, nextSpawnPosition);

        // 2. 맵 수거 로직(거리 계산X, 화면에 맵 3개 이상 깔려있으면 제일 뒤에것 자르기)
        if (activeMaps.Count > 2)
        {
            RecycleOldMap();
        }
    }
    #endregion

    #region RecycleOldMap
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

        // 전체 풀의 Key를 순회하며 이름이 같은 풀을 찾아 반환
        foreach(var prefabKey in mapPools.Keys)
        {
            if(oldMap.name == prefabKey.name)
            {
                mapPools[prefabKey].Enqueue(oldMap);
                return;
            }
        }

        // 안전장치
        Debug.LogWarning($"[MapManager] {oldMap.name}에 해당하는 오브젝트 풀을 찾지 못해 파괴합니다.");
        Destroy(oldMap);
       
    }
    #endregion

    #region InitPool
    private void InitializationPools()
    {

        // 안전지대 바구니 생성
        CreateAndEnqueuePool(safeZonePrefab, initialChunkPoolSize);
        // 모든 페이즈 맵들을 미리 세팅
        foreach(var phase in phases)
        {
            if (phase.phasePalette == null || phase.phasePalette.chunkPrefabs == null) continue;

            foreach(var prefab in phase.phasePalette.chunkPrefabs)
            {
                if (prefab == null) return;
                // 중복 방지
                if(!mapPools.ContainsKey(prefab))
                {
                    CreateAndEnqueuePool(prefab, initialChunkPoolSize);
                }
            }
        }
    }
    #endregion

    private void CreateAndEnqueuePool(GameObject prefab, int count)
    {
        if (!mapPools.ContainsKey(prefab))
        {
            mapPools.Add(prefab, new Queue<GameObject>());
        }

        for (int i = 0; i < count; i++)
        {
            GameObject mapObj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            mapObj.name = prefab.name;
            mapObj.SetActive(false);
            mapPools[prefab].Enqueue(mapObj);
        }
    }

    #region Spawn & GetCreate
    // 특정 맵 강제 스폰
    private void SpawnSpecificMap(GameObject prefab)
    {
        Vector3 startOffset = Vector3.zero;
        if(prefab.TryGetComponent<MapChunk>(out MapChunk chunk) && chunk.startPosition != null)
        {
            startOffset = chunk.startPosition.localPosition;
        }
        Vector3 targetPosition = nextSpawnPosition - startOffset;

        // X축 1.0f 타일 단위 Snap 및 Y축 위치 완전 맞춤
        targetPosition.x = SnapToHalfGrid(targetPosition.x);
        targetPosition.y = SnapToHalfGrid(nextSpawnPosition.y - startOffset.y);

        GameObject mapToSpawn = GetOrCreateMap(prefab, targetPosition);
        activeMaps.Enqueue(mapToSpawn);

        // 보정되어 실제 배치된 값 기준으로 EndPosition을 누적 연산.
        nextSpawnPosition = GetEndPosition(mapToSpawn, targetPosition);
    }
    // 스폰하고 나서 다음 스폰 위치를 반환하는 함수
    private Vector3 SpawnMap(GameObject prefab, Vector3 spawnPos)
    {
        Vector3 startOffset = Vector3.zero;
        if (prefab.TryGetComponent<MapChunk>(out MapChunk chunk) && chunk.startPosition != null)
        {
            startOffset = chunk.startPosition.localPosition;
        }
        Vector3 targetPosition = nextSpawnPosition - startOffset;

        // X축 1.0f 타일 단위 Snap 및 Y축 위치 완전 맞춤
        targetPosition.x = SnapToHalfGrid(targetPosition.x);
        targetPosition.y = SnapToHalfGrid(nextSpawnPosition.y - startOffset.y);

        GameObject map = GetOrCreateMap(prefab, targetPosition);
        activeMaps.Enqueue(map);

        // 보정되어 실제 배치된 값 기준으로 EndPosition을 누적 연산.
        return GetEndPosition(map, map.transform.position);
    }

    private GameObject GetOrCreateMap(GameObject prefab, Vector3 targetPosition)
    {
        GameObject map;
        if (mapPools.ContainsKey(prefab) && mapPools[prefab].Count > 0)
        {
            map = mapPools[prefab].Dequeue();
            // 활성화 전에 목표 위치로 먼저 이동
            map.transform.position = targetPosition;
            map.SetActive(true);
        }
        else
        {
            // 인스턴스화할 때도 처음부터 목표 위치로 생성 --> OnEnable 시점의 위치문제 방지
            map = Instantiate(prefab, targetPosition, Quaternion.identity);
            map.name = prefab.name; // 클론 방지
        }
        return map;
    }
    #endregion

    #region SuffleBag
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
    #endregion

    #region Find
    // Find 리펙토링 함수
    private Vector3 GetEndPosition(GameObject map, Vector3 fallbackPos)
    {
        if (map.TryGetComponent<MapChunk>(out MapChunk chunk) && chunk.endPosition != null)
        {
            // return chunk.endPosition.position; <-- 글로벌 좌표 사용
            Vector3 endPos = fallbackPos + chunk.endPosition.localPosition;

            endPos.x = SnapToHalfGrid(endPos.x);
            // endPos.y = SnapToHalfGrid(endPos.y);
            return endPos;
        }

        Debug.LogError($"{map.name}에 EndPosition 앵커가 없습니다");
        return fallbackPos;
    }
    #endregion

    #region CoRoutine
    private IEnumerator CoPhaseTimerRoutine()
    {
        // 누적 시간을 트래킹하기 위한 로컬 변수
        float cumulativeTime = 0f;

        for(currentPhaseIndex = 0; currentPhaseIndex < phases.Count; currentPhaseIndex++)
        {
            PhaseData currentPhase = phases[currentPhaseIndex];

            // 테스트용 디버깅
           // Debug.Log($"[{currentPhase.phaseName}] 돌입! " +
            //    $"누적 시작 시간: {cumulativeTime}초, 논리적 페이즈: {currentLogicalPhase}");

            // 현재 페이즈의 맵들로 셔플 백 갈아끼우기
            UpdateShuffleBagForCurrentPhase();

            // 마지막 세부 페이즈인 경우 더 기다리지 않고 종료
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

    private IEnumerator CoEnableBackgroundRoutine()
    {
        // 설정한 딜레이 시간만큼 대기
        yield return new WaitForSeconds(backgroundDelayTime);

        if(backgroundGroup != null)
        {
            backgroundGroup.SetActive(true);
        }

        // 배경 세팅까지 모두 끝났음을 이벤트를 통해 알림
        OnMapReady?.Invoke();
    }
    #endregion
}
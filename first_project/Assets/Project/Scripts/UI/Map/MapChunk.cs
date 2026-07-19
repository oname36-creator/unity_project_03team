using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor.Rendering;

public class MapChunk : MonoBehaviour
{
    #region DataAttribute
    [Header("시작/끝 앵커")]
    public Transform startPosition;
    public Transform endPosition;

    [Header("카메라 제한 영역")]
    public Collider2D cameraBoundaryCollider;

    [Header("몬스터 자동 스폰 설정")]
    [SerializeField] private SOMonsterSpawnSetting spawnSetting;

    [Header("상자 자동 스폰 설정")]
    [SerializeField] private SOBoxSpawnSetting boxSpawnSetting;
    
    private TentacleTrapSpawner trapSpawner;

    [Header("플레이어 안전 스폰 거리")]
    [SerializeField] private float minPlayerDistance = 4f;


    [Header("진입 영역 마진")]
    public float margin = 2f;

    // 해당 청크에서 스폰되어 추적 중인 몬스터 목록
    private List<GameObject> spawnedMonsters = new List<GameObject>();

    private List<GameObject> spawnedBoxes = new List<GameObject>();
    #endregion

    private void Awake()
    {
        trapSpawner = GetComponentInChildren<TentacleTrapSpawner>();
    }

#region Event
    private void OnEnable()
    {
        // 맵 청크 활성화 시 스폰 처리
        SpawnMonstersInChunk();

        if (trapSpawner != null)
        {
            trapSpawner.SpawnTraps();
        }
    }
    private void OnDisable()
    {
        // 맵 청크 비활성화 시 스폰한 몬스터 일괄 풀 반환
        RecycleMonsters();

        RecycleBoxes();
        
        if(trapSpawner != null)
        {
            trapSpawner.RecycleTraps();
        }
    }


    #endregion

#region InChunk

    #region Monsters
    private void SpawnMonstersInChunk()
    {
        // 예외 처리 : SO가 지정되지 않을 시 스폰 생략
        if(spawnSetting == null)
        {
            Debug.LogWarning($"[MapChunk] {gameObject.name}에 SOMonsterSpawnSetting이 할당되지 않아 몬스터 자동 스폰을 진행하지 않습니다.");
            return;
        }

        // 1. 이름이 Trap인 부모 오브젝트를 찾아 하위 콜라이더들을 자동 수집
        List<Collider2D> prohibitedColliders = new List<Collider2D>();
        foreach(Transform child in transform)
        {

            if(child.name == "Trap")
            {
                // 해당 부모 하위의 모든 Collider2D를 수집해 리스트에 보관
                Collider2D[] colliders = child.GetComponentsInChildren<Collider2D>(true);
                prohibitedColliders.AddRange(colliders);
            }
        }

        // 1-2. SpikeTrap 컴포넌트가 부착된 하위 오브젝트의 모든 Collider2D도 수집함
        SpikeTrap[] spikeTraps = GetComponentsInChildren<SpikeTrap>(true);
        foreach(var spike in spikeTraps)
        {
            Collider2D[] spikeColliders = spike.GetComponentsInChildren<Collider2D>(true);
            prohibitedColliders.AddRange(spikeColliders);
        }

        // Overlap 판정 작동하도록 강제 동기화
        Physics2D.SyncTransforms();

       //Debug.Log($"[MapChunk] 수집된 금지 구역 콜라이더 개수 : {prohibitedColliders.Count}개");

        // 2. Ground 역할을 할 Tilemap 탐색
        UnityEngine.Tilemaps.Tilemap tilemap = null;
        foreach(Transform child in transform)
        {
            if(child.name == spawnSetting.groundTilemapName)
            {
                tilemap = child.GetComponent<UnityEngine.Tilemaps.Tilemap>();
                if (tilemap != null) break;
            }
        }

        // 예비용으로 첫 번째 발견되는 Tilemap을 탐색
        if(tilemap == null)
        {
            tilemap = GetComponentInChildren<UnityEngine.Tilemaps.Tilemap>();
        }

        if(tilemap == null)
        {
            Debug.LogWarning($"[MapChunk] {gameObject.name}에 타일맵이 없어 몬스터 자동 스폰을 건너뜁니다.");
            return;
        }

        // 3. 스폰 가능한 바닥 셀 후보군 탐색
        BoundsInt bounds = tilemap.cellBounds;
        List<Vector3> validSpawnPositions = new List<Vector3>();

        // startPos 기준으로 스폰 금지 영역 설정
        Vector3 startAnchorPos = startPosition != null ? startPosition.position : transform.position;
        MapSpawnTrigger spawnTrigger = GetComponentInChildren<MapSpawnTrigger>();
        Collider2D triggerCollider = spawnTrigger != null ? spawnTrigger.GetComponent<Collider2D>() : null;
        
        // Debug.Log($"[MapChunk Debug] StartAnchor: {startAnchorPos}, isStartPositionNull: {startPosition == null}");

        int entryExcludingCount = 0;

        for(int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for(int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                // 현재 셀에 바닥 타일이 있고, 그 바로 윗칸은 비어있는지 확인
                if(tilemap.HasTile(pos) && !tilemap.HasTile(pos + Vector3Int.up))
                {
                    // 타일 셀의 월드 좌표 중심점 계산
                    Vector3 CellPos = tilemap.CellToWorld(pos);
                    //index=0 : 실제 스폰 위치    index = 1 : 오버랩 정밀 검사를 위한 임시 좌표
                    Vector3[] worldPos = { CellPos + new Vector3(0.5f, 1f, 0f), CellPos + new Vector3(0.5f, 0.2f, 0f) }; ;

                    // 플레이어가 진입할 입구 2.5f 범위 이내의 타일은 스폰에서 제외
                    if(triggerCollider != null)
                    {
                        Bounds triggerBounds = triggerCollider.bounds;
                        float minX = triggerBounds.min.x - margin;
                        float maxX = triggerBounds.max.x + margin;
                        float minY = triggerBounds.min.y - margin;
                        float maxY = triggerBounds.max.y + margin;

                        if (worldPos[0].x >= minX && worldPos[0].x <= maxX &&
                            worldPos[0].y >= minY && worldPos[0].y <= maxY)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        // Fallback: 트리거가 없을 시 startPosition 기준 2.5f 범위 제외
                        Vector2 startAnchor2D = new Vector2(startAnchorPos.x, startAnchorPos.y);
                        Vector2 worldPos2D = new Vector2(worldPos[0].x, worldPos[0].y);
                        if (Vector2.Distance(startAnchor2D, worldPos2D) < 2.5f)
                        {
                            continue;
                        }
                    }
                    // 금지 구역 콜라이더 바운즈 내부에 포함되는지 체크
                    bool isProhibited = false;
                    foreach (var col in prohibitedColliders)
                    {
                        if (col != null && col.OverlapPoint(worldPos[1]))
                        {
                            isProhibited = true;
                            Debug.Log($"[MapChunk] 함정 감지되어 제외됨 위치: {worldPos}, 함정 오브젝트: {col.gameObject.name}");
                            break;
                        }
                    }

                    // 금지 구역(함정/특정 플랫폼 위)에 걸쳐있다면 스폰하지 않고 패스
                    if (isProhibited)
                    {
                        continue;
                    }

                    // 금지 구역이 아닌 경우 스폰 후보지 리스트에 추가
                    validSpawnPositions.Add(worldPos[0]);
                }
            }
        }

        Debug.Log($"[MapChunk Debug] 입구제외: {entryExcludingCount}개");
        Debug.Log($"[MapChunk Debug] 최종 유효 스폰 후보지 수: {validSpawnPositions.Count}개");

        if (validSpawnPositions.Count == 0) return;

        // 4. 스폰 지점 셔플
        for(int i=0; i < validSpawnPositions.Count; i++)
        {
            Vector3 temp = validSpawnPositions[i];
            int randomIndex = UnityEngine.Random.Range(i, validSpawnPositions.Count);
            validSpawnPositions[i] = validSpawnPositions[randomIndex];
            validSpawnPositions[randomIndex] = temp;
        }

        // 5. 조건에 맞춰 실제 몬스터 스폰 요청
        int spawnedCount = 0;
        List<Vector3> actualSpawnedPositions = new List<Vector3>();



        // 현재 페이즈에 맞는 확률과 딜레이 값 선택
        int currentPhase = MapManager.Instance?.CurrentLogicalPhase ?? 0;
        currentPhase = Mathf.Clamp(currentPhase, 0, 2);

        // 페이즈별 설정 구조체 데이터 가져오기
        SPhaseMonsterSpawnData activeSetting = spawnSetting.phaseSettings[Mathf.Clamp(currentPhase, 0, spawnSetting.phaseSettings.Length - 1)];
        float MonsterSpawnChance = activeSetting.spawnChane;
        int maxMonsterCount = activeSetting.maxMonsterCount;
        MonsterType[] allowedTypes = activeSetting.spawnableMonsterTypes;

        foreach (Vector3 candidatePos in validSpawnPositions)
        {
            if (spawnedCount >= maxMonsterCount) continue;

            // 스폰 확률 검사
            if (UnityEngine.Random.value > maxMonsterCount) continue;

            // 최소 거리 검사
            bool tooClose = false;
            foreach(Vector3 spawnedPos in actualSpawnedPositions)
            {
                if(Vector3.Distance(spawnedPos, candidatePos) < spawnSetting.minSpawnInterval)
                {
                    tooClose = true;
                    break;
                }
            }
            if (tooClose) continue;

            // 스폰 타입 목록 예외처리
            if (allowedTypes == null || allowedTypes.Length == 0)
                break;

            // 지정된 후보 타입 중 랜덤 선택
            MonsterType selectedType = allowedTypes[UnityEngine.Random.Range(0, allowedTypes.Length)];
            string monsterName = selectedType.ToString();

            // 공중 몬스터의 경우 Y축에 높이 오프셋 추가
            Vector3 spawnPos = candidatePos;
            if(selectedType == MonsterType.Bird)
            {
                spawnPos.y += spawnSetting.bridHeightOffset;
            }

            // MapEvent 호출하여 MonsterRespawn에 생성되도록 요청
            MapEvent.onRequestMonsterSpawn?.Invoke(monsterName, spawnPos, (monster) =>
            {
                if (monster != null)
                {
                    spawnedMonsters.Add(monster);

                    // 스폰 성공 시 생성 수 증가 및 실제 스폰 좌표 기록
                    spawnedCount++;
                    actualSpawnedPositions.Add(spawnPos);
                }
            });
        }

        SpawnBoxesInChunk(validSpawnPositions, actualSpawnedPositions);
    }
    #endregion

    #region Box
    private void SpawnBoxesInChunk(List<Vector3> candidatePositions, List<Vector3> spawnedMonsterPostion)
    {
        #region Exception handling
        if(boxSpawnSetting == null)
        {
            Debug.LogError("SOBoxSpawnSetting이 할당안됨");
        }

        if (candidatePositions == null || candidatePositions.Count == 0) return;
        #endregion

        int spawnedBoxCount = 0;
        List<Vector3> actualSpawnedBoxPositions = new List<Vector3>();

        // 현재 페이즈 인덱스 조회 및 페이즈 범위 제한
        int currentPhase = MapManager.Instance?.CurrentLogicalPhase ?? 0;
        currentPhase = Mathf.Clamp(currentPhase, 0, 2);

        // 현재 페이즈에 맞는 확률과 딜레이 값 선택
        float boxSpawnChane = boxSpawnSetting.spawnChanes[Mathf.Clamp(currentPhase, 0, boxSpawnSetting.spawnChanes.Length - 1)];
        float maxBoxCount = boxSpawnSetting.maxBoxCounts[Mathf.Clamp(currentPhase, 0, boxSpawnSetting.spawnChanes.Length - 1)];

        foreach (Vector3 candidatePos in candidatePositions)
        {
            if (spawnedBoxCount >= maxBoxCount) break;

            // 1. 이미 몬스터가 스폰된 위치와의 간격 검사 => 중복 검사
            bool isOverlapWithMonser = false;
            foreach(Vector3 monsterPos in spawnedMonsterPostion)
            {
                if(Vector3.Distance(monsterPos, candidatePos) < boxSpawnSetting.minSpawnInterval)
                {
                    isOverlapWithMonser = true;
                    break;
                }    
            }
            if (isOverlapWithMonser) continue;

            // 2. 이미 스폰된 다른 상자와의 간격 검사
            bool toCloseToBox = false;
            foreach(Vector3 boxPos in actualSpawnedBoxPositions)
            {
                if(Vector3.Distance(boxPos, candidatePos) < boxSpawnSetting.minSpawnInterval)
                {
                    toCloseToBox = true;
                    break;
                }
            }
            if (toCloseToBox) continue;

            // 3. 상자 스폰 이벤트 호출
            if (UnityEngine.Random.value > boxSpawnChane) continue;

            // 4. 상자 스폰 이벤트 호출
            Vector3 spawnPos = candidatePos;    // 상자는 바닥에 스폰되므로 후보 위치 그대로 사용
            MapEvent.onRequestBoxSpawn?.Invoke(spawnPos, boxSpawnSetting, (box) =>
            {
                if (box != null)
                {
                    spawnedBoxes.Add(box);
                    spawnedBoxCount++;
                    actualSpawnedBoxPositions.Add(spawnPos);
                }
            });
        }
    }
    #endregion
    #endregion

#region Recycle

    #region Monsters
    private void RecycleMonsters()
    {
        foreach(var monster in spawnedMonsters)
        {
            // 몬스터가 씬에 파괴되지 않고 아직 활성화된 상태라면 안전하게 풀에 푸시
            if(monster != null && monster.activeSelf)
            {
                ObjectPoolManager.Instance.MonsterPush(monster);
            }
        }

        spawnedMonsters.Clear();
    }
    #endregion

    #region Boxes
    private void RecycleBoxes()
    {
        foreach(var box in spawnedBoxes)
        {
            if(box != null && box.activeSelf)
            {
                ObjectPoolManager.Instance.BoxPush(box);
            }
        }
        spawnedBoxes.Clear();
    }
    #endregion

#endregion

}

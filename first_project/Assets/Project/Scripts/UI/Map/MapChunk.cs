using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor.Rendering;

public class MapChunk : MonoBehaviour
{
    [Header("시작/끝 앵커")]
    public Transform startPosition;
    public Transform endPosition;

    [Header("카메라 제한 영역")]
    public Collider2D cameraBoundaryCollider;

    [Header("몬스터 자동 스폰 설정")]
    [SerializeField] private SOMonsterSpawnSetting spawnSetting;

    // 해당 청크에서 스폰되어 추적 중인 몬스터 목록
    private List<GameObject> spawnedMonsters = new List<GameObject>();

    #region Event
    private void OnEnable()
    {
        // 맵 청크 활성화 시 스폰 처리
        SpawnMonstersInChunk();
    }
    private void OnDisable()
    {
        // 맵 청크 비활성화 시 스폰한 몬스터 일괄 풀 반환
        RecycleMonsters();
    }
    #endregion

    #region SpawnMonstersInChunk
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

        for(int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for(int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                // 현재 셀에 바닥 타일이 있고, 그 바로 윗칸은 비어있는지 확인
                if(tilemap.HasTile(pos))
                {
                    // 타일 셀의 월드 좌표 중심점 계산
                    Vector3 worldPos = tilemap.CellToWorld(pos); + new Vector3(0.5f, 1.0f, 0f);

                    // 금지 구역 콜라이더 바운즈 내부에 포함되는지 체크
                    bool isProhibited = false;
                    foreach (var col in prohibitedColliders)
                    {
                        if (col != null && col.bounds.Contains(spawnPoint.transform.position))
                        {
                            isProhibited = true;
                            break;
                        }
                    }

                    // 금지 구역(함정/특정 플랫폼 위)에 걸쳐있다면 스폰하지 않고 패스
                    if (isProhibited)
                    {
                        continue;
                    }
                }
            }
        }

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

        foreach(Vector3 candidatePos in validSpawnPositions)
        {
            if (spawnedCount >= spawnSetting.spawnChance) continue;

            // 스폰 확률 검사
            if (UnityEngine.Random.value > spawnSetting.spawnChance) return;

            // 최소 거리 검사
            bool tooClose = false;
            foreach(Vector3 spawnedPos in actualSpawnedPositions)
            {
                if(Vector3.Distance(spawnedPos, candidatePos) < spawnSetting.minSpawnInterval)
                {
                    tooClose = true;
                    return;
                }
            }
            if (tooClose) return;

            // 스폰 타입 목록 예외처리
            if (spawnSetting.spawnableMonsterTypes == null || spawnSetting.spawnableMonsterTypes.Length == 0)
                break;

            // 지정된 후보 타입 중 랜덤 선택
            MonsterType selectedType = spawnSetting.spawnableMonsterTypes[UnityEngine.Random.Range(0, spawnSetting.spawnableMonsterTypes.Length)];
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
                }
            });
        }
    }
    #endregion

    #region RecycleMonsters
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
}

using System.Collections.Generic;
using UnityEngine;

class SCliffTrapNode
{
    public Vector3 worldPosition;
    public bool isEvaluted;
    public bool isTriggered;
}
public class MapChunkSpawnController : MonoBehaviour
{
    [Header("스폰 설정 에셋")]
    [SerializeField] private SOMapPhaseSpawnSetting spawnSetting;

    [Header("촉수 함정(낭떠러지) 데이터 에셋")]
    [SerializeField] private SOHollowTrapData hollowTrapData;
    public SOHollowTrapData HollowTrapData => hollowTrapData;

    [Header("공통 설정")]
    [SerializeField] private string groundTilemapName = "Tilemap";
    [SerializeField] private int minSpawnInterval = 3;
    [SerializeField] private float birdHeightOffset = 3.5f;
    [SerializeField] private float cameraMargin = 1.0f;

    [SerializeField] private float minSpawnStartXOffset = 5.0f;

    private MapChunk _chunk;

    private List<GameObject> _spawnedMonsters = new List<GameObject>();
    private List<GameObject> _spawnedBoxes = new List<GameObject>();
    private List<GameObject> _spawnedTraps = new List<GameObject>();
    private List<SCliffTrapNode> _activeCliffNodes = new List<SCliffTrapNode>();
    private float _nextTrapSpawnTime = 0;
    private bool[] _hasTrap;

    private void Awake()
    {
        _chunk = GetComponent<MapChunk>();
    }

    private void Update()
    {
        if (_activeCliffNodes.Count == 0 || Camera.main == null) return;

        if (spawnSetting == null) return;
        float trapSpawnChance = spawnSetting.tentacleTrapSpawnChane;
        float staggerDelay = spawnSetting.tentacleStaggerDelay;

        // 카메라 영역 체크
        float camHalfWidth = Camera.main.orthographicSize + Camera.main.aspect;
        float camRightBound = Camera.main.transform.position.x + camHalfWidth + cameraMargin;

        for (int i = 0; i < _activeCliffNodes.Count; i++)
        {
            var node = _activeCliffNodes[i];
            if (node.isEvaluted) continue;
            if (node.worldPosition.x <= camRightBound && !_hasTrap[i])
            {
                node.isEvaluted = true;
                if (Random.value <= trapSpawnChance)
                {
                    node.isTriggered = true;
                    float scheduledTime = Mathf.Max(Time.time, _nextTrapSpawnTime);
                    float delay = scheduledTime - Time.time;
                    _hasTrap[i] = true;
                    StartCoroutine(SpawnTrapWithDelay(node.worldPosition, hollowTrapData.isTrapDirectionUp, delay));
                    _nextTrapSpawnTime = scheduledTime + staggerDelay;
                }
                else
                {
                    Debug.Log($"[MapChunkSpawnController] 낭떠러지 {node.worldPosition} 함정 발동 실패 (확률 탈락)");
                }
            }
        }

    }

    public void SpawnAll()
    {
        Physics2D.SyncTransforms();
        if (_chunk == null)
        {
            _chunk = GetComponent<MapChunk>();
        }

        if (spawnSetting == null)
        {
            Debug.LogWarning($"[MapChunkSpawnController] {gameObject.name}의 스폰 설정이 존재하지 않습니다.");
            return;
        }
        // 1. 금지 구역 콜라이더 수집
        List<Collider2D> prohibitedColliders = new List<Collider2D>();
        foreach (Transform child in transform)
        {
            if (child.name == "Trap")
            {
                prohibitedColliders.AddRange(child.GetComponentsInChildren<Collider2D>(true));
            }

            if(child.name.Contains("NoSpawnZone"))
            {
                prohibitedColliders.AddRange(child.GetComponentsInChildren<Collider2D>(true));
            }
        }
        SpikeTrap[] spikeTraps = GetComponentsInChildren<SpikeTrap>(true);
        foreach (var spike in spikeTraps)
        {
            prohibitedColliders.AddRange(spike.GetComponentsInChildren<Collider2D>(true));
        }
        // 2. 바닥 타일맵 탐색
        UnityEngine.Tilemaps.Tilemap tilemap = null;
        foreach (Transform child in transform)
        {
            if (child.name == groundTilemapName)
            {
                tilemap = child.GetComponent<UnityEngine.Tilemaps.Tilemap>();
                if (tilemap != null) break;
            }
        }
        if (tilemap == null) tilemap = GetComponentInChildren<UnityEngine.Tilemaps.Tilemap>();
        if (tilemap == null)
        {
            Debug.LogWarning($"[MapChunkSpawnController] {gameObject.name}에 타일맵이 없어 스폰을 건너뜁니다.");
            return;
        }
        // 3. 스폰 가능 후보 셀 수집
        BoundsInt bounds = tilemap.cellBounds;
        List<Vector3> validPositions = new List<Vector3>();
        Vector3 startAnchorPos = _chunk.startPosition != null ? _chunk.startPosition.position : transform.position;
        MapSpawnTrigger spawnTrigger = GetComponentInChildren<MapSpawnTrigger>();
        Collider2D triggerCollider = spawnTrigger != null ? spawnTrigger.GetComponent<Collider2D>() : null;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (tilemap.HasTile(pos) && !tilemap.HasTile(pos + Vector3Int.up))
                {
                    Vector3 CellPos = tilemap.CellToWorld(pos);
                    Vector3[] worldPos = { CellPos + new Vector3(0.5f, 1f, 0f), CellPos + new Vector3(0.5f, 0.2f, 0f) };
                    if (triggerCollider != null)
                    {
                        Bounds triggerBounds = triggerCollider.bounds;
                        float margin = _chunk.margin;
                        if (worldPos[0].x >= triggerBounds.min.x - margin && worldPos[0].x <= triggerBounds.max.x + margin &&
                            worldPos[0].y >= triggerBounds.min.y - margin && worldPos[0].y <= triggerBounds.max.y + margin)
                        {
                            continue;
                        }
                    }
                    else if (Vector2.Distance(startAnchorPos, worldPos[0]) < 2.5f)
                    {
                        continue;
                    }
                    bool isProhibited = false;
                    foreach (var col in prohibitedColliders)
                    {
                        if (col != null && col.OverlapPoint(worldPos[1]))
                        {
                            isProhibited = true;
                            break;
                        }
                    }
                    if (isProhibited) continue;
                    validPositions.Add(worldPos[0]);
                }
            }
        }
        // 4. 스폰 지점 셔플
        for (int i = 0; i < validPositions.Count; i++)
        {
            Vector3 temp = validPositions[i];
            int randomIndex = Random.Range(i, validPositions.Count);
            validPositions[i] = validPositions[randomIndex];
            validPositions[randomIndex] = temp;
        }
        // 5. 몬스터 스폰 실행
        int spawnedMonsterCount = 0;
        List<Vector3> actualSpawnedMonsterPositions = new List<Vector3>();
        if (spawnSetting.spawnableMonsterTypes != null && spawnSetting.spawnableMonsterTypes.Length > 0)
        {
            foreach (Vector3 candidatePos in validPositions)
            {
                if(candidatePos.x - startAnchorPos.x < minSpawnStartXOffset)
                {
                    continue;
                }

                if (spawnedMonsterCount >= spawnSetting.maxMonsterCount) break;
                if (Random.value > spawnSetting.monsterSpawnChane) continue;
                bool tooClose = false;
                foreach (Vector3 spawnedPos in actualSpawnedMonsterPositions)
                {
                    if (Vector3.Distance(spawnedPos, candidatePos) < minSpawnInterval)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose) continue;
                MonsterType selectedType = spawnSetting.spawnableMonsterTypes[Random.Range(0, spawnSetting.spawnableMonsterTypes.Length)];
                Vector3 spawnPos = candidatePos;
                if (selectedType == MonsterType.Bird)
                {
                    spawnPos.y += birdHeightOffset;
                }
                MapEvent.onRequestMonsterSpawn?.Invoke(selectedType.ToString(), spawnPos, (monster) =>
                {
                    if (monster != null)
                    {
                        _spawnedMonsters.Add(monster);
                        spawnedMonsterCount++;
                        actualSpawnedMonsterPositions.Add(spawnPos);
                    }
                });
            }
        }
        // 6. 상자 스폰 실행
        int spawnedBoxCount = 0;
        List<Vector3> actualSpawnedBoxPositions = new List<Vector3>();
        foreach (Vector3 candidatePos in validPositions)
        {
            if (spawnedBoxCount >= spawnSetting.maxBoxCount) break;
            bool isOverlap = false;
            foreach (Vector3 monsterPos in actualSpawnedMonsterPositions)
            {
                if (Vector3.Distance(monsterPos, candidatePos) < minSpawnInterval)
                {
                    isOverlap = true;
                    break;
                }
            }
            if (isOverlap) continue;
            bool tooCloseToBox = false;
            foreach (Vector3 boxPos in actualSpawnedBoxPositions)
            {
                if (Vector3.Distance(boxPos, candidatePos) < minSpawnInterval)
                {
                    tooCloseToBox = true;
                    break;
                }
            }
            if (tooCloseToBox) continue;
            if (Random.value > spawnSetting.boxSpawnChane) continue;
            Vector3 spawnPos = candidatePos;
            MapEvent.onRequestBoxSpawn?.Invoke(spawnPos, spawnSetting, (box) =>
            {
                if (box != null)
                {
                    _spawnedBoxes.Add(box);
                    spawnedBoxCount++;
                    actualSpawnedBoxPositions.Add(spawnPos);
                }
            });
        }
        // 7. 촉수 함정 활성화 등록
        if (hollowTrapData != null && hollowTrapData.hollowTrapList.Count > 0)
        {
            _activeCliffNodes.Clear();
            _nextTrapSpawnTime = Time.time;
            foreach (var gimmick in hollowTrapData.hollowTrapList)
            {
                _activeCliffNodes.Add(new SCliffTrapNode
                {
                    worldPosition = transform.TransformPoint(gimmick.position),
                    isEvaluted = false,
                    isTriggered = false
                });
            }
            _hasTrap = new bool[_activeCliffNodes.Count];
            Debug.Log($"[MapChunkSpawnController] {gameObject.name}의 낭떠러지 함정 {_activeCliffNodes.Count}개 작동 대기 등록 완료.");
        }
    }

    public void RecycleAll()
    {
        foreach (var monster in _spawnedMonsters)
        {
            if (monster != null && monster.activeSelf)
                ObjectPoolManager.Instance.MonsterPush(monster);
        }
        _spawnedMonsters.Clear();
        foreach (var box in _spawnedBoxes)
        {
            if (box != null && box.activeSelf)
                ObjectPoolManager.Instance.BoxPush(box);
        }
        _spawnedBoxes.Clear();
        foreach (var trap in _spawnedTraps)
        {
            if (trap != null && trap.activeSelf)
                ObjectPoolManager.Instance.TentaclePush(trap);
        }
        _spawnedTraps.Clear();
        _activeCliffNodes.Clear();
    }


    #region Courtin
    private System.Collections.IEnumerator SpawnTrapWithDelay(Vector3 spawnPosition, bool isUp, float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        MapEvent.onRequestTrapSpawn?.Invoke(spawnPosition, isUp, (trapObj) =>
        {
            if (trapObj != null) _spawnedTraps.Add(trapObj);
        });
    }

    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (hollowTrapData == null || hollowTrapData.hollowTrapList == null) return;

        Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.5f); // 빨간색 반투명
        foreach (var gimmick in hollowTrapData.hollowTrapList)
        {
            Vector3 worldPos = transform.TransformPoint(gimmick.position);
            Gizmos.DrawSphere(worldPos, 0.5f);
            Gizmos.DrawLine(worldPos + Vector3.down * 1.5f, worldPos + Vector3.up * 1.5f);
        }

        Gizmos.color = new Color(1.0f, 0.92f, 0.012f, 0.3f);
        foreach(Transform child in transform)
        {
            if(child.name.Contains("NoSpawnZone"))
            {
                BoxCollider2D boxCol = child.GetComponent<BoxCollider2D>();
                if(boxCol != null)
                {
                    Matrix4x4 rotationMatrix = Matrix4x4.TRS(child.position, child.rotation, child.lossyScale);
                    Gizmos.matrix = rotationMatrix;

                    Gizmos.DrawCube((Vector3)boxCol.offset, (Vector3)boxCol.size);
                    Gizmos.color = new Color(1.0f, 0.92f, 0.012f, 0.8f);

                    Gizmos.DrawWireCube((Vector3)boxCol.offset, (Vector3)boxCol.size);
                    Gizmos.matrix = Matrix4x4.identity;
                }
            }
        }
    }
#endif
}


using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

struct CliffTrapNode
{
    public Vector3 worldPosition;
    public bool isEvaluted;
    public bool isTriggered;
}

public class TentacleTrapSpawner : MonoBehaviour
{
    [Header("데이터 에셋 연결")]
    [SerializeField] private SOMapData mapData;

   
    private List<GameObject> spawnedTraps = new List<GameObject>();
    private List<CliffTrapNode> activeCliffNodes = new List<CliffTrapNode>();
    private float nextSpawnTime = 0f;
    private bool[] _hasTrap;

    #region TentacleTrapSettings
    [Header("함정 설정")]
    [Tooltip("페이즈별 촉수 함정 발동 확률")]
    [SerializeField] private float[] TentacletrapSpawnChanes = new float[3] { 0.30f, 0.65f, 0.95f };

    [Tooltip("페이즈별 함정 간의 스폰 딜레이")]
    [SerializeField] private float[] staggerDelays = new float[3] { 0.5f, 0.3f, 0.1f };
    #endregion

    [SerializeField] private float cameraMargin = 1.0f;

    private void Start()
    {
        _hasTrap = new bool[activeCliffNodes.Count];
    
    }

    #region Update
    private void Update()
    {
        #region Exception Handling
        if (activeCliffNodes.Count == 0) return;
        if (Camera.main == null) return;
        #endregion

        
        int currentPhase = MapManager.Instance?.CurrentLogicalPhase ?? 0;   // MapManager.Instance != null ? MapManager.Instance.CurrentLogicalPhase : 0;
        currentPhase = Mathf.Clamp(currentPhase, 0, 2);

        // 현재 페이즈에 맞는 확률과 딜레이 값 선택
        float trapSpawnChance = TentacletrapSpawnChanes[Mathf.Clamp(currentPhase, 0, TentacletrapSpawnChanes.Length - 1)];
        float staggerDelay = staggerDelays[Mathf.Clamp(currentPhase, 0, staggerDelays.Length - 1)];

        // 카메라 가로 반쪽 및 우측 경계선 계산(수학적으로)
        float camHalfWidth = Camera.main.orthographicSize + Camera.main.aspect;
        float camRightBound = Camera.main.transform.position.x + camHalfWidth + cameraMargin;
        //Debug.Log("camRightBound : " + camRightBound);
        // 대기 중인 모든 낭떠러지 노드 검사
        for(int i= 0; i < activeCliffNodes.Count; i++)
        {
            var node = activeCliffNodes[i];
            if (node.isEvaluted) continue;

            // 플레이어(카메라)가 오른쪽으로 진행
            // --> 낭떠러지 X좌표가 카메라 우측 감지 영역 내로 들어왔는지 판단
            if(node.worldPosition.x <= camRightBound && !_hasTrap[i])
            {
                node.isEvaluted = true;
             
                // 확률 검사
                float roll = UnityEngine.Random.value;
                if(roll <= trapSpawnChance)
                {
                    node.isTriggered = true;

                    // 중앙 통제형 시차 스케줄러 적용
                    float scheduledTime = Mathf.Max(Time.time, nextSpawnTime);
                    float delay = scheduledTime - Time.time;

                    _hasTrap[i] = true;
                    StartCoroutine(SpawnTrapWithDelay(node.worldPosition, delay));

                    // 다음 스폰 예약 시간을 stagerDelay 만큼 미룸
                    nextSpawnTime = scheduledTime + staggerDelay;
                }
                else
                {
                    Debug.Log($"[TentacleTrapSpawner] 낭떠러지 {node.worldPosition} 함정 발동 실패 (확률 탈락)");
                }
            }
        }
    }
    #endregion

    #region Call RespawnTrap
    public void SpawnTraps()
    {
        #region Exception Handling
       if(mapData == null)
        {
            Debug.LogError("SOMapData가 없습니다.");
        }
        #endregion

        activeCliffNodes.Clear();
        nextSpawnTime = Time.time;
        // 2. 베이킹되어 저장된 상대 좌표들을 월드 좌표로 변환하여 RespawnTrap 호출
        int registerCount = 0;
        foreach(var gimmick in mapData.gimmicList)
        {
            if(gimmick.gimmickType == EGimmickType.HollowTrap)
            {
                // 월드 좌표로 변환
                Vector3 worldPos = transform.TransformPoint(gimmick.position);

                activeCliffNodes.Add(new CliffTrapNode
                {
                    worldPosition = worldPos,
                    isEvaluted = false,
                    isTriggered = false
                });

                registerCount++;
            }
        }

        // 확인용 디버깅
        Debug.Log($"[TentacleTrapSpawner] {gameObject.name}의 낭떠러지 함정 {registerCount}개 스폰 완료.");
        
    }
    #endregion

    #region Recycle
    public void RecycleTraps()
    {
        foreach(var trap in spawnedTraps)
        {
            if(trap != null && trap.activeSelf)
            {
                ObjectPoolManager.Instance.TentaclePush(trap);
            }
        }
        spawnedTraps.Clear();
    }
    #endregion

    #region Baking

#if UNITY_EDITOR
    // 3. 우클릭 메뉴를 통해 베이킹
    [ContextMenu("낭떠러지 좌표 찾기")]
    public void BakeHollowPoints()
    {
        #region Exception Handling
        // 맵청크의 가로 범위를 가져오기 위해 MapChunk 탐색
        MapChunk chunk = GetComponentInParent<MapChunk>();
        if(chunk == null)
        {
            Debug.LogError("[TentacleTrapSpawner] MapChunk 찾지 못했습니다.");
            return;
        }

        if(chunk.startPosition == null || chunk.endPosition == null)
        {
            Debug.LogError("[TentacleTrapSpawner] 시작/끝 앵커가 지정되지 않았습니다.");
            return;
        }

        UnityEngine.Tilemaps.Tilemap tilemap = chunk.GetComponentInChildren<UnityEngine.Tilemaps.Tilemap>();
        if(tilemap == null)
        {
            Debug.LogError("[TentacleTrapSpawner] MapChunk 내에서 Tilemap 컴포넌트를 찾지 못했습니다.");
            return;
        }
        // [교정] 타일맵 오브젝트에 부착된 모든 Collider2D 컴포넌트(TilemapCollider2D 및 CompositeCollider2D 모두 포함) 수집
        Collider2D[] groundColliders = tilemap.GetComponents<Collider2D>();
        if (groundColliders.Length == 0)
        {
            Debug.LogWarning($"[TentacleTrapSpawner] {tilemap.gameObject.name}에 Collider2D 컴포넌트가 없어 낭떠러지 판정이 정상 작동하지 않을 수 있습니다.");
        }
        #endregion

        // 1. 기존에 저장되어 있던 낭떠러지(HollowTrap) 데이터만 지움
        mapData.gimmicList.RemoveAll(g => g.gimmickType == EGimmickType.HollowTrap);

        float startWorldX = chunk.startPosition.position.x;
        float endWorldX = chunk.endPosition.position.x;
        float startWorldY = chunk.startPosition.position.y;

        // 2. 낭떠러지(구멍) 구간 탐색 시작
        float step = 0.2f;                       // 0.2미터 간격으로 정밀 스캔
        float checkY = -10.0f;                    // 캐릭터 기준 기본 Y 레벨 오프셋
        float minGapWidth = 0.8f;                // 함정을 스폰할 최소 구멍 너비 (너무 좁은 틈새는 제외)
        List<Vector3> detectedCliffs = new List<Vector3>();
        bool inGap = false;
        float lastGroundX = startWorldX;
        // 시작 지점에 땅이 있는지 미리 체크
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
                    // 구멍 구간이 끝나고 땅이 다시 시작됨 -> 중앙값 계산하여 낭떠러지 등록
                    float gapWidth = x - lastGroundX;
                    if (gapWidth >= minGapWidth)
                    {
                        float centerX = (lastGroundX + x) / 2f;
                        detectedCliffs.Add(new Vector3(centerX, startWorldY + checkY, 0f));
                    }
                    inGap = false;
                }
                lastGroundX = x; // 마지막으로 땅이 확인된 위치 갱신
            }
            else
            {
                if (!inGap)
                {
                    // 땅이 끝나고 구멍 구간이 시작됨
                    inGap = true;
                }
            }
        }
        // 루프 종료 후에도 여전히 구멍 상태라면 (맵 오른쪽 끝이 낭떠러지인 경우)
        if (inGap)
        {
            float gapWidth = endWorldX - lastGroundX;
            if (gapWidth >= minGapWidth)
            {
                float centerX = (lastGroundX + endWorldX) / 2f;
                detectedCliffs.Add(new Vector3(centerX, startWorldY + checkY, 0f));
            }
        }
        // 3. 검출된 중앙 좌표들을 로컬 좌표로 변환하여 SOMapData에 저장
        int bakedCount = 0;
        foreach (Vector3 worldSpawnPos in detectedCliffs)
        {
            Vector3 localSpawnPos = transform.InverseTransformPoint(worldSpawnPos);
            mapData.gimmicList.Add(new SGimmickSpawnData
            {
                gimmickType = EGimmickType.HollowTrap,
                position = localSpawnPos, // 로컬 좌표로 정확히 저장
                prefabIndex = 0
            });
            bakedCount++;
        }
        // 4. SO 파일 디스크 물리 저장 및 Dirty 마킹
        UnityEditor.EditorUtility.SetDirty(mapData);
        UnityEditor.AssetDatabase.SaveAssets();

        Debug.Log($"<color=cyan>[TentacleTrapSpawner] {gameObject.name} 스캔 완료: 총 {bakedCount}개의 낭떠러지 좌표가 SOMapData에 저장됨.</color>");
    }
#endif
    #endregion

    #region EditorVisualization
    // 4. 에디터 씬 뷰 시각화 : 낭떠러지 공격 위치를 빨간색 구체와 화살표선으로 씬 뷰에서 보여줌
    private void OnDrawGizmosSelected()
    {
        if(mapData == null) return;
        Gizmos.color = Color.red;
        foreach (var gimmick in mapData.gimmicList)
        {
            if(gimmick.gimmickType != EGimmickType.HollowTrap)
            {
                continue;
            }

            Vector3 worldPos = transform.TransformPoint(gimmick.position);
            Gizmos.DrawWireSphere(worldPos, 0.4f);
            Gizmos.DrawLine(worldPos + Vector3.down * 0.5f, worldPos + Vector3.up * 0.8f);
        }
    }
    #endregion

    #region CheckHasGroundAtX
    ///<summary>
    /// 특정 X 좌표의 수직 범위 내에 땅 콜라이더가 존재하는지 검사
    /// </summary>
    private bool CheckHasGroundAtX(float x, float startWorldY, Collider2D[] groundColliders)
    {
        // 기존 낭떠러지 스캔과 동일한 수직 검사 범위
        for(int yOffset = -5; yOffset <= 2; ++yOffset)
        {
            Vector2 scanPoint = new Vector2(x, startWorldY + yOffset);
            foreach(var col in groundColliders)
            {
                if(col != null && col.OverlapPoint(scanPoint))
                {
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    #region Coroutine
    private System.Collections.IEnumerator SpawnTrapWithDelay(Vector3 spawnPosition, float delay)
    {
        if(delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        MapEvent.onRequestTrapSpawn?.Invoke(spawnPosition, (trapObj) =>
        {
            if(trapObj != null)
            {
                spawnedTraps.Add(trapObj);
            }
        });
    }

    #endregion
}

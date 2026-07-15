using UnityEngine;
using System.Collections.Generic;
using System;

public class MapChunk : MonoBehaviour
{
    [Header("시작/끝 앵커")]
    public Transform startPosition;
    public Transform endPosition;

    [Header("카메라 제한 영역")]
    public Collider2D cameraBoundaryCollider;

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

        // 2. 하위의 모든 스폰 포인트 컴포넌트 수집(이건 Ground도 GameObject brush로 변경하여 자동 스폰지점찍게끔)
        MonsterSpawnPoint[] spawnPoints = GetComponentsInChildren<MonsterSpawnPoint>(true);

        foreach(var spawnPoint in spawnPoints)
        {
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

            // 4. 필터링을 통과한 포인트에 한해 스폰 이벤트 발행
            string monsterName = spawnPoint.MonsterType.ToString(); // "Base" 또는 "Bird"
            // 이벤트를 통해 MonsterRespawn에게 소환을 요청하고, 생성된 오브젝트를 콜백으로 리스트에 담습니다.
            MapEvent.onRequestMonsterSpawn?.Invoke(monsterName, spawnPoint.transform.position, (monster) =>
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

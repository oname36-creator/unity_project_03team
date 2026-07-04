using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("맵 세팅")]
    public GameObject[] mapPrefabs;     // 맵 프리팹 연결
    public Transform player;
    
    // 플레이어 앞쪽 거리에 도달하면 새 맵 스폰 
    public float spawnTriggerDistance = 30f;

    // 오브젝트 풀링을 위한 2개의 바구니
    private Queue<GameObject> mapPool = new Queue<GameObject>();
    private Queue<GameObject> activeMaps = new Queue<GameObject>();

    private Vector3 nextSpawnPosition = Vector3.zero;   // 다음 맵이 생성될 좌표
    void Start()
    {
        // 게임 시작 시 맵 4개 미리 깔아둠
        for(int i=0; i<4; ++i)
        {
            SpawnNextMap();
        }
    }

    void Update()
    {
        // 캐릭터가 다음 맵 생성 지점에 가까워지면 새 맵을 깐다
        if(player.position.x + spawnTriggerDistance > nextSpawnPosition.x)
        {
            SpawnNextMap();
            RecycleOldMap();
        }
    }

    private void SpawnNextMap()
    {
        GameObject mapToSpawn;

        // 남은 맵이 있으면 꺼내고 없으면 만듦
        if (mapPool.Count > 0)
        {
            mapToSpawn = mapPool.Dequeue();
            mapToSpawn.SetActive(true);
        }
        else
        {
            // 랜덤 생성
            int randomIndex = Random.Range(0, mapPrefabs.Length);
            mapToSpawn = Instantiate(mapPrefabs[randomIndex]);
        }

        // 맵 위치를 다음 스폰 좌표로 이동
        mapToSpawn.transform.position = nextSpawnPosition;
        activeMaps.Enqueue(mapToSpawn);

        // 끝지점 체크를 하는 마커를 찾아서 다음 스폰 좌표로 갱신
        Transform endPos = mapToSpawn.transform.Find("EndPosition");

        if (endPos != null)
        {
            nextSpawnPosition = endPos.position;
        }
        else
        {
            Debug.LogError($"{mapToSpawn.name}에 'EndPosition' 마커가 없다.");
        }
    }

    private void RecycleOldMap()
    {
        // 제일 뒤에 있는 맵 꺼내서 비활성화 시키고 Pool에 넣는다
        GameObject oldMap = activeMaps.Dequeue();
        oldMap.SetActive(false);
        mapPool.Enqueue(oldMap);
    }
}

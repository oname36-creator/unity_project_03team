using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : Singleton<ObjectPoolManager>
{

    [Header("총알 설정")]
    public GameObject bulletPrefab; // 인스펙터에서 총알 프리팹을 할당하세요
    public int poolSize = 20;        // 처음에 미리 만들어둘 총알 개수

    private Queue<GameObject> bulletPool = new Queue<GameObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializePool();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 게임 시작 시 풀을 미리 채워둡니다.
    private void InitializePool()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("ObjectPoolManager: 총알 프리팹이 할당되지 않았습니다!");
            return;
        }

        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.transform.SetParent(this.transform);
            bullet.SetActive(false); // 비활성화 상태로 보관
            bulletPool.Enqueue(bullet);
        }
    }

    // 플레이어가 총알을 요청할 때 꺼내주는 메서드
    public GameObject GetBullet(Vector2 position, Quaternion rotation)
    {
        if (bulletPool.Count > 0)
        {
            GameObject bullet = bulletPool.Dequeue();
            bullet.transform.position = position;
            bullet.transform.rotation = rotation;
            bullet.SetActive(true); // 활성화해서 내보냄
            return bullet;
        }
        else
        {
            // 만약 총알이 모자라면 임시로 새로 생성해서 줍니다.
            GameObject bullet = Instantiate(bulletPrefab, position, rotation);
            return bullet;
        }
    }

    // 총알이 화면 밖으로 나가거나 적에 부딪혔을 때 다시 풀로 반반환하는 메서드
    public void ReturnBullet(GameObject bullet)
    {
        bullet.SetActive(false);
        bullet.transform.SetParent(this.transform);
        bulletPool.Enqueue(bullet);
    }
}


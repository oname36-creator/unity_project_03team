using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    [Header("Monster Bullet Prefab")]
    [SerializeField] private GameObject _monsterBulletPrefab;


    private List<GameObject> _monsterBulletPool = new List<GameObject>();




    void Start()
    {
        for (int i = 0; i < 20; i++)
        {
            GameObject obj = Instantiate(_monsterBulletPrefab, this.transform);
            obj.SetActive(false); // 비활성화 상태로 대기
            _monsterBulletPool.Add(obj); // 리스트에 추가
        }
    }


    // 풀에서 총알 가져오는 함수
    public GameObject MonsterBulletPop()
    {
        for (int i = 0; i < _monsterBulletPool.Count; i++)
        {
            // 꺼져있는(놀고있는) 총알을 발견하면
            if (_monsterBulletPool[i].activeSelf == false)
            {
                _monsterBulletPool[i].SetActive(true); // 켜서
                return _monsterBulletPool[i];          // 내보내기
            }
        }

        return null;
    }

    // 다 쓴 총알을 다시 풀에 반환하는 함수
    public void MonsterBulletPush(GameObject obj)
    {
        if (obj == null) return;

        obj.SetActive(false); // 꺼서 다시 재사용 대기 상태로 만들기
    }
}

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    [Header("Player")]
    public GameObject Player;

    [Header("Boss")]
    public GameObject Boss;

    [Header("Box")]
    [SerializeField] private GameObject _boxPrefab;

    [Header("Monster Bullet Prefab")]
    [SerializeField] private GameObject _monsterBulletPrefab;

    [Header("Monster Prefab")]
    [SerializeField] private GameObject _monsterBirdPrefab;
    [SerializeField] private GameObject _monsterBasePrefab;
    [SerializeField] private GameObject _monsterDarkWolfPrefab;

    [Header("Tentacle Prefab")]
    [SerializeField] private GameObject _tentaclePrefab;

    [Header("Effect Prefab")]
    [SerializeField] private GameObject _dustEffectPrefab;
    [SerializeField] private GameObject _bigDustEffectPrefab;
    [SerializeField] private GameObject _slashEffectPrefab;
    [SerializeField] private GameObject _smokeEffectPrefab;
    [SerializeField] private GameObject _smokeBurstEffectPrefab;

    [Header("Being Thrown")]
    [SerializeField] private GameObject _treePrefab;
    [SerializeField] private GameObject _stonePrefab;
    [SerializeField] private GameObject _logPrefab;


    [Header("총알 설정")]
    public GameObject bulletPrefab; // 인스펙터에서 총알 프리팹을 할당하세요
    public int poolSize = 20;        // 처음에 미리 만들어둘 총알 개수

    private Queue<GameObject> bulletPool = new Queue<GameObject>();

    #region Pool
    private Queue<GameObject> _monsterBulletPool = new Queue<GameObject>();
    private Queue<GameObject> _monsterBirdPool = new Queue<GameObject>();
    private Queue<GameObject> _monsterBasePool = new Queue<GameObject>();
    private Queue<GameObject> _monsterDarkWolfPool = new Queue<GameObject>();
    private Queue<GameObject> _tentaclePool = new Queue<GameObject>();
    private Queue<GameObject> _dustEffectPool = new Queue<GameObject>();
    private Queue<GameObject> _bigDustEffectPool = new Queue<GameObject>();
    private Queue<GameObject> _slashEffectPool = new Queue<GameObject>();
    private Queue<GameObject> _smokeEffectPool = new Queue<GameObject>();
    private Queue<GameObject> _smokeBurstEffectPool = new Queue<GameObject>();
    private Queue<GameObject> _boxPool = new Queue<GameObject>();
    private Queue<GameObject> _thrownPool = new Queue<GameObject>();
    #endregion


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializePool();
        
        // 몬스터 총알 생성
        for (int i = 0; i < 20; i++)
        {
            GameObject obj = Instantiate(_monsterBulletPrefab, this.transform);
            obj.SetActive(false); // 비활성화 상태로 대기
            _monsterBulletPool.Enqueue(obj); // 리스트에 추가
        }
        // Base 몬스터 생성
        for (int i = 0; i < 20; i++)
        {
            GameObject obj = Instantiate(_monsterBasePrefab, this.transform);
            obj.GetComponent<MonsterController>().Player = Player;
            obj.SetActive(false); // 비활성화 상태로 대기
            _monsterBasePool.Enqueue(obj); // 리스트에 추가
        }
        // Bird 몬스터 생성
        for (int i = 0; i < 20; i++)
        {
            GameObject obj = Instantiate(_monsterBirdPrefab, this.transform);
            obj.GetComponent<MonsterController>().Player = Player;
            obj.SetActive(false); // 비활성화 상태로 대기
            _monsterBirdPool.Enqueue(obj); // 리스트에 추가
        }
        // DarkWolf 몬스터 생성
        for (int i = 0; i < 20; i++)
        {
            GameObject obj = Instantiate(_monsterDarkWolfPrefab, this.transform);
            obj.GetComponent<MonsterController>().Player = Player;
            obj.SetActive(false); // 비활성화 상태로 대기
            _monsterDarkWolfPool.Enqueue(obj); // 리스트에 추가
        }
        // Tentacle 생성
        for (int i = 0; i < 20; i++)
        {
            GameObject obj = Instantiate(_tentaclePrefab, this.transform);
            obj.GetComponent<TentacleController>().Boss = Boss.GetComponent<BossController>();
            obj.GetComponent<TentacleController>().Body = Boss.GetComponent<BodyController>();
            obj.SetActive(false); // 비활성화 상태로 대기
            _tentaclePool.Enqueue(obj); // 리스트에 추가
        }
        // Dust Effect 생성
        for (int i = 0; i < 20; i++)
        {
            GameObject obj = Instantiate(_dustEffectPrefab, this.transform);
            obj.SetActive(false); // 비활성화 상태로 대기
            _dustEffectPool.Enqueue(obj); // 리스트에 추가
        }
        
        // Big Dust Effect 생성
        for (int i = 0; i < 20; i++)
        {
            GameObject obj = Instantiate(_bigDustEffectPrefab, this.transform);
            obj.SetActive(false); // 비활성화 상태로 대기
            _bigDustEffectPool.Enqueue(obj); // 리스트에 추가
        }

        // slash Effect 생성
        for (int i = 0; i < 50; i++)
        {
            GameObject obj = Instantiate(_slashEffectPrefab, this.transform);
            obj.SetActive(false); // 비활성화 상태로 대기
            _slashEffectPool.Enqueue(obj); // 리스트에 추가
        }
        // smoke burst Effect 생성
        for (int i = 0; i < 10; i++)
        {
            GameObject obj = Instantiate(_smokeBurstEffectPrefab, this.transform);
            obj.SetActive(false); // 비활성화 상태로 대기
            _smokeBurstEffectPool.Enqueue(obj); // 리스트에 추가
        }
        // smoke Effect 생성
        for (int i = 0; i < 30; i++)
        {
            GameObject obj = Instantiate(_smokeEffectPrefab, this.transform);
            obj.SetActive(false); // 비활성화 상태로 대기
            _smokeEffectPool.Enqueue(obj); // 리스트에 추가
        }

        // thrown pool 생성
        for (int i = 0; i < 4; ++i) 
        {
            GameObject obj1 = Instantiate(_treePrefab, this.transform);
            GameObject obj2 = Instantiate(_stonePrefab, this.transform);
            GameObject obj3 = Instantiate(_logPrefab, this.transform);

            obj1.SetActive(false);
            obj2.SetActive(false);
            obj3.SetActive(false);

            _thrownPool.Enqueue(obj1);
            _thrownPool.Enqueue(obj2);
            _thrownPool.Enqueue(obj3);
        }

        // box Pool 생성
        for (int i = 0; i < 20; ++i)
        {
            GameObject obj = Instantiate(_boxPrefab, this.transform);
            obj.SetActive(false);
            _boxPool.Enqueue(obj);
        }
    }

#region MonsterPop
    #region Bullet
    // 풀에서 총알 가져오는 함수
    public GameObject MonsterBulletPop()
    {

        if (_monsterBulletPool.Count > 0) 
        {
            GameObject obj = _monsterBulletPool.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        return null;
    }
    #endregion

    #region Thrown

    public GameObject ThrownPop() 
    {
        if (_thrownPool.Count > 0)
        {
            int randomInt = Random.Range(0, 5);

            for (int i = 0; i < randomInt; ++i)
            {
                GameObject go = _thrownPool.Dequeue();
                _thrownPool.Enqueue(go);
            }

            GameObject obj = _thrownPool.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        return null;

    }



    #endregion

    #region Base
    // 풀에서 Base 몬스터 가져오는 함수
    public GameObject MonsterBasePop()
    {

        if (_monsterBasePool.Count > 0)
        {
            GameObject obj = _monsterBasePool.Dequeue();
            return obj;
        }

        return null;
    }
    #endregion

    #region Bird
    // 풀에서 Bird 몬스터 가져오는 함수
    public GameObject MonsterBirdPop()
    {

        if (_monsterBirdPool.Count > 0)
        {
            GameObject obj = _monsterBirdPool.Dequeue();
            return obj;
        }

        return null;
    }
    #endregion

    #region DarkWolf
    // 풀에서 Bird 몬스터 가져오는 함수
    public GameObject MonsterDarkWolfPop()
    {

        if (_monsterDarkWolfPool.Count > 0)
        {
            GameObject obj = _monsterDarkWolfPool.Dequeue();
            return obj;
        }

        return null;
    }
    #endregion

    #region Tetacle
    // 풀에서 가져오는 함수
    public GameObject TentaclePop(bool trap = false)
    {
        if (_tentaclePool.Count > 0)
        {
            GameObject obj = _tentaclePool.Dequeue();
            obj.GetComponent<TentacleController>().isTrap = trap;
            return obj;
        }

        if (_tentaclePrefab != null)
        {
            GameObject obj = Instantiate(_tentaclePrefab, this.transform);

            // Start()에서 최초 생성 시 해주던 Boss, Body 캐싱 작업을 동일하게 수행
            obj.GetComponent<TentacleController>().Boss = Boss.GetComponent<BossController>();
            obj.GetComponent<TentacleController>().Body = Boss.GetComponent<BodyController>();
            obj.GetComponent<TentacleController>().isTrap = trap;

            return obj;
        }

        return null;
    }
    #endregion

    #region Effect
    public GameObject DustEffectPop()
    {
        if (_dustEffectPool.Count > 0)
        {
            GameObject obj = _dustEffectPool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        return null;
    }

    public GameObject BigDustEffectPop()
    {
        if (_bigDustEffectPool.Count > 0)
        {
            GameObject obj = _bigDustEffectPool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        return null;
    }

    public GameObject SlashEffectPop()
    {
        if (_slashEffectPool.Count > 0)
        {
            GameObject obj = _slashEffectPool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        return null;
    }

    public GameObject SmokeEffectPop() 
    {
        if(_smokeEffectPool.Count > 0) 
        {
            GameObject obj = _smokeEffectPool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        return null;
    }

    public GameObject SmokeBurstEffectPop() 
    {
        if (_smokeBurstEffectPool.Count > 0)
        {
            GameObject obj = _smokeBurstEffectPool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        return null;
    }


    #endregion

    #endregion

#region MonsterPush

    #region Bullet

    // 다 쓴 총알을 다시 풀에 반환하는 함수
    public void MonsterBulletPush(GameObject obj)
    {
        if (obj == null) return;

        obj.SetActive(false);
        _monsterBulletPool.Enqueue(obj);

    }
    #endregion

    #region Thrown

    public void ThrownPush(GameObject obj)
    {
        if(obj == null) return;

        obj.SetActive(false);
        _thrownPool.Enqueue(obj);

    }



    #endregion
    #region Monster(bird, base, darkWolf)

    // 몬스터를 다시 풀에 반환하는 함수
    public void MonsterPush(GameObject obj)
    {
        if (obj == null) return;
        obj.SetActive(false);
        Debug.Log("오브젝트 풀 반환");
        if (obj.GetComponent<MonsterController>().Name == "Base")
        {
            _monsterBasePool.Enqueue(obj);
        }
        else if (obj.GetComponent<MonsterController>().Name == "Bird")
        {
            _monsterBirdPool.Enqueue(obj);
        }
        else if (obj.GetComponent<MonsterController>().Name == "DarkWolf")
        {
            _monsterDarkWolfPool.Enqueue(obj);
        }

    }
    #endregion

    #region Tentacle
    // 다시 풀에 반환하는 함수
    public void TentaclePush(GameObject obj)
    {
        if (obj == null) return;
        obj.SetActive(false);
        _tentaclePool.Enqueue(obj);
    }
    #endregion

    #region Effect
    public void DustEffectPush(GameObject obj)
    {
        if (obj == null) return;
        obj.SetActive(false);
        _dustEffectPool.Enqueue(obj);
    }

    public void BigDustEffectPush(GameObject obj)
    {
        if (obj == null) return;
        obj.SetActive(false);
        _bigDustEffectPool.Enqueue(obj);
    }


    public void SlashEffectPush(GameObject obj)
    {
        if (obj == null) return;
        obj.SetActive(false);
        _slashEffectPool.Enqueue(obj);
    }

    public void SmokeEffectPush(GameObject obj)
    {
        if (obj == null) return;
        obj.SetActive(false);
        _smokeEffectPool.Enqueue(obj);
    }
    public void SmokeBurstEffectPush(GameObject obj)
    {
        if (obj == null) return;
        obj.SetActive(false);
        _smokeBurstEffectPool.Enqueue(obj);
    }


    #endregion
    #endregion

    #region Init
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
    #endregion

#region ETC

    #region PlayerBullet
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
    #endregion

    #region ReturnBullet
    // 총알이 화면 밖으로 나가거나 적에 부딪혔을 때 다시 풀로 반반환하는 메서드
    public void ReturnBullet(GameObject bullet)
    {
        bullet.SetActive(false);
        bullet.transform.SetParent(this.transform);
        bulletPool.Enqueue(bullet);
    }
    #endregion

    #region Box
    // 풀-> 상자 꺼내옴
    public GameObject BoxPop()
    {
        if(_boxPool.Count > 0)
        {
            GameObject obj = _boxPool.Dequeue();
            return obj;
        }

        // 풀이 비어있다면 새로 생성하여 대처
        if(_boxPrefab != null)
        {
            GameObject obj = Instantiate(_boxPrefab, this.transform);
            return obj;
        }
        return null;
    }

    // 상자-> 풀로 반환
    public void BoxPush(GameObject obj)
    {
        if (obj == null) return;
        obj.SetActive(false);
        obj.transform.SetParent(this.transform);
        _boxPool.Enqueue(obj);
    }

    
    #endregion

#endregion

}


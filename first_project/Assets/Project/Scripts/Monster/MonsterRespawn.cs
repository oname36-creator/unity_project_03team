using UnityEngine;
using System;
public class MonsterRespawn : MonoBehaviour
{
    #region Event
    private void OnEnable()
    {
        MapEvent.onRequestMonsterSpawn += HandleSpawnRequest;
        MapEvent.onRequestTrapSpawn += HandleTrapSpawnRequest;
    }
    private void OnDisable()
    {
        MapEvent.onRequestMonsterSpawn -= HandleSpawnRequest;
        MapEvent.onRequestTrapSpawn -= HandleTrapSpawnRequest;
    }
    #endregion

    #region HandleSpawnRequest
    private void HandleSpawnRequest(string name, Vector3 pos, Action<GameObject> onSpawned)
    {
        GameObject monster = Respawn(name, pos);
        onSpawned?.Invoke(monster);
    }
    #endregion

    #region HandleTrapSpawnRequest
    private void HandleTrapSpawnRequest(Vector2 rootPos, bool isUp, Action<GameObject> onSpawned)
    {
        GameObject trap = RespawnTrap(rootPos, isUp);
        onSpawned?.Invoke(trap);
    }
    #endregion

    // void -> GameObject로 변경
    public GameObject Respawn(string name, Vector3 pos) 
    {
        GameObject monster = null;
        if (name == "Base")
        {
            monster = ObjectPoolManager.Instance.MonsterBasePop();
        }
        else if (name == "Bird") 
        {
            monster = ObjectPoolManager.Instance.MonsterBirdPop();
        }
        else if(name == "DarkWolf") 
        {
            monster = ObjectPoolManager.Instance.MonsterDarkWolfPop();
        }

        if (monster == null) { return null; }
        MonsterController monsterController = monster.GetComponent<MonsterController>();
        
        monster.transform.position = pos;
        
        monsterController.IsDead = false;

        monster.SetActive(true);
        return monster;
    } 


    public GameObject RespawnTrap (Vector2 rootPos, bool up = true)
    {
        //Debug.Log("Respawn Tentacle Trap");
        GameObject obj = ObjectPoolManager.Instance.TentaclePop(true);
        TentacleController tentacleController = obj.GetComponent<TentacleController>();
        obj.SetActive(true);
        if (!up) 
        {
            rootPos.y += 30;
        }

        tentacleController.warningEffectRenderer_2.flipY = !up;

        tentacleController.SetRootPos(rootPos);
        tentacleController.Up = up;
        
        return obj;

    }
    
}

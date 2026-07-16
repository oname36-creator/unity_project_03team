using UnityEngine;
using System;
public class MonsterRespawn : MonoBehaviour
{
    #region Event
    private void OnEnable()
    {
        MapEvent.onRequestMonsterSpawn += HandleSpawnRequest;
    }
    private void OnDisable()
    {
        MapEvent.onRequestMonsterSpawn -= HandleSpawnRequest;
    }
    #endregion

    #region HandleSpawnRequest
    private void HandleSpawnRequest(string name, Vector3 pos, Action<GameObject> onSpawned)
    {
        GameObject monster = Respawn(name, pos);
        onSpawned?.Invoke(monster);
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
        if(monster == null) { return null; }
        MonsterController monsterController = monster.GetComponent<MonsterController>();
        
        monster.transform.position = pos;
        
        monsterController.IsDead = false;

        monster.SetActive(true);
        return monster;
    } 


    public void RespawnTrap (Vector2 rootPos)
    {
        Debug.Log("Respawn Tentacle Trap");
        GameObject obj = ObjectPoolManager.Instance.TentaclePop(true);
        TentacleController tentacleController = obj.GetComponent<TentacleController>();
        obj.SetActive(true);
        tentacleController.SetRootPos(rootPos);


    }
    
}

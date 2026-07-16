using UnityEngine;

public class MonsterRespawn : MonoBehaviour
{


    
    
    public void Respawn(string name, Vector3 pos) 
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
        if(monster == null) { return; }
        MonsterController monsterController = monster.GetComponent<MonsterController>();
        
        monster.transform.position = pos;
        
        monsterController.IsDead = false;

        monster.SetActive(true);

    } 


    public void RespawnTrap (Vector2 rootPos)
    {
        Debug.Log("Respawn Tentacle Trap");
        GameObject obj = ObjectPoolManager.Instance.TentaclePop(true);
        TentacleController tentacleController = obj.GetComponent<TentacleController>();
        tentacleController.SetRootPos(rootPos);

        obj.SetActive(true);



    }
    
}

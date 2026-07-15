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
        GameObject obj = ObjectPoolManager.Instance.TentaclePop();
        TentacleController tentacleController = obj.GetComponent<TentacleController>();
        tentacleController.isTrap = true;
        tentacleController.RootPos = rootPos;

        obj.SetActive(true);

    }
    
}

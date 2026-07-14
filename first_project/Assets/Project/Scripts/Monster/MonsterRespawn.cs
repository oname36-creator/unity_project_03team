using UnityEngine;

public class MonsterRespawn : MonoBehaviour
{

    [Header("Player")]
    public GameObject Player;
    
    
    
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

        monster.GetComponent<MonsterController>().Player = Player;
        monster.transform.position = pos;

    } 




    
}

using UnityEngine;

public class BaseMonsterColider : MonoBehaviour
{



    private MonsterController _monsterController;

    private void Start()
    {
        _monsterController = GetComponent<MonsterController>();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {

            Vector3 closestPoint = GetComponent<BoxCollider2D>().ClosestPoint(collision.transform.position);


            Vector3 normalVector = (closestPoint - transform.position).normalized;

            Debug.Log("NormalVector : " + normalVector);
            Debug.Log("Dot : " + Vector2.Dot(normalVector, Vector2.up));    

            if (Vector2.Dot(normalVector, Vector2.up) > 0.9f)
            {
                    _monsterController.IsDead = true;
            }
        }
    }

}

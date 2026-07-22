using UnityEngine;

public class BaseMonsterColider : MonoBehaviour
{

    [Header("Head Coliider")]
    public BoxCollider2D targetBoxCollider;


    private MonsterController _monsterController;

    private void Start()
    {
        _monsterController = GetComponent<MonsterController>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && collision.otherCollider == targetBoxCollider)
        {
            _monsterController.IsDead = true;
        }
    }

}

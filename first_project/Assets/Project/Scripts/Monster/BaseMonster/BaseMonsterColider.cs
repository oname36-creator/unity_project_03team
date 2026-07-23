using UnityEngine;

public class BaseMonsterColider : MonoBehaviour
{
    [Header("Controllers")]
    [SerializeField] private MonsterController _monsterController;

    [Header("Colliders")]
    [SerializeField] private BoxCollider2D targetBoxCollider;
    [SerializeField] private Collider2D[] _allColliders;

    private void OnEnable()
    {
        if (_allColliders == null) return;


        int length = _allColliders.Length;
        for (int i = 0; i < length; i++)
        {

            _allColliders[i].enabled = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.otherCollider == targetBoxCollider && collision.gameObject.CompareTag("Player"))
        {
            _monsterController.IsDead = true;
            DisableAllColliders();
        }
    }

    private void DisableAllColliders()
    {
        if (_allColliders == null) return;

        int length = _allColliders.Length;
        for (int i = 0; i < length; i++)
        {
            _allColliders[i].enabled = false;
        }
    }
}
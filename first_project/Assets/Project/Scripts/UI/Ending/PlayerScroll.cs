using UnityEngine;

public class PlayerScroll : MonoBehaviour
{
    [Header("플레이어 스크롤 속도")]
    public float scrollSpeed = 0.5f;

    private Transform _playerTransform;


    private void Start()
    {
        _playerTransform = GetComponent<Transform>();
    }

    private void Update()
    {
        _playerTransform.position += Vector3.right * scrollSpeed * Time.deltaTime;
    }

}

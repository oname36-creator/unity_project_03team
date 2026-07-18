using UnityEngine;

public class CameraScrol : MonoBehaviour
{

    [Header("배경 스크롤 속도")]
    public float scrollSpeed = 0.25f;

    private Transform _cameraTransform;


    private void Start()
    {
        _cameraTransform = GetComponent<Transform>();
    }

    private void Update()
    {
        _cameraTransform.position += Vector3.right * scrollSpeed * Time.deltaTime;
    }


}

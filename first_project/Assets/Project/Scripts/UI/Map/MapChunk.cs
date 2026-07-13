using UnityEngine;

public class MapChunk : MonoBehaviour
{
    [Header("시작/끝 앵커")]
    public Transform startPosition;
    public Transform endPosition;

    [Header("카메라 제한 영역")]
    public Collider2D cameraBoundaryCollider;
}

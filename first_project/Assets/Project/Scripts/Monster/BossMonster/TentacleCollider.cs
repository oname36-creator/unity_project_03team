using UnityEngine;

public class TentacleCollider : MonoBehaviour
{
    public TentacleGrabber MainGrabber;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // EdgeCollider에 무언가 닿으면 MainGrabber의 잡기 로직을 대신 실행해줍니다.
        if (MainGrabber != null)
        {
            MainGrabber.TryGrab(other);
        }




    }
}

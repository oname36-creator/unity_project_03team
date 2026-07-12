using UnityEngine;

public class TentacleGrabber : MonoBehaviour
{
    public TentacleController Tentacle;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 잡을 수 있는 태그인지 확인
        if (other.CompareTag("Player") || other.CompareTag("Ground"))
        {
            // 대상의 부모를 이 오브젝트로 설정하거나, FixedJoint2D로 묶어서 끌고 감
            //other.transform.SetParent(this.transform);
            Tentacle.Boss.Target = other.transform;

        }
    }
}
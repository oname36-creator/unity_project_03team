using UnityEngine;

public class TentacleGrabber : MonoBehaviour
{
    public TentacleController Tentacle;
    private BodyController _bodyController;
    private BossController _bossController;

    private GameObject _anchorPoint; // 추가

    public void Start()
    {
        _bodyController = Tentacle.Body;
        _bossController = Tentacle.Boss;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        // 잡을 수 있는 태그인지 확인
        if (other.CompareTag("Player") || other.CompareTag("Ground"))
        {
            if (other.CompareTag("Ground"))
            {
                if (_anchorPoint == null)
                {
                    _anchorPoint = new GameObject("TentacleAnchor");
                }
                // 부착된 순간의 촉수 머리 위치를 타겟으로 고정
                _anchorPoint.transform.position = this.transform.position;
                _anchorPoint.transform.SetParent(other.transform);

                Tentacle.Boss.Target = _anchorPoint.transform;
            }
            else
            {
                Tentacle.Boss.Target = other.transform;
            }

            _bossController.Attached = true;
            Tentacle.IsAttach = true;
        }
    }
}
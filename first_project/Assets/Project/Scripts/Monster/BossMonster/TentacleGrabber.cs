using UnityEngine;

public class TentacleGrabber : MonoBehaviour
{
    public TentacleController Tentacle;
    private BodyController _bodyController;
    private BossController _bossController;

    public void Start()
    {
        _bodyController = Tentacle.Body;
        _bossController = Tentacle.Boss;
    }

    // 자신의 Trigger에 닿았을 때 호출
    private void OnTriggerEnter2D(Collider2D other)
    {
        TryGrab(other);
    }

    // 외부(EdgeCollider 등)에서도 호출할 수 있도록 분리한 잡기 로직
    public void TryGrab(Collider2D other)
    {
        // 플레이어(Player) 또는 몬스터(Monster)만 판정
        if (other.CompareTag("Player") || other.CompareTag("Monster"))
        {
            // 이미 무언가를 잡고 끌고 오는 중(IsAttach == true)이라면 중복 충돌 무시
            // (자신의 Trigger와 EdgeCollider에 동시에 닿아도 여기서 안전하게 걸러집니다)
            if (Tentacle.IsAttach) return;

            GameObject hitObj = other.gameObject;

            // 1. 내가 찜한 애가 맞다면 정상적으로 잡기 성공
            if (Tentacle.Target == hitObj)
            {
                Tentacle.IsAttach = true;
                Debug.Log($"TentacleGrabber: 내 타겟 {hitObj.name} 잡기 성공!");
            }
            // 2. 우연히 다른 애를 건드렸는데, 아무도 찜하지 않은 애라면 낚아채기
            else if (!Tentacle.Boss.IsTargeted(hitObj))
            {
                // 기존 타겟이 있었다면 놔줌
                if (Tentacle.Target != null)
                    Tentacle.Boss.RemoveTarget(Tentacle.Target);

                // 새 타겟 찜하기
                Tentacle.Target = hitObj;
                Tentacle.Boss.AddTarget(hitObj);

                Tentacle.IsAttach = true;
                Debug.Log($"TentacleGrabber: 지나가다 {hitObj.name} 낚아챔!");
            }
            // 3. 남이 찜한 애를 건드렸다면 무시하고 통과
        }
    }
}
using UnityEngine;

public class TentacleGrabber : MonoBehaviour
{
    public TentacleController Tentacle;


    private BossController _bossController;


    private float _lastEffectTime = 0f;
    private const float EFFECT_COOLDOWN = 0.2f;

    public void Start()
    {
        _bossController = Tentacle.Boss;
    }

    // 자신의 Trigger에 닿았을 때 호출
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Tentacle.Attack == false) { return; }
        TryGrab(other);
    }

    // 외부(EdgeCollider 등)에서도 호출할 수 있도록 분리한 잡기 로직
    public void TryGrab(Collider2D other)
    {
        // 1. 몬스터(Monster) 판정 -> 닿으면 즉시 사망, 잡기 처리 안함
        if (other.CompareTag("Monster"))
        {
            if (other.TryGetComponent<MonsterController>(out MonsterController monster))
            {
                monster.IsDead = true;
            }

            GameObject hitObj = other.gameObject;

            // 만약 내가 찜한 대상이 몬스터였다면 초기화
            if (Tentacle.Target == hitObj)
            {
                _bossController.RemoveTarget(hitObj);
                Tentacle.Target = null;
                Tentacle.IsSearch = false; // 타겟이 죽었으므로 회수
            }
            return;
        }

        // 2. 플레이어(Player)만 판정
        if (other.CompareTag("Player"))
        {
            // 아치(포물선) 패턴 중이거나 이미 잡고 있다면 무시
            if (Tentacle.isArch) return;
            if (Tentacle.IsAttach) return;

            GameObject hitObj = other.gameObject;

            // 내가 찜한 애가 맞다면 정상적으로 잡기 성공
            if (Tentacle.Target == hitObj)
            {
                Tentacle.IsAttach = true;
            }

            else if (!_bossController.IsTargeted(hitObj))
            {
                // 기존 타겟이 있었다면 놔줌
                if (Tentacle.Target != null)
                    _bossController.RemoveTarget(Tentacle.Target);

                // 새 타겟 찜하기
                Tentacle.Target = hitObj;
                _bossController.AddTarget(hitObj);

                Tentacle.IsAttach = true;
            }
        }

        else if (other.CompareTag("Ground"))
        {
            if (Time.time - _lastEffectTime < EFFECT_COOLDOWN) return;
            _lastEffectTime = Time.time;

            Vector2 hitPoint = other.ClosestPoint(transform.position);
            GameObject obj;

            if (Tentacle.isArch)
            {
                if (Tentacle.IsGroundHit) return;

                Tentacle.IsGroundHit = true;
                obj = ObjectPoolManager.Instance.BigDustEffectPop();

                GameObject smokeObj = ObjectPoolManager.Instance.SmokeEffectPop();
                if (smokeObj != null)
                {
                    smokeObj.transform.position = hitPoint;
                }
            }
            else
            {
                obj = ObjectPoolManager.Instance.DustEffectPop();
            }

            if (obj != null)
            {
                obj.transform.position = hitPoint;
            }
        }
    }
}
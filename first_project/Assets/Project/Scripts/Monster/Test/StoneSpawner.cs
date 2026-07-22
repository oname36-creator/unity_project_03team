using UnityEngine;

/// <summary>
/// 오브젝트 풀에서 돌(또는 투사체)을 가져와 목표 위치로 던지는 동작을 전담하는 클래스
/// </summary>
public class StoneSpawner : MonoBehaviour
{
    [Header("던지기 설정")]
    [Tooltip("투사체가 생성될 시작 위치 (할당하지 않으면 이 스크립트가 붙은 오브젝트의 위치를 사용합니다)")]
    public Transform spawnPoint;

    public Transform Player;

    private float _time = 0f;

    private void Awake()
    {
        // spawnPoint가 비어있다면 자기 자신의 Transform을 사용합니다.
        if (spawnPoint == null)
        {
            spawnPoint = this.transform;
        }
    }

    public void Update()
    {
        _time += Time.deltaTime;
        if (_time > 10f)
        {
            ThrowStone(Player.position);
            _time = 0f;
        }
    }


    /// <summary>
    /// 외부(상태 머신 등)에서 타겟 위치를 전달받아 돌을 던집니다.
    /// </summary>
    /// <param name="targetPosition">돌이 날아갈 목표 위치</param>
    public void ThrowStone(Vector3 targetPosition)
    {
        // 1. 오브젝트 풀에서 던질 오브젝트를 가져옵니다.
        GameObject thrown = ObjectPoolManager.Instance.ThrownPop();

        if (thrown != null)
        {
            // 2. 투사체의 시작 위치를 설정합니다.
            thrown.transform.position = spawnPoint.position;

            // 3. BeingThrown 컴포넌트를 찾아 초기화(던지기 실행)합니다.
            BeingThrown beingThrownComponent = thrown.GetComponent<BeingThrown>();

            if (beingThrownComponent != null)
            {
                beingThrownComponent.InitializeThrow(targetPosition);
            }
            else
            {
                //Debug.LogError($"[StoneThrower] 가져온 오브젝트({thrown.name})에 BeingThrown 컴포넌트가 없습니다!");
            }
        }
        else
        {
            //Debug.LogWarning("[StoneThrower] ObjectPoolManager에서 투사체를 가져오지 못했습니다. 풀이 비어있는지 확인하세요.");
        }
    }
}
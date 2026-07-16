using UnityEngine;

// 스폰할 몬스터 타입 정의
public enum MonsterType
{
    Base,
    Bird,
    // 슬라임
}
public class MonsterSpawnPoint : MonoBehaviour
{
    [Header("스폰할 몬스터 종류")]
    [SerializeField] private MonsterType _monsterType = MonsterType.Base;
    public MonsterType MonsterType => _monsterType;

    #region OnDrawGizmos
    ///<summary>
    /// 에디터 씬 뷰에서 위치를 쉽게 볼 수 있도록 가이드라인 드로잉
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        // 스폰 포인트에 빨간색 반투명 구와 십자선을 그려 가독성을 높입니다.
        Gizmos.DrawWireSphere(transform.position, 0.4f);
        Gizmos.DrawLine(transform.position + Vector3.left * 0.2f, transform.position + Vector3.right * 0.2f);
        Gizmos.DrawLine(transform.position + Vector3.up * 0.2f, transform.position + Vector3.down * 0.2f);
    }
    #endregion
}

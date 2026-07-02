using UnityEngine;

[CreateAssetMenu(fileName = "BaseMonsterData", menuName = "Scriptable Objects/BaseMonsterData")]
public class BaseMonsterData : ScriptableObject
{
    [Header("Monster Settings")] // 인스펙터에 제목 표시
    public int hp;
    public int damage;
    public int MaxSpeed;

    [Header("Search Settings")] // 인스펙터에 제목 표시
    public int searchRange;
    public float angle;
}

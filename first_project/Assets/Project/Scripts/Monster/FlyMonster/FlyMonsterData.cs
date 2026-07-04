using UnityEngine;

[CreateAssetMenu(fileName = "FlyMonsterData", menuName = "Scriptable Objects/FlyMonsterData")]
public class FlyMonsterData : ScriptableObject
{
    [Header("Monster Settings")] // 인스펙터에 제목 표시
    public int hp;
    public int damage;
    public int Speed;
    public int MaxSpeed;
    public int Force;
    public string Name; 

    [Header("Search Settings")] // 인스펙터에 제목 표시
    public int searchRange;
    public float angle;

    [Header("Attack Settings")] // 인스펙터에 제목 표시
    public int attackRange;

}

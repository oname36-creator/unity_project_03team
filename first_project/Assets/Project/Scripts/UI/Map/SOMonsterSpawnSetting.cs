using UnityEngine;

[CreateAssetMenu(fileName = "SOMonsterSpawnSetting", menuName = "Scriptable Objects/SOMonsterSpawnSetting")]
public class SOMonsterSpawnSetting : ScriptableObject
{
    [Header("타일맵 설정")]
    public string groundTilemapName = "Ground";

    [Header("스폰 몬스터 설정")]
    public MonsterType[] spawnableMonsterTypes = new MonsterType[] { MonsterType.Base, MonsterType.Bird };

    [Header("스폰 빈도 및 제한")]
    [Range(0f, 1f)] public float spawnChance = 0.15f;

    public int minSpawnInterval = 3;
    public int maxMonsterCount = 3;
    public float bridHeightOffset = 3.5f;
}

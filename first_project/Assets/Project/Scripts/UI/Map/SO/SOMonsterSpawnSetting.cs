using JetBrains.Annotations;
using UnityEngine;

[System.Serializable] 
public struct SPhaseMonsterSpawnData
{
    [Tooltip("페이즈별 몬스터 타입")]
    public MonsterType[] spawnableMonsterTypes;

    [Tooltip("페이즈별 스폰 확률")]
    public float spawnChane;

    [Tooltip("페이즈별 청크당 최대 몬스터 수")]
    public int maxMonsterCount;
}

[CreateAssetMenu(fileName = "SOMonsterSpawnSetting", menuName = "Scriptable Objects/SOMonsterSpawnSetting")]
public class SOMonsterSpawnSetting : ScriptableObject
{
    [Header("타일맵 설정")]
    public string groundTilemapName = "Tilemap";

    [Header("페이즈별 스폰 몬스터 설정")]
    public SPhaseMonsterSpawnData[] phaseSettings = new SPhaseMonsterSpawnData[3];

    [Header("공통 설정")]
    public int minSpawnInterval = 3;
    public float bridHeightOffset = 3.5f;
}

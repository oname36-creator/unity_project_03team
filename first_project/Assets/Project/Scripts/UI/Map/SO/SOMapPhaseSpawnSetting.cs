using System.Collections.Generic;
using UnityEngine;

// 아이템 가중치 데이터
[System.Serializable]
public struct SItemWeightData
{
    public ItemData item;
    public int weight;
}
[CreateAssetMenu(fileName = "SOMapPhaseSpawnSetting", menuName = "Scriptable Objects/SOMapPhaseSpawnSetting")]
public class SOMapPhaseSpawnSetting : ScriptableObject
{
    [Header("--- 몬스터 설정 ---")]

    [Tooltip("페이즈별 몬스터 타입")]
    public MonsterType[] spawnableMonsterTypes;

    [Tooltip("페이즈별 스폰 확률")]
    public float monsterSpawnChane;

    [Tooltip("페이즈별 청크당 최대 몬스터 수")]
    public int maxMonsterCount;

    [Header("--- 박스 설정 ---")]

    [Tooltip("페이즈별 상자 스폰 확률")]
    public float boxSpawnChane;

    [Tooltip("페이즈별 상자 오픈 시 지급될 아이템 가중치")]
    public List<SItemWeightData> rewardItemsWeights;

    public int maxBoxCount;

    [Header("--- 촉수 함정 ---")]
    [Tooltip("페이즈별 촉수 함정 발동 확률")]
    [Range(0.0f, 1.0f)] public float tentacleTrapSpawnChane;

    [Tooltip("페이즈별 함정 간의 스폰 딜레이")]
    [SerializeField] public float tentacleStaggerDelay;
}

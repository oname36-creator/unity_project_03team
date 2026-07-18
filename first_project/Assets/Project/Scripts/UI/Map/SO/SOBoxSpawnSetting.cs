using System.Collections.Generic;
using UnityEngine;

// 아이템 가중치 데이터
[System.Serializable]
public struct ItemWeightData
{
    public ItemData item;

    [Tooltip("1페이즈 Drop 가중치")]
    public int phaseOWeight;

    [Tooltip("2페이즈 Drop 가중치")]
    public int phase1Weight;

    [Tooltip("3페이즈 Drop 가중치")]
    public int phase2Weight;
}

[CreateAssetMenu(fileName = "SOBoxSpawnSetting", menuName = "Scriptable Objects/SOBoxSpawnSetting")]
public class SOBoxSpawnSetting : ScriptableObject
{
    [Header("타일맵 설정")]
    public string groundTilemapName = "Tilemap";

    [Header("스폰 빈도 및 제한")]
    [Range(0f, 1f)]
    [Tooltip("상자가 스폰될 확률")]
    public float spawnChance = 0.15f;

    [Tooltip("몬스터 혹은 다른 상자와의 최소 안전간격")]
    public float minSpawnInterval = 3f;

    [Tooltip("한 청크 내에 스폰될 수 있는 최대 상자 개수")]
    public int maxBoxCount = 3;

    [Header("상자 오픈 시 보상 아이템 목록")]
    [Tooltip("상자를 획득했을 때 이 배열에 있는 아이템 데이터 중 무작위로 하나 지급")]
    public List<ItemWeightData> rewardItemsWeights;
}

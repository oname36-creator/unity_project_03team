using System.Collections.Generic;
using UnityEngine;

//// 아이템 가중치 데이터
//[System.Serializable]
//public struct ItemWeightData
//{
//    public ItemData item;

//    [Tooltip("1페이즈 Drop 가중치")]
//    public int phaseOWeight;

//    [Tooltip("2페이즈 Drop 가중치")]
//    public int phase1Weight;

//    [Tooltip("3페이즈 Drop 가중치")]
//    public int phase2Weight;
//}

[CreateAssetMenu(fileName = "SOBoxSpawnSetting", menuName = "Scriptable Objects/SOBoxSpawnSetting")]
public class SOBoxSpawnSetting : ScriptableObject
{
    [Header("타일맵 설정")]
    public string groundTilemapName = "Tilemap";

    [Header("스폰 빈도 및 제한")]
    [Tooltip("페이즈별 스폰 확률")]
    public float[] spawnChanes = new float[3] { 0.25f, 0.10f, 0.05f };

    [Tooltip("페이즈별 청크당 최대 상자 개수")]
    public int[] maxBoxCounts = new int[3] { 2, 2, 1 };

    [Tooltip("몬스터 혹은 다른 상자와의 최소 안전간격")]
    public int minSpawnInterval = 3;

    [Header("상자 오픈 시 보상 아이템 목록")]
    [Tooltip("상자를 획득했을 때 이 배열에 있는 아이템 데이터 중 무작위로 하나 지급")]
    public List<SItemWeightData> rewardItemsWeights;
}

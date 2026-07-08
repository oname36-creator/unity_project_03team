using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum EGimmickType
{
    Trap,
    Pad,
    JumpPad,
    Platform,
    Breakable
}

[System.Serializable]
public struct SGimmickSpawnData
{
    public EGimmickType gimmickType;
    public Vector3 position;
    public int prefabIndex;
}

[CreateAssetMenu(fileName = "MapData", menuName = "Scriptable Objects/SOMapData")]
public class SOMapData : ScriptableObject
{
    // 함정 위치
    [Header("함정 생성 위치 데이터")]
    public List<SGimmickSpawnData> gimmicList = new List<SGimmickSpawnData>();
}

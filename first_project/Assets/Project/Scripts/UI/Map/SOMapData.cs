using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public struct STrapSpawnData
{
    public Vector3 position;
    public int prefabIndex;
}

[CreateAssetMenu(fileName = "MapData", menuName = "Scriptable Objects/SOMapData")]
public class SOMapData : ScriptableObject
{
    // 함정 위치
    [Header("함정 생성 위치 데이터")]
    public List<STrapSpawnData> trapList = new List<STrapSpawnData>();
    public List<GameObject> chunkSequence;
}

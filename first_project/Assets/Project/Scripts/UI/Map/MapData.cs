using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct TrapSaveData
{
    public string trapName;
    public Vector3 position;
}

[CreateAssetMenu(fileName = "MapData", menuName = "Scriptable Objects/MapData")]
public class MapData : ScriptableObject
{
    // 함정 위치
    [Header("함정 생성 위치 데이터")]
    public List<TrapSaveData> trapList = new List<TrapSaveData>();
    public List<GameObject> chunkSequence;
}

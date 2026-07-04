using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "Scriptable Objects/MapData")]
public class MapData : ScriptableObject
{
    // 함정 위치
    public List<Vector3> spikePositions;
    public List<GameObject> chunkSequence;
}

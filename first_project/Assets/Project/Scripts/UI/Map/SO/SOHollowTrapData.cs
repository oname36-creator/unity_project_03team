using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct SHoolowTrapSapwnData
{
    public Vector3 position;
    // public int prefabindex;     // 다른 종류의 촉수 함정이 있을 시 사용
}

[CreateAssetMenu(fileName = "SOHollowTrapData", menuName = "Scriptable Objects/SOHollowTrapData")]
public class SOHollowTrapData : ScriptableObject
{
    [Header("촉수 함정 생성 위치 데이터")]
    public List<SHoolowTrapSapwnData> hollowTrapList = new List<SHoolowTrapSapwnData>();
}

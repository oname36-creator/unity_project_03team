using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SOMapPalette", menuName = "Scriptable Objects/SOMapPalette")]
public class SOMapPalette : ScriptableObject
{
    [Header("오브젝트 풀링에 사용할 원본 프리팹들")]
    public List<GameObject> chunkPrefabs;
}

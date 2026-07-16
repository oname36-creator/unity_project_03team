using System;
using UnityEngine;

public static class MapEvent
{
    public static Action onPlayerHitSpawnTrigger;
    // 이름, 월드좌표, 객체 반환
    public static Action<string, Vector3, Action<GameObject>> onRequestMonsterSpawn;
}

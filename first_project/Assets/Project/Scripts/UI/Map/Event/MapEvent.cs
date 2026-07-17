using System;
using UnityEngine;

public static class MapEvent
{
    public static Action onPlayerHitSpawnTrigger;

    // 이름, 월드좌표, 객체 반환(몬스터 스폰)
    public static Action<string, Vector3, Action<GameObject>> onRequestMonsterSpawn;

    // 촉수 트랩 생성(좌표, 콜백)
    public static Action<Vector2, Action<GameObject>> onRequestTrapSpawn;
}

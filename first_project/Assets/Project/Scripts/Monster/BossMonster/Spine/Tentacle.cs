using UnityEngine;

public class Tentacle : MonoBehaviour
{
    [Header("Attachment Data")]
    public int attachNodeIndex; // 서브 척추의 몇 번째 마디에 붙을 것인가?

    // SubSpine이 호출해 줍니다.
    public void UpdateTentacleProcess(Vector3 basePosition)
    {
        // 1. 시작점 고정
        // myIK.SetBasePosition(basePosition);

        // 2. 촉수 특유의 꿈틀거리는 IK 연산 (Perlin Noise 활용)
        // Vector3 noiseTarget = GenerateNoiseTarget();
        // myIK.ResolveIK(noiseTarget);
    }
}
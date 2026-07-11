using UnityEngine;
using System.Collections.Generic;

public class MainSpine : MonoBehaviour
{
    [Header("Sub Spines")]
    public List<SubSpine> subSpines = new List<SubSpine>();

    [Header("IK Settings")]
    public int solverIterations = 3; // 루프를 닫기 위한 IK 반복 연산 횟수

    // BossController에서 계산된 목표 위치(보스 머리/중심)를 받아옵니다.
    public void UpdateSpineProcess(Vector3 targetPosition)
    {
        //Debug.Log(targetPosition);
        if (subSpines == null || subSpines.Count == 0) return;

        // FABRIK 알고리즘 적용: 반복 연산을 통해 체인의 시작과 끝을 자연스럽게 닫습니다.
        for (int iteration = 0; iteration < solverIterations; iteration++)
        {
            // 1. 역방향 연산 (Backward Pass)
            // 9번 선의 끝이 0번 선의 시작(targetPosition)에 닿도록 거꾸로 계산합니다.
            Vector3 currentTarget = targetPosition;
            for (int i = subSpines.Count - 1; i >= 0; i--)
            {
                if (subSpines[i] == null) continue;
                subSpines[i].UpdateBackward(currentTarget);

                // 다음 역방향 타겟은 방금 계산한 선의 시작점
                currentTarget = subSpines[i].StartPoint;
            }

            // 2. 정방향 연산 (Forward Pass)
            // 0번 선의 시작을 다시 본래 목표 위치(targetPosition)에 고정하고 정방향으로 재정렬합니다.
            currentTarget = targetPosition;
            for (int i = 0; i < subSpines.Count; i++)
            {
                if (subSpines[i] == null) continue;
                subSpines[i].UpdateForward(currentTarget);

                // 다음 정방향 타겟은 방금 계산한 선의 끝점
                currentTarget = subSpines[i].EndPoint;
            }
        }

        // 형태가 모두 안정화된 후, 자식 촉수(Tentacle)들을 일괄 갱신합니다.
        for (int i = 0; i < subSpines.Count; i++)
        {
            if (subSpines[i] != null)
            {
                subSpines[i].UpdateTentacles();
            }
        }
    }
}
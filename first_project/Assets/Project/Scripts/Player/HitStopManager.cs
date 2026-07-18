using UnityEngine;
using System.Collections;

public class HitStopManager : MonoBehaviour
{
    private bool isWaiting = false;

    public void TriggerHitStop(float duration)
    {
        // 이미 히트 스톱이 작동 중이라면 중복 실행 방지
        if (isWaiting) return;

        StartCoroutine(HitStopRoutine(duration));
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        isWaiting = true;

        // 1. 게임 글로벌 시간 정지
        Time.timeScale = 0f;

        // 2. 실시간(현실 시간) 기준으로 지정된 초만큼 대기
        yield return new WaitForSecondsRealtime(duration);

        // 3. 게임 시간 복구
        Time.timeScale = 1f;

        isWaiting = false;
    }
}
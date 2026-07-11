using System.Collections; // 코루틴(IEnumerator)을 사용하기 위해 필수입니다.
using UnityEngine;

public class BodyBackGround : MonoBehaviour
{
    private Vector3 _originalScale; // 원래 크기 저장용
    private Vector3 _targetScale;   // 커졌을 때의 크기 저장용

    public float _duration = 2.0f;  // 크기가 변하는 데 걸리는 시간 (2초 간격)

    void Start()
    {
        // 시작할 때 원래 크기를 저장합니다.
        _originalScale = transform.localScale;

        // 원래 크기에서 0.5배(50%) 더 커진 크기 (즉, 1.5배)
        _targetScale = _originalScale * 1.5f;

        // 코루틴 실행
        StartCoroutine(PulseScale());
    }

    // 커졌다 작아지는 동작을 무한히 반복하는 코루틴
    IEnumerator PulseScale()
    {
        while (true)
        {
            // 1. 2초 동안 원래 크기 -> 목표 크기로 커지기
            yield return StartCoroutine(ScaleOverTime(_originalScale, _targetScale, _duration));

            // 2. 2초 동안 목표 크기 -> 원래 크기로 작아지기
            yield return StartCoroutine(ScaleOverTime(_targetScale, _originalScale, _duration));
        }
    }

    // 지정된 시간(time) 동안 시작 크기에서 목표 크기로 부드럽게 변경하는 코루틴
    IEnumerator ScaleOverTime(Vector3 startScale, Vector3 endScale, float time)
    {
        float currentTime = 0.0f;

        while (currentTime < time)
        {
            currentTime += Time.deltaTime; // 프레임 간의 시간 누적

            // Vector3.Lerp를 사용하여 부드럽게 크기 전환
            transform.localScale = Vector3.Lerp(startScale, endScale, currentTime / time);

            yield return null; // 다음 프레임까지 대기
        }

        // 루프가 끝난 후, 오차가 생기지 않도록 정확한 목표 크기로 고정해줍니다.
        transform.localScale = endScale;
    }
}
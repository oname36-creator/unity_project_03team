using System.Collections;
using UnityEngine;

public class FlyMonsterReturn : IMonsterState
{
    private MonsterController _owner;
    private Coroutine _returnCoroutine;

    private Vector2 _hitPos; // 타격했던 위치 (현재 위치)
    private Vector2 _originalStartPos; // 공격을 시작했던 원래 위치
    private Vector2 _endPos; // 최종 도착 지점
    private float _duration;
    private float _curveOffset = -1.0f; // Attack과 동일한 값 사용

    // 생성자에서 원래 시작 위치를 받아와서 대칭 도착점을 계산합니다.
    public FlyMonsterReturn(MonsterController owner)
    {
        this._owner = owner;
    }

    public void Enter()
    {
        _owner.IsAttack = false;

        _originalStartPos = _owner.AttackStartPoint;

        _hitPos = _owner.transform.position;

        // 도착 지점 계산 (X는 타격점 기준으로 원래 시작점의 반대편, Y는 원래 몬스터 비행 고도)
        _endPos = new Vector2(_hitPos.x + (_hitPos.x - _originalStartPos.x), _originalStartPos.y);

        float distance = Vector2.Distance(_hitPos, _endPos);
        _duration = distance / _owner.Speed;

        Debug.Log($"[복귀 상태 진입] 타격점: {_hitPos}, 도착: {_endPos}");

        _returnCoroutine = _owner.StartCoroutine(ReturnRoutine());
    }

    public void Update() { }

    public void Exit()
    {
        if (_returnCoroutine != null)
        {
            _owner.StopCoroutine(_returnCoroutine);
            _returnCoroutine = null;
        }
        _owner.IsAttack = false;
    }

    private IEnumerator ReturnRoutine()
    {
        float timePassed = 0f;
        Vector2 controlPoint = Vector2.Lerp(_hitPos, _endPos, 0.5f) + new Vector2(0, _curveOffset);

        while (timePassed < _duration)
        {
            timePassed += Time.fixedDeltaTime;
            float t = timePassed / _duration;

            Vector2 nextPos = Mathf.Pow(1 - t, 2) * _hitPos +
                              2 * (1 - t) * t * controlPoint +
                              Mathf.Pow(t, 2) * _endPos;

            _owner.MoveToPosition(nextPos);
            yield return new WaitForFixedUpdate();
        }

        // 최종 도착 지점 오차 보정
        _owner.MoveToPosition(_endPos);

        _returnCoroutine = null;
        _owner.IsBack = true;
    }
}
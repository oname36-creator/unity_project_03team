using System.Collections;
using UnityEngine;

public class FlyMonsterAttack : IMonsterState
{
    private MonsterController _owner;
    private Coroutine _attackCoroutine;

    private Vector2 _originalStartPos;
    private Vector2 _playerPos;
    private float _duration;
    private float _curveOffset = -1.0f;

    private float _timePassed = 0f;
    private bool _isPaused = false; // 일시정지(Hurt) 후 복귀인지 체크

    public FlyMonsterAttack(MonsterController owner)
    {
        this._owner = owner;
    }

    public void Enter()
    {
        _owner.SetExclamationMark(true);
        _owner.SetQuestionMark(false);
        _owner.IsAttack = true;

        if (_isPaused)
        {
            Debug.Log($"<color=orange>[Attack 재개]</color> 멈췄던 위치: {_owner.transform.position} | 남은 시간: {_duration - _timePassed}초");
            _isPaused = false;
            _attackCoroutine = _owner.StartCoroutine(AttackRoutine());
            return;
        }

        _owner.Stop();
        _timePassed = 0f; // 진행 시간 초기화
        _originalStartPos = _owner.transform.position;
        _owner.AttackStartPoint = _originalStartPos;
        _playerPos = _owner.GetPlayerPos;

        float distance = Vector2.Distance(_originalStartPos, _playerPos);
        _duration = distance / _owner.MaxSpeed;
        Debug.Log($"<color=cyan>[Attack 최초 진입]</color> 시작점: {_originalStartPos} | 최종 목표점(플레이어): {_playerPos} | 총 비행시간: {_duration}초");
        _attackCoroutine = _owner.StartCoroutine(AttackRoutine());
    }

    public void Update() { }

    public void Exit()
    {
        if (_attackCoroutine != null)
        {
            _owner.StopCoroutine(_attackCoroutine);
            _attackCoroutine = null;
            _isPaused = true; // 다친 상태로 나갔으므로 일시정지 True
            Debug.Log($"<color=red>[Attack 중단 (Hurt)]</color> 강제 정지된 위치: {_owner.transform.position} | 목표까지 남은 거리: {Vector2.Distance(_owner.transform.position, _playerPos)}");
        }
        else
        {
            _isPaused = false;
            Debug.Log($"<color=green>[Attack 완료]</color> 최종 도달 위치: {_owner.transform.position} (오차: {Vector2.Distance(_owner.transform.position, _playerPos)})");
        }
    }

    private IEnumerator AttackRoutine()
    {
        Vector2 controlPoint = Vector2.Lerp(_originalStartPos, _playerPos, 0.5f) + new Vector2(0, _curveOffset);

        // 디버깅용: 시작점, 제어점, 목표점을 눈으로 확인하기 위한 선 (파란색 계열)
        Debug.DrawLine(_originalStartPos, controlPoint, Color.blue, 2.0f);
        Debug.DrawLine(controlPoint, _playerPos, Color.cyan, 2.0f);

        while (_timePassed < _duration)
        {
            _timePassed += Time.fixedDeltaTime;
            float t = _timePassed / _duration;

            Vector2 nextPos = Mathf.Pow(1 - t, 2) * _originalStartPos +
                              2 * (1 - t) * t * controlPoint +
                              Mathf.Pow(t, 2) * _playerPos;
            // [디버그 1] 현재 위치 저장
            Vector2 currentPos = _owner.transform.position;

            _owner.MoveToPosition(nextPos);
            // [디버그 2] 씬 뷰에서 이동 궤적을 빨간 선으로 그리기 (2초 동안 유지됨)
            Debug.DrawLine(currentPos, nextPos, Color.red, 2.0f);

            // [디버그 3] (선택사항) 특정 구간마다 텍스트 로그 출력 (프레임 저하 방지를 위해 10% 단위로 출력)
            if (Mathf.RoundToInt(t * 100) % 10 == 0)
            {
                 Debug.Log($"[이동 중 {Mathf.RoundToInt(t * 100)}%] 계산된 다음 위치: {nextPos} | 몬스터 실제 위치: {_owner.transform.position}");
            }

            yield return new WaitForFixedUpdate();
        }

        _owner.MoveToPosition(_playerPos);
        _owner.IsAttack = false;
        _attackCoroutine = null;
        _isPaused = false; // 공격이 완전히 끝났으므로 초기화
    }
}
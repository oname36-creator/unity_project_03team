using System.Collections;
using UnityEngine;

public class FlyMonsterAttack : IMonsterState
{
    private MonsterController _owner;
    private Coroutine _attackCoroutine;

    private Vector2 _originalStartPos;
    private Vector2 _playerPos;
    private float _duration;
    private float _curveOffset = -1.5f; // 곡선의 휨 정도 (양수면 위로 도약 후 강하)

    public FlyMonsterAttack(MonsterController owner)
    {
        this._owner = owner;
    }

    public void Enter()
    {
        if (_owner.IsAttack) return;

        _owner.SetExclamationMark(true);
        _owner.SetQuestionMark(false);

        _owner.IsAttack = true;
        _owner.Stop();


        _originalStartPos = _owner.transform.position;
        
        _owner.AttackStartPoint = _originalStartPos;

        _playerPos = _owner.GetPlayerPos;

        // 비행 시간 계산

        float distance = Vector2.Distance(_originalStartPos, _playerPos);

        // 최대 속도로 공격
        _duration = distance / _owner.MaxSpeed;

        Debug.Log($"[공격 상태 진입] 시작: {_originalStartPos}, 목표: {_playerPos}");

        _attackCoroutine = _owner.StartCoroutine(AttackRoutine());
    }

    public void Update() { }

    public void Exit()
    {
        if (_attackCoroutine != null)
        {
            _owner.StopCoroutine(_attackCoroutine);
            _attackCoroutine = null;
        }
        // IsAttack = false는 복귀 상태(Return)가 완전히 끝났을 때 해제하는 것이 안전합니다.
    }

    private IEnumerator AttackRoutine()
    {
        float timePassed = 0f;
        Vector2 controlPoint = Vector2.Lerp(_originalStartPos, _playerPos, 0.5f) + new Vector2(0, _curveOffset);

        while (timePassed < _duration)
        {
            timePassed += Time.fixedDeltaTime;
            float t = timePassed / _duration;

            Vector2 nextPos = Mathf.Pow(1 - t, 2) * _originalStartPos +
                              2 * (1 - t) * t * controlPoint +
                              Mathf.Pow(t, 2) * _playerPos;

            _owner.MoveToPosition(nextPos);
            yield return new WaitForFixedUpdate();
        }

        // 오차 보정: 정확히 플레이어 위치로 착지
        _owner.MoveToPosition(_playerPos);

        _owner.IsAttack = false;

        _attackCoroutine = null;
    }
}
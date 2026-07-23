using System.Collections;
using UnityEngine;

public class FlyMonsterReturn : IMonsterState
{
    private MonsterController _owner;
    private Coroutine _returnCoroutine;

    private Vector2 _hitPos;
    private Vector2 _originalStartPos;
    private Vector2 _endPos;
    private float _duration;
    private float _curveOffset = -1.0f;


    private float _timePassed = 0f;
    private bool _isPaused = false;

    public FlyMonsterReturn(MonsterController owner)
    {
        this._owner = owner;
    }

    public void Enter()
    {
        _owner.IsAttack = false;


        if (_isPaused)
        {
            _isPaused = false;
            _returnCoroutine = _owner.StartCoroutine(ReturnRoutine());
            return;
        }

        // --- 완전 최초 진입 시 계산 ---
        _timePassed = 0f;
        _originalStartPos = _owner.AttackStartPoint;
        _hitPos = _owner.transform.position;

        _endPos = new Vector2(_hitPos.x + (_hitPos.x - _originalStartPos.x), _originalStartPos.y);

        float distance = Vector2.Distance(_hitPos, _endPos);
        _duration = distance / _owner.MaxSpeed;

        _returnCoroutine = _owner.StartCoroutine(ReturnRoutine());
    }

    public void Update() { }

    public void Exit()
    {
        if (_returnCoroutine != null)
        {
            _owner.StopCoroutine(_returnCoroutine);
            _returnCoroutine = null;
            _isPaused = true; // 다친 상태로 나갔으므로 일시정지 True
        }
        else
        {
            _isPaused = false;
        }
    }

    private IEnumerator ReturnRoutine()
    {
        Vector2 controlPoint = Vector2.Lerp(_hitPos, _endPos, 0.5f) + new Vector2(0, _curveOffset);

        while (_timePassed < _duration)
        {
            _timePassed += Time.fixedDeltaTime;
            float t = _timePassed / _duration;

            Vector2 nextPos = Mathf.Pow(1 - t, 2) * _hitPos +
                              2 * (1 - t) * t * controlPoint +
                              Mathf.Pow(t, 2) * _endPos;

            _owner.MoveToPosition(nextPos);
            yield return new WaitForFixedUpdate();
        }

        _owner.MoveToPosition(_endPos);
        _returnCoroutine = null;
        _owner.IsBack = true;
        _isPaused = false; // 리턴 완료
    }
}
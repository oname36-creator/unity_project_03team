using UnityEngine;

public class TentacleStretch : IMonsterState
{
    private TentacleController _owner;
    private GameObject _target;
    private Transform _targetTransform;
    private Vector2 _originalTargetPosition;
    private Vector2 _finalAttackPosition;

    private float _attackRangeX = 2f;
    private float _attackRangeY = 2f;
    private float _reachThreshold = 0.5f;  // 목표 지점에 도달했는지 판단하는 거리

    public TentacleStretch(TentacleController owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        _target = _owner.Target;
        if (_target == null)
        {
            _owner.IsSearch = false; // 타겟이 없으면 바로 회수
            return;
        }

        _targetTransform = _target.transform;

        // 타겟의 현재 위치 저장
        _originalTargetPosition = _targetTransform.position;

        // 기준 위치에서 랜덤한 공격 위치(목표점) 계산 
        _finalAttackPosition = _originalTargetPosition;
        _finalAttackPosition.x += Random.Range(-_attackRangeX, _attackRangeX);
        _finalAttackPosition.y += Random.Range(-_attackRangeY, _attackRangeY);

        // 촉수의 IK 타겟을 최종 공격 위치로 설정 (발사)
        _owner.IkTargetPosition = _finalAttackPosition;

        //Debug.Log($"TentacleStretch: 발사! 목표({_originalTargetPosition}) -> 랜덤 타겟({_finalAttackPosition})");
    }

    public void Update()
    {
        // 1. 타겟이 사라졌을 경우 회수
        if (_target == null)
        {
            _owner.IsSearch = false;
            return;
        }

        // 2. 이미 TentacleGrabber의 물리 충돌로 대상을 잡았다면 (IsAttach == true)
        // 뻗는 동작의 Update 처리를 멈추고 StateMachine이 Attach 상태로 전환해주길 대기
        if (_owner.IsAttach)
        {
            return;
        }

        // 3. 아직 못 잡은 상태라면, 촉수 끝이 랜덤 공격 위치에 도달했는지 확인
        float sqrDistanceToFinalTarget = (_owner.GetGrabber.position - (Vector3)_finalAttackPosition).sqrMagnitude;

        if (sqrDistanceToFinalTarget < _reachThreshold * _reachThreshold)
        {

            //Debug.Log("TentacleStretch: 뻗었지만 아무것도 닿지 않음... 회수 시작");

            _owner.Boss.RemoveTarget(_owner.Target); // 명부에서 삭제
            _owner.IsSearch = false;
            _owner.Target = null;
        }
    }

    public void Exit()
    {
    }
}
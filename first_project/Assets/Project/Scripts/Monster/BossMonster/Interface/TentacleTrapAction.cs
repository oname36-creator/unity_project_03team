using UnityEngine;

public class TentacleTrapAction : IMonsterState
{


    private TentacleController _owner;

    private Vector2 _rootPos;
    private Vector2 _targetPos;

    private float _reachThreshold = 0.5f;

    public TentacleTrapAction(TentacleController owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        _rootPos = _owner.RootPos;
        _targetPos = _rootPos;
        _targetPos.y += 20f;

        _owner.UpdateSegmentLength(20);
        _owner.segmentDistance = 1f;
        _owner.IkTargetPosition = _targetPos; 
    }

    public void Update()
    {
        if (_owner.IsAttach)
        {
            return;
        }

        // 3. 아직 못 잡은 상태라면, 촉수 끝이 랜덤 공격 위치에 도달했는지 확인
        float distanceToFinalTarget = Vector2.Distance(_rootPos, _targetPos);

        if (distanceToFinalTarget < _reachThreshold)
        {


            _owner.Boss.RemoveTarget(_owner.Target); // 명부에서 삭제
            _owner.Attack = false;
        }

    }

    public void Exit()
    {

    }
}

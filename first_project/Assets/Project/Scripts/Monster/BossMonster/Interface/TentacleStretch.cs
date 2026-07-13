using UnityEngine;

public class TentacleStretch : IMonsterState
{
    private TentacleController _owner;
    private Transform _targetTransform;
    private Vector2 _targetPosition;

    public TentacleStretch(TentacleController owner)
    {
        this._owner = owner;
    }

    public void Enter()
    {
        _targetTransform = _owner.Boss.Target; // 플레이어 등
        UpdateTargetPosition();
        Debug.Log("TentacleStretch");
    }

    public void Update()
    {
        // 매 프레임 타겟이 움직인다면 위치를 갱신
        UpdateTargetPosition();

        // Owner의 IK 타겟에 주입 (TentacleController가 알아서 부드럽게 이동시킴)
        _owner.IkTargetPosition = _targetPosition;
    }

    public void Exit()
    {
        
    }

    private void UpdateTargetPosition()
    {
        if (_targetTransform == null) return;

        _targetPosition = _targetTransform.position;
        if (!_owner.IsAttach)
        {
            // 랜덤한 위치로 약간 빗나가게 공격하거나 위협하는 연출
            _targetPosition.x += Random.Range(-2f, 2f);
            _targetPosition.y += Random.Range(-2f, 2f); 
        }
    }
}
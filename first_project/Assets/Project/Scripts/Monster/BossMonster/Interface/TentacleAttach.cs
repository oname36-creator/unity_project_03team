using UnityEngine;

public class TentacleAttach : IMonsterState
{
    TentacleController _owner;
    Transform _grabberTransform;
    Transform _targetTransform;

    public TentacleAttach(TentacleController owner)
    {
        _owner = owner;
        _grabberTransform = _owner.GetGrabber;
    }

    public void Enter()
    {
        _targetTransform = _owner.Boss.Target;
        Debug.Log("TentacleAttach");
    }

    public void Update()
    {
        if (_targetTransform != null)
        {
            _owner.IkTargetPosition = _targetTransform.position;
        }
    }

    public void Exit()
    {
    }
}
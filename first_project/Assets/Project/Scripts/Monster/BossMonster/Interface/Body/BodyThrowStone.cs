using UnityEngine;
using System.Collections;

public class BodyThrowStone : IMonsterState
{
    private BodyController _owner;

    private Transform _playerTransform;

    private Vector3 _targetPosition;

    private Coroutine _throwCoroutine;

    public BodyThrowStone(BodyController owner)
    {
        _owner = owner;
        _playerTransform = _owner.Boss.Player.transform;

        _targetPosition = Vector3.zero;
    }

    public void Enter()
    {
        if (_throwCoroutine != null || _playerTransform.position.y < -10)
        {
            _owner.Throw = false;
            return;
        }

        _throwCoroutine = _owner.StartCoroutine(ThrowRoutine());
    }

    public void Update()
    {
    }

    public void Exit()
    {

    }
    private IEnumerator ThrowRoutine()
    {
        yield return new WaitForSeconds(_owner.ThrowCycle);


        _targetPosition = _playerTransform.position;
        _targetPosition.x += 20f;


        if (_targetPosition != Vector3.zero)
        {
            GameObject thrown = ObjectPoolManager.Instance.ThrownPop();
            thrown.GetComponent<BeingThrown>().InitializeThrow(_targetPosition);
        }
    }
}
using UnityEngine;
using System.Collections;

public class BodyThrowStone : IMonsterState
{
    private BodyController _owner;

    private Transform _playerTransform;

    private Vector3 _targetPosition;

    private float _time = 15f;

    private Coroutine _throwCoroutine;

    public BodyThrowStone(BodyController owner)
    {
        _owner = owner;
        _playerTransform = _owner.Boss.Player.transform;
        _targetPosition = Vector3.zero;
    }

    public void Enter()
    {
        if (_throwCoroutine != null)
        {
            _owner.Throw = false;
            return;
        }
        Debug.Log("Throw");
        _throwCoroutine = _owner.StartCoroutine(ThrowRoutine());
    }

    public void Update()
    {
    }

    public void Exit()
    {

    }
    private void GetGroundObjectRight()
    {
        Vector2 origin = (Vector2)_playerTransform.position + Vector2.up * 1.0f;
        Vector2 direction = ((Vector2)_playerTransform.right + Vector2.down).normalized;

        float maxDistance = 50f;
        int groundLayerMask = 1 << LayerMask.NameToLayer("Ground");


        Debug.DrawRay(origin, direction * maxDistance, Color.red, 3f);


        RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxDistance, groundLayerMask);

        if (hit.collider != null)
        {
            GameObject hitGroundObj = hit.collider.gameObject;
            _targetPosition = hitGroundObj.transform.position;
        }
        else
        {
            _targetPosition = Vector3.zero;
        }
    }

    private IEnumerator ThrowRoutine()
    {
        yield return new WaitForSeconds(_owner.ThrowCycle);
        GetGroundObjectRight();

        if (_targetPosition != Vector3.zero)
        {
            GameObject thrown = ObjectPoolManager.Instance.ThrownPop();
            if (thrown != null)
            {
                thrown.GetComponent<BeingThrown>().InitializeThrow(_targetPosition);
            }
        }

        _owner.Throw = true;

        _throwCoroutine = null;
    }

}

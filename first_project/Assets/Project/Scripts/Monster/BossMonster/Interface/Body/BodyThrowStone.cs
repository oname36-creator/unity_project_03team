using UnityEngine;
using System.Collections;

public class BodyThrowStone : IMonsterState
{
    private BodyController _owner;

    private Transform _playerTransform;

    private Vector3 _targetPosition;

    public BodyThrowStone(BodyController owner)
    {
        _owner = owner;
        _playerTransform = _owner.Boss.Player.transform;
        _targetPosition = Vector3.zero; 
    }

    public void Enter()
    {
        GetGroundObjectRight();
    }

    public void Update()
    {

    }

    public void Exit()
    {

    }
    private void GetGroundObjectRight()
    {
        Vector3 origin = _playerTransform.position + Vector3.up * 1.0f;


        Vector3 direction = (_playerTransform.right + Vector3.down).normalized;

        float maxDistance = 50f;
        int groundLayerMask = 1 << LayerMask.NameToLayer("Ground");

        RaycastHit hit;

        Debug.DrawRay(origin, direction * maxDistance, Color.red, 3f);

        if (Physics.Raycast(origin, direction, out hit, maxDistance, groundLayerMask))
        {
            GameObject hitGroundObj = hit.collider.gameObject;

            _targetPosition = hitGroundObj.transform.position;

        }
    }
}

using UnityEngine;

public class BodyIdle : IMonsterState
{
    private BodyController _owner;
    private Transform _ownerTransform;

    private Transform _targetTransform;

    // 생성자에서 owner를 직접 받도록 셋업
    public BodyIdle(BodyController owner)
    {
        this._owner = owner;
        _ownerTransform = _owner.GetComponent<Transform>();


    }

    public void Enter()
    {
        Debug.Log("BodyIdle");
    }

    public void Update()
    {


    }

    public void Exit()
    {

    }

}

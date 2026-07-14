using UnityEngine;

public class BodyCreateTentacle : IMonsterState
{

    private BodyController _owner;
    private GameObject _tentacle;
    private TentacleController _tentacleController;

    public BodyCreateTentacle(BodyController owner) 
    {
        _owner = owner;
    }


    public void Enter()
    {
        _tentacle = ObjectPoolManager.Instance.TentaclePop();
        _tentacle.SetActive(true);
        _tentacleController = _tentacle.GetComponent<TentacleController>();
        _tentacleController.IsDead = false;

        _tentacleController.IsAttackTentacle = _owner.Boss.IsAttackTentacle; //  공격용 촉수로
        Debug.Log("IsAttackTentacle : " + _tentacleController.IsAttackTentacle);
    }

    public void Update()
    {

        _tentacleController.IsAttackTentacle = _owner.Boss.IsAttackTentacle;
        _owner.Create = false;
        
    }

    public void Exit()
    {

    }
}

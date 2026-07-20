using System.Collections;
using UnityEngine;

public class BodyCreateTentacle : IMonsterState
{
    private BodyController _owner;

    public BodyCreateTentacle(BodyController owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        _owner.StartCoroutine(CreateTentacleRoutine());

    }

    public void Update()
    {

    }

    public void Exit()
    {

    }

    private IEnumerator CreateTentacleRoutine()
    {
        GameObject tentacle = ObjectPoolManager.Instance.TentaclePop();
        tentacle.SetActive(true);

        TentacleController tentacleController = tentacle.GetComponent<TentacleController>();
        tentacleController.IsDead = false;
        tentacleController.IsAttackTentacle = _owner.Boss.IsAttackTentacle;

        Debug.Log("IsAttackTentacle : " + tentacleController.IsAttackTentacle);

        
        yield return new WaitUntil(() => !tentacleController.IsAttackTentacle);


        tentacleController.IsAttackTentacle = true;
        _owner.Create = false;
    }
}
using System.Collections;
using UnityEngine;

public class BodyCreateTentacle : IMonsterState
{
    private BodyController _owner;
    private Coroutine _createCoroutine;


    public BodyCreateTentacle(BodyController owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        if (_createCoroutine != null)
        {
            _owner.Create = false;
            return;
        }

        _createCoroutine = _owner.StartCoroutine(CreateTentacleRoutine());
    }

    public void Update()
    {

    }

    public void Exit()
    {
        if (_createCoroutine != null)
        {
            _owner.StopCoroutine(_createCoroutine);
            _createCoroutine = null;
        }
    }

    private IEnumerator CreateTentacleRoutine()
    {
        GameObject tentacle = ObjectPoolManager.Instance.TentaclePop();
        tentacle.SetActive(true);

        TentacleController tentacleController = tentacle.GetComponent<TentacleController>();
        tentacleController.IsDead = false;
        tentacleController.IsAttackTentacle = true;

        Debug.Log("IsAttackTentacle : " + tentacleController.IsAttackTentacle);

        
        yield return null;

        _owner.Create = false;
    }
}
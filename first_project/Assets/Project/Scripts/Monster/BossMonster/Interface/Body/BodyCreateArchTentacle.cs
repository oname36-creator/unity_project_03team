using System.Collections;
using UnityEngine;

public class BodyCreateArchTentacle : IMonsterState
{
    private BodyController _owner;
    private Coroutine _createCoroutine;

    public BodyCreateArchTentacle(BodyController owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        //Debug.Log("BodyCreateArch");
        if (_createCoroutine != null)
        {
            _owner.CreateArch = false;
            return;
        }

        _createCoroutine = _owner.StartCoroutine(CreateArchTentacleRoutine());
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

    private IEnumerator CreateArchTentacleRoutine()
    {
        GameObject tentacle = ObjectPoolManager.Instance.TentaclePop();
        tentacle.SetActive(true);

        TentacleController tentacleController = tentacle.GetComponent<TentacleController>();
        tentacleController.IsDead = false;
        
        // Arch 패턴용 세팅
        tentacleController.isArch = true;
        
        // 생성 후 바로 탈출
        _owner.CreateArch = false;
        _createCoroutine = null;
        yield return null;

        _createCoroutine = null;
    }
}

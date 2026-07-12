using UnityEngine;

public class BodyMove : IMonsterState
{


    private BodyController _owner;
    private Transform _ownerTransform;
    private Rigidbody2D _rigidbody2D;

    private Transform _targetTransform;


    // 생성자에서 owner를 직접 받도록 셋업
    public BodyMove(BodyController owner)
    {
        this._owner = owner;
        _ownerTransform = _owner.GetComponent<Transform>();


    }

    public void Enter()
    {
        _targetTransform = _owner.Boss.Target;
        Debug.Log("BodyMove");
    }

    public void Update()
    {
        // 1. 이동 방향 계산
        Vector2 direction = ((Vector2)_targetTransform.position - _rigidbody2D.position).normalized;

        // 2. 촉수가 벽에 붙어있다고 가정하고 본체를 끌어당기는 물리적 힘 적용
        // 실제 게임에서는 "벽에 부착된 촉수들의 방향 벡터 평균"을 구해서 힘을 주는 것이 가장 자연스럽습니다.
        _rigidbody2D.AddForce(direction * _owner.PullForce * Time.deltaTime, ForceMode2D.Force);

    }

    public void Exit()
    {
        
    }

}

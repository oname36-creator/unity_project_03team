using UnityEngine;

public class TentacleAttach : IMonsterState
{
    private TentacleController _owner;
    private Transform _grabberTransform;

    private GameObject _target;
    private Transform _targetTransform;
    private Rigidbody2D _targetRb;

    // 타겟의 원래 BodyType을 기억해두는 변수 (2D 방식)
    private RigidbodyType2D _originalBodyType;

    public TentacleAttach(TentacleController owner)
    {
        _owner = owner;
        _grabberTransform = _owner.GetGrabber;
    }

    public void Enter()
    {
        _target = _owner.Target;

        if (_target != null)
        {
            _targetTransform = _target.transform;
            _targetRb = _target.GetComponent<Rigidbody2D>();

            // Rigidbody2D 제어권 뺏기 (bodyType 변경)
            if (_targetRb != null)
            {
                _originalBodyType = _targetRb.bodyType;              // 원래 상태 저장
                _targetRb.bodyType = RigidbodyType2D.Kinematic;      // 강제로 물리 끄기 (Kinematic으로 변경)
            }
        }

        //Debug.Log("TentacleAttach: 잡았다! 몸통으로 끌어오기 시작");
    }

    public void Update()
    {
        if (_targetTransform == null)
        {
            _owner.IsAttach = false;
            return;
        }


        Vector2 root = _owner.isTrap ? _owner.RootPos : _owner.tentacleRoot.position;

        _owner.IkTargetPosition = root;
        _targetTransform.position = _grabberTransform.position;


        float distanceToRoot = Vector2.Distance(_grabberTransform.position, root);
        if (distanceToRoot < 1.0f)
        {
            //Debug.Log("몸통까지 끌고 오기 완료! 데미지 처리 등 실행");

            // TODO: 데미지 처리
            _owner.Boss.RemoveTarget(_owner.Target);
            _owner.IsSearch = false;
            _owner.IsAttach = false;
            _owner.Target = null;

            _owner.PrevSegmentLength += 1;

        }
    }

    public void Exit()
    {
        // 상태를 빠져나갈 때 타겟이 살아있다면 원래의 BodyType으로 원상복구
        if (_targetRb != null)
        {
            _targetRb.bodyType = _originalBodyType;
        }

        if (_owner.IsAttackTentacle) 
        {
            _owner.IsAttackTentacle = false;
        }

        if (_owner.isTrap) 
        {
            _owner.isTrap = false;
            ObjectPoolManager.Instance.TentaclePush(_owner.gameObject);
        }


    }
}
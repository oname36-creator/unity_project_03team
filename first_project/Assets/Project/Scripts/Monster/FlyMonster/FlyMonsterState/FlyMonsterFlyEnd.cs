using Unity.VisualScripting;
using UnityEngine;


public class FlyMonsterFlyEnd : IMonsterState
{
    private MonsterController _owner;
    private Animator _animator;

    private GameObject[] seats;
    private float closestDistance;

    private Vector2 _myPos;
    private Vector2 _landPos;
    private Vector2 _upPos;
    private Vector2 _pivot;
    private Vector2 _moveDir;

    private float _radius;
    private float _radian;
    private float _delta;

    private bool _landing;

    // 생성자에서 owner를 직접 받도록 셋업
    public FlyMonsterFlyEnd(MonsterController owner)
    {
        this._owner = owner;
        _animator = _owner.GetComponent<Animator>();
        
    }
    public void Enter()
    {
        _landing = false;
        _myPos = _owner.GetComponent<Transform>().position;
        seats = GameObject.FindGameObjectsWithTag("LandingPoint");
        closestDistance = Mathf.Infinity;


        foreach (GameObject seat in seats)
        {
            Vector2 landPos = seat.transform.position;
            if(Vector2.Dot(_owner.Front, landPos - _myPos) < 0) 
            { 
                continue; 
            }

            float distance = Vector2.Distance(_myPos, landPos);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                _landPos = landPos;
            }
        }

        _upPos.x = _landPos.x - (_landPos.x - _myPos.x)/2;
        _upPos.y = _landPos.y + 10;

        float distanceX = Mathf.Abs(_upPos.x - _myPos.x);
        float distanceY = Mathf.Abs(_upPos.y - _myPos.y);

        _pivot.x = _upPos.x;
        _upPos.y = Mathf.Abs(_upPos.y);
        _pivot.y = _upPos.y + ((distanceX) * (distanceX) + (distanceY) * (distanceY)) / (2 * distanceY);

        _radius = _pivot.y - _upPos.y;

        // cos 값
        _radian = Vector2.Dot(Vector2.Normalize(_upPos - _pivot), Vector2.Normalize(_myPos - _pivot));
        // arccos 
        _radian = Mathf.Acos(_radian);

        if (_radian < 0)
        {
            _delta = 0.01f;
        }
        else
        {
            _delta = -0.01f;
        }

    }

    public void Update()
    {
        if (_landing) 
        {
            _owner.Move(_landPos - _upPos);
            _owner.IsFly = false;
        }


        _radian += _delta;
        _moveDir.x = _radius * Mathf.Cos(_radian);
        _moveDir.y = -_radius * Mathf.Sin(_radian);

        if ((_owner.Front.x < 0 && _moveDir.x > 0) ||
            (_owner.Front.x > 0 && _moveDir.x < 0))
        {
            _moveDir.x = -_moveDir.x;
        }


        _owner.Move(_moveDir);

        if (_radius < 0.05F && _radius > -0.05F)
        {
            _landing = true;
        }


        

    }

    public void Exit()
    {
        _owner.Stop();
        _owner.Front = -_owner.Front;
        _animator.SetBool(AnimatorHash.IsFly, false);
    }
}

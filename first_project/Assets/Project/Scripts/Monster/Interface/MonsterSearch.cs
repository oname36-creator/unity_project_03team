using UnityEngine;

public class MonsterSearch : MonoBehaviour 
{

    protected Animator _mAnimator;
    protected MonsterController _monsterController;  // 자신 객체

    // 애니메이션 hash 저장
    protected int _workHash;


    public virtual void Search() { }
    public void GetComponent(Animator animator, MonsterController monsterController)
    {
        _mAnimator = animator;
        _monsterController = monsterController;
        _workHash = Animator.StringToHash("work");
    }
}

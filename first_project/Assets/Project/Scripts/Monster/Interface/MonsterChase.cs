using UnityEngine;

public class MonsterChase : MonoBehaviour
{
    protected Animator _mAnimator;
    protected MonsterController _monsterController;  // ÀÚ½Å °´Ã¼

    protected int _chaseHash;

    public virtual void Chase() { }

    public void GetComponent(Animator animator, MonsterController monsterController)
    {
        _mAnimator = animator;
        _monsterController = monsterController;
        _chaseHash = Animator.StringToHash("hasSearched");
    }

}

using UnityEngine;

public class MonsterAttack : MonoBehaviour
{

    protected Animator _mAnimator;
    protected MonsterController _monsterController; // ÀÚ½Å °´Ã¼

    protected int _attackHash; 

    public virtual void Attack() { }
    public void GetComponent(Animator animator, MonsterController monsterController)
    {
        _mAnimator = animator;
        _monsterController = monsterController;
        _attackHash = Animator.StringToHash("attackable");
       
    }
}

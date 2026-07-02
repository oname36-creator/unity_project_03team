using UnityEngine;
using System.Collections;
public class BaseMonsterAttack : MonsterAttack
{

 

    public override void Attack()
    {
        _mAnimator.SetTrigger(_attackHash);



    }


   
}

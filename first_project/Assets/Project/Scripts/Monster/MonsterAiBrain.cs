using System.Collections.Generic;
using UnityEngine;

public class MonsterAiBrain
{

    public static MonsterStateMachine MakeMachine(string name, MonsterController owner) 
    {
        Debug.Log(name);

        IMonsterState initialState = null;
        var transitionMap = new Dictionary<IMonsterState, List<Transition>>();

        if (name == "Base")
        {
         
            IMonsterState search = new BaseMonsterSearch(owner);
            IMonsterState chase = new BaseMonsterChase(owner);
            IMonsterState attack = new BaseMonsterAttack(owner);

            initialState = search;
            transitionMap[search] = new List<Transition>
            {
                // 람다식 문법( () => { 중괄호로 로직 감싸기 } )을 사용
                new Transition(
                    condition: () =>
                    {
                        if (owner.InRange && owner.InAngle)
                        {
                            return true;
                        }
                        return false;
                    },
                    targetState: chase
                    )
            };

            transitionMap[chase] = new List<Transition>
            {
    
                new Transition(
                    condition: () =>
                    {
                        if (!owner.InRange || !owner.InAngle)
                        {
                            return true;
                        }
                        return false;
                    },
                    targetState: search
                    ),
                new Transition(
                    condition: () =>
                    {
                        if (owner.InAttackRange && owner.InAngle)
                        {
                            // 공격 
                            return true;
                        }
                        return false;
                    },
                    targetState: attack
                    )
            };


            transitionMap[attack] = new List<Transition>
            {

                new Transition(
                    condition: () =>
                    {
                        if (!owner.IsAttack)
                        {
                            return true;
                        }
                        return false;
                    },
                    targetState: chase
                    )
            };
        }



        return new MonsterStateMachine(owner, initialState, transitionMap);
    }

}

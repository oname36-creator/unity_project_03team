using System.Collections;
using System.Collections.Generic;
using UnityEditor.Searcher;
using UnityEngine;

public class MonsterAiBrain
{

    public static MonsterStateMachine MakeMachine(string name, MonsterController owner) 
    {
        

        IMonsterState initialState = null;
 
        var transitionMap = new Dictionary<IMonsterState, List<Transition>>();

        if (name == "Base")
        {
            Debug.Log(name);
            IMonsterState search = new BaseMonsterSearch(owner);
            IMonsterState chase = new BaseMonsterChase(owner);
            IMonsterState attack = new BaseMonsterAttack(owner);
            IMonsterState hurt = new MonsterHurt(owner);
            IMonsterState die = new MonsterDie(owner);


            initialState = search;
  
            transitionMap[search] = new List<Transition>
            {
                new Transition( // search -> hurt
                    condition: () =>
                    {
                        return owner.IsHurt;
                    },
                    targetState: hurt
                    ),


                // 람다식 문법( () => { 중괄호로 로직 감싸기 } )을 사용
                new Transition(
                    condition: () =>
                    {
                        return owner.InRange && owner.InAngle;
                    },
                    targetState: chase
                    )
            };

            transitionMap[chase] = new List<Transition>
            {

                new Transition( // chase -> hurt
                    condition: () =>
                    {
                        return owner.IsHurt;
                    },
                    targetState: hurt
                    ),


                new Transition(
                    condition: () =>
                    {
                      return !owner.InRange || !owner.InAngle;
                    },
                    targetState: search
                    ),

                new Transition(
                    condition: () =>
                    {
                       return owner.InAttackRange && owner.InAngle;
                    },
                    targetState: attack
                    )
            };


            transitionMap[attack] = new List<Transition>
            {


                new Transition( // attack -> hurt
                    condition: () =>
                    {
                        return owner.IsHurt;
                    },
                    targetState: hurt
                    ),

                new Transition(
                    condition: () =>
                    {
                      return !owner.IsAttack;
                    },
                    targetState: chase
                    )
            };

            transitionMap[hurt] = new List<Transition>
            {
                new Transition( 
                    condition: () =>
                    {
                        return owner.IsDead;
                    },
                    targetState: die
                    ),

                new Transition(
                    condition: () =>
                    {
                        return !owner.IsHurt && owner.InAttackRange && owner.InAngle;
                    },
                    targetState:attack
                    ),

                new Transition(
                    condition: () =>
                    {
                        return !owner.IsHurt &&owner.InRange && owner.InAngle;
                    },
                    targetState:chase
                    ),

                new Transition(
                    condition: () =>
                    {
                        return !owner.IsHurt;
                    },
                    targetState:search
                    )
            };


            transitionMap[die] = new List<Transition>
            {
                new Transition(
                    condition: () =>
                    {
                        return !owner.IsDead;
                    },
                    targetState:search
                    )
            };


        }


        else if (name == "Bird")
        {
            Debug.Log(name);
            IMonsterState search = new FlyMonsterSearch(owner);
            IMonsterState attack = new FlyMonsterAttack(owner);
            IMonsterState re = new FlyMonsterReturn(owner);


            IMonsterState hurt = new MonsterHurt(owner);
            IMonsterState die = new MonsterDie(owner);


            initialState = search;
             transitionMap[search] = new List<Transition>
            {
                new Transition( // search -> hurt
                    condition: () =>
                    {
                        return owner.IsHurt;
                    },
                    targetState: hurt
                    ),


                new Transition(
                    condition: () =>
                    {
                        //Debug.Log("Search -> Attack");
                        return  owner.InRange && owner.InAngle && !owner.IsAttack;
                    },
                    targetState: attack
                    )
            };

   

            transitionMap[attack] = new List<Transition>
            {

                new Transition( // attack -> hurt
                    condition: () =>
                    {
                        return owner.IsHurt;
                    },
                    targetState: hurt
                    ),

                new Transition(
                    condition: () =>
                    {
                      return !owner.IsAttack;
                    },
                    targetState: re
                    )
            };
            transitionMap[re] = new List<Transition>
            {

                new Transition( // attack -> hurt
                    condition: () =>
                    {
                        return owner.IsHurt;
                    },
                    targetState: hurt
                    ),

                new Transition(
                    condition: () =>
                    {
                      return owner.IsBack;
                    },
                    targetState: search
                    )
            };


            transitionMap[hurt] = new List<Transition>
            {
                new Transition(
                    condition: () =>
                    {
                        return owner.IsDead;
                    },
                    targetState: die
                    ),
      
                new Transition(
                    condition: () =>
                    {
                        return !owner.IsHurt && owner.IsAttack;
                    },
                    targetState:re
                    ),

                new Transition(
                    condition: () =>
                    {
                        return !owner.IsHurt;
                    },
                    targetState:search
                    )
            };

            transitionMap[die] = new List<Transition>
            {
                new Transition(
                    condition: () =>
                    {
                        return !owner.IsDead;
                    },
                    targetState:search
                    )
            };


        }

        initialState.Enter();
        return new MonsterStateMachine(owner, initialState, transitionMap);
    }
    public static MonsterStateMachine MakeMachine(string name, BossController owner)
    {

        IMonsterState initialState = null;
        var transitionMap = new Dictionary<IMonsterState, List<Transition>>();

        IMonsterState chase = new BossChase(owner);
        // 보스룸 상태 추가할 예정

        initialState = chase;

        // 나중에 상태 추가
        //transitionMap[chase] = new List<Transition>
        //    {
        //        new Transition(
        //            condition: () =>
        //            {
        //                return !owner.Chase;
        //            },
        //            targetState:Attack
        //            )
        //    };



        return new MonsterStateMachine(owner, initialState, transitionMap);

    }

}

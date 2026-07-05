using System.Collections;
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
            IMonsterState hurt = new MonsterHurt(owner);
            IMonsterState die = new MonsterDie(owner);


            initialState = search;
            transitionMap[search] = new List<Transition>
            {
                new Transition( // search -> die
                    condition: () =>
                    {
                        return owner.IsDead;
                    },
                    targetState: die
                    ),


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
                new Transition( // chase -> die
                    condition: () =>
                    {
                        return owner.IsDead;
                    },
                    targetState: die
                    ),


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
                new Transition( // attack -> die
                    condition: () =>
                    {
                        return owner.IsDead;
                    },
                    targetState: die
                    ),


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

            IMonsterState search = new FlyMonsterSearch(owner);
            IMonsterState fly = new FlyMonsterFly(owner);
            IMonsterState attack = new FlyMonsterAttack(owner);
            IMonsterState flyEnd = new FlyMonsterFlyEnd(owner);
            IMonsterState jump = new FlyMonsterJump(owner);
            IMonsterState collider = new FlyMonsterCollider(owner);


            IMonsterState hurt = new MonsterHurt(owner);
            IMonsterState die = new MonsterDie(owner);


            initialState = search;
            transitionMap[search] = new List<Transition>
            {
                new Transition( // search -> die
                    condition: () =>
                    {
                        return owner.IsDead;
                    },
                    targetState: die
                    ),


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
                    targetState: jump
                    )
            };

            transitionMap[jump] = new List<Transition>
            {
                new Transition( // search -> die
                    condition: () =>
                    {
                        return owner.IsDead;
                    },
                    targetState: die
                    ),


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
                        return owner.IsCollision;
                    },
                    targetState: collider
                    ),

                // 람다식 문법( () => { 중괄호로 로직 감싸기 } )을 사용
                new Transition(
                    condition: () =>
                    {
                        return true;
                    },
                    targetState: fly
                    )
            };



            transitionMap[fly] = new List<Transition>
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
                        return owner.IsHurt;
                    },
                    targetState: hurt
                    ),

                new Transition(
                    condition: () =>
                    {
                        return owner.IsCollision;
                    },
                    targetState: collider
                    ),

                new Transition(
                    condition: () =>
                    {
                      return !owner.InRange || !owner.InAngle;
                    },
                    targetState: flyEnd
                    ),

                new Transition(
                    condition: () =>
                    {
                       return owner.InAttackRange && owner.InAngle;
                    },
                    targetState: attack
                    )
            };


            transitionMap[flyEnd] = new List<Transition>
            {
                new Transition( // attack -> die
                    condition: () =>
                    {
                        return owner.IsDead;
                    },
                    targetState: die
                    ),


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
                        return owner.IsCollision;
                    },
                    targetState: collider
                    ),

                new Transition(
                    condition: () =>
                    {
                      return !owner.IsFly;
                    },
                    targetState: search
                    )
            };



            transitionMap[attack] = new List<Transition>
            {
                new Transition( // attack -> die
                    condition: () =>
                    {
                        return owner.IsDead;
                    },
                    targetState: die
                    ),


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
                        return owner.IsCollision;
                    },
                    targetState: collider
                    ),

                new Transition(
                    condition: () =>
                    {
                      return !owner.IsAttack;
                    },
                    targetState: flyEnd
                    )
            };

            transitionMap[collider] = new List<Transition>
            {
                new Transition( // attack -> die
                    condition: () =>
                    {
                        return owner.IsDead;
                    },
                    targetState: die
                    ),


                new Transition( // attack -> hurt
                    condition: () =>
                    {
                        return owner.IsHurt;
                    },
                    targetState: hurt
                    )
            };

            transitionMap[hurt] = new List<Transition>
            {

                new Transition(
                    condition: () =>
                    {
                        return !owner.IsHurt && owner.IsFly;
                    },
                    targetState:flyEnd
                    ),
                new Transition(
                    condition: () =>
                    {
                        return !owner.IsHurt && !owner.IsFly;
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







        return new MonsterStateMachine(owner, initialState, transitionMap);
    }

}

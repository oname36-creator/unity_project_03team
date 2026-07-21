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
            new Transition(
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
            new Transition(
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
            new Transition(
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
                      return !owner.IsAttack || !(owner.InAttackRange && owner.InAngle);
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
            new Transition(
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
                        //Debug.Log("Search -> Attack");
                        return  owner.InRange && owner.InAngle && !owner.IsAttack;
                    },
                    targetState: attack
                    )
            };



            transitionMap[attack] = new List<Transition>
            {
            new Transition(
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
                    targetState: re
                    )
            };
            transitionMap[re] = new List<Transition>
            {
            new Transition(
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
                    targetState:attack
                    ),
            new Transition(
                    condition: () =>
                    {
                        return !owner.IsHurt && !owner.IsBack;
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

        else if (name == "DarkWolf")
        {

            Debug.Log(name);
            IMonsterState idle = new DarkWolfIdle(owner);
            IMonsterState walk = new DarkWolfWalk(owner);
            IMonsterState chase = new DarkWolfChase(owner);
            IMonsterState attack = new DarkWolfAttack(owner);

            IMonsterState hurt = new MonsterHurt(owner);
            IMonsterState die = new MonsterDie(owner);

            initialState = idle;

            transitionMap[idle] = new List<Transition>
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
                        return owner.CheckGroundAhead();
                    },
                    targetState:walk
                    ),

                new Transition(
                    condition: () =>
                    {
                        return owner.InRange && owner.InAngle;
                    },
                    targetState: chase
                    )
            };

            transitionMap[walk] = new List<Transition>
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
                        return !owner.CheckGroundAhead();
                    },
                    targetState:idle
                    ),

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
                        return !owner.InRange || !owner.InAngle;
                    },
                    targetState: walk
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
            new Transition(
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
                      return !owner.InAttackRange || !owner.InAngle;
                    },
                    targetState: chase
                    ),

                new Transition(
                    condition: () =>
                    {
                        return !owner.InRange || !owner.InAngle;
                    },
                    targetState: walk
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
                        return !owner.IsHurt;
                    },
                    targetState:idle
                    )
            };

            transitionMap[die] = new List<Transition>
            {
                new Transition(
                    condition: () =>
                    {
                        return !owner.IsDead;
                    },
                    targetState:idle
                    )
            };
        }

        initialState.Enter();
        return new MonsterStateMachine(initialState, transitionMap);
    }
    public static MonsterStateMachine MakeMachine(string name, BossController owner)
    {
        Debug.Log("BossController");
        IMonsterState initialState = null;
        var transitionMap = new Dictionary<IMonsterState, List<Transition>>();

        IMonsterState chase = new BossChase(owner);
        IMonsterState attack = new BossAttack(owner);
        // 보스룸 상태 추가할 예정

        initialState = chase;

        // 나중에 상태 추가
        transitionMap[chase] = new List<Transition>
            {
                new Transition(
                    condition: () =>
                    {
                        return owner.Attack;
                    },
                    targetState:attack
                    )
            };


        initialState.Enter();
        return new MonsterStateMachine(initialState, transitionMap);

    }

    public static MonsterStateMachine MakeMachine(string name, BodyController owner)
    {
        Debug.Log("Body");
        IMonsterState initialState = null;
        var transitionMap = new Dictionary<IMonsterState, List<Transition>>();

        IMonsterState idle = new BodyIdle(owner);
        IMonsterState move = new BodyMove(owner);
        IMonsterState create = new BodyCreateTentacle(owner);
        IMonsterState createArch = new BodyCreateArchTentacle(owner);
        IMonsterState throwObject = new BodyThrowStone(owner);

        // 보스룸 상태 추가할 예정

        initialState = idle;

        // Todo : Boss가 원하면 촉수 무한 생성 

        // 나중에 상태 추가
        transitionMap[idle] = new List<Transition>
            {
                new Transition(
                    condition: () =>
                    {
                        return owner.Move;
                    },
                    targetState:move
                    )
            };

        transitionMap[move] = new List<Transition>
            {
                new Transition(
                    condition: () =>
                    {
                        return !owner.Move;
                    },
                    targetState:idle
                    ),

                new Transition(
                    condition : () =>
                    {
                        return owner.Create;
                    },
                    targetState: create
                    ),

                new Transition(
                    condition : () =>
                    {
                        return (owner.Phase > 1) && (owner.Distance > 50);
                    },
                    targetState: throwObject
                    ),

                new Transition(
                    condition: () =>
                    {
                        return (owner.Phase > 2) && owner.CreateArch;
                    },
                    targetState:createArch
                    )

            };

        transitionMap[create] = new List<Transition>
            {
                new Transition(
                    condition: () =>
                    {
                        return !owner.Create;
                    },
                    targetState:move
                    )
            };

        transitionMap[createArch] = new List<Transition>
            {
                new Transition(
                    condition: () =>
                    {
                        return !owner.CreateArch;
                    },
                    targetState:move
                    )
            };

        transitionMap[throwObject] = new List<Transition>
            {
                new Transition(
                    condition: () =>
                    {
                        return owner.Throw;
                    },
                    targetState:move
                    ),

                new Transition(
                    condition : () =>
                    {
                        return owner.Create;
                    },
                    targetState: create
                    )
            };

        initialState.Enter();
        return new MonsterStateMachine(initialState, transitionMap);

    }


    public static MonsterStateMachine MakeMachine(string name, TentacleController owner)
    {
        Debug.Log("TentacleController");
        IMonsterState initialState = null;
        var transitionMap = new Dictionary<IMonsterState, List<Transition>>();

        IMonsterState idle = new TentacleIdle(owner);
        IMonsterState stretch = new TentacleStretch(owner);
        IMonsterState attach = new TentacleAttach(owner);
        IMonsterState up = new TentacleUp(owner);
        IMonsterState attack = new TentacleAttack(owner);
        IMonsterState trapIdle = new TentacleTrap(owner);
        IMonsterState trapAction = new TentacleTrapAction(owner);
        IMonsterState re = new TentacleReturn(owner);
        IMonsterState archUp = new TentacleArchUp(owner);
        IMonsterState archAttack = new TentacleArchAttack(owner);

        initialState = idle;

        // 나중에 상태 추가
        transitionMap[idle] = new List<Transition>
            {
                new Transition(
                    condition: () =>
                    {
                        return owner.isArch;
                    },
                    targetState:archUp
                    ),
            new Transition(
                    condition: () =>
                    {
                        return owner.isTrap;
                    },
                    targetState:trapIdle
                    ),

                new Transition(
                    condition: () =>
                    {
                        return owner.IsAttackTentacle;
                    },
                    targetState:up
                    ),

                new Transition(
                    condition: () =>
                    {
                        return owner.IsSearch;
                    },
                    targetState:stretch
                    )
            };

        transitionMap[stretch] = new List<Transition>
            {
                new Transition(
                    condition: () =>
                    {
                        return owner.IsAttach;;
                    },
                    targetState:attach
                    ),

                new Transition(
                    condition: () =>
                    {
                        return !owner.IsSearch;
                    },
                    targetState:idle
                    )
            };

        transitionMap[attach] = new List<Transition>
            {
                new Transition(
                    condition: () =>
                    {
                        return !owner.IsAttach;
                    },
                    targetState:idle
                    )
            };

        transitionMap[up] = new List<Transition>
            {
                new Transition(
                    condition: () =>
                    {
                        return owner.Attack;;
                    },
                    targetState:attack
                    )
            };


        transitionMap[attack] = new List<Transition>
            {
                new Transition(
                    condition: () =>
                    {
                        return !owner.Attack;
                    },
                    targetState:attach
                    )
            };

        transitionMap[archUp] = new List<Transition>
            {
                new Transition(
                    condition: () =>
                    {
                        return owner.Attack;
                    },
                    targetState:archAttack
                    )
            };

        transitionMap[archAttack] = new List<Transition>
            {
                new Transition(
                    condition: () =>
                    {
                        return !owner.isArch;
                    },
                    targetState:idle
                    )
            };
        transitionMap[trapIdle] = new List<Transition>
            {
                new Transition(
                    condition: () =>
                    {
                        return owner.Attack;
                    },
                    targetState:trapAction
                    )
            };

        transitionMap[trapAction] = new List<Transition>
            {
                new Transition(
                    condition: () =>
                    {
                        return owner.IsAttach;
                    },
                    targetState:attach
                    ),

                new Transition(
                    condition: () =>
                    {
                        return !owner.Attack;
                    },
                    targetState:re
                    )

            };
        transitionMap[re] = new List<Transition>
            {
                new Transition(
                    condition: () =>
                    {
                        return !owner.isTrap;
                    },
                    targetState:idle
                    ),

            new Transition(
                    condition: () =>
                    {
                        return owner.IsAttach;
                    },
                    targetState:attach
                    )
            };


        initialState.Enter();
        return new MonsterStateMachine(initialState, transitionMap);

    }

}

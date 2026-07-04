using UnityEngine;

public interface IMonsterState
{
    // 상태에 들어왔을때
    void Enter();

    // 상태에 있을떄
    void Update();

    // 상태가 변경되었을떄
    void Exit();

}

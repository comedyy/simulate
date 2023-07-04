using Unity.Entities;
using Unity.Mathematics;

public class BattleEndFlag
{
    public bool isEnd;
}


public class GameTimeoutSystem : ComponentSystem
{
    public BattleEndFlag flag;

    protected override void OnCreate()
    {
        base.OnCreate();

        var entity = EntityManager.CreateEntity(typeof(GameTimeoutComponent));
        EntityManager.SetComponentData(entity, new GameTimeoutComponent(){
            destoryTime = 60
        });
    }

    protected override void OnUpdate()
    {
        if(GetSingleton<LogicTime>().escaped > GetSingleton<GameTimeoutComponent>().destoryTime)
        {
            flag.isEnd = true;
        }
    }
}
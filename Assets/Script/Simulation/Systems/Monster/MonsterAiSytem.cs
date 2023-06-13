using Unity.Entities;
using Unity.Mathematics;


public class MonsterAiSytem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var escaped = GetSingleton<LogicTime>().escaped;

        Entities.ForEach((ref MonsterAiComponent ai, ref LMoveByDirComponent moveByDirComponent)=>{
            if(escaped < ai.lastTurnTime + ai.randomTurnInterval)
            {
                return;
            }

            var random = EntityManager.GetComponentObject<RandomComponent>(GetSingletonEntity<RandomComponent>()).random;
            moveByDirComponent.dir = quaternion.RotateY(RandomUtils.Random(random, 2 * math.PI));
            ai.lastTurnTime = escaped;
        });
    }
}
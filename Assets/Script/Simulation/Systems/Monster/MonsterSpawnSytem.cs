using Unity.Entities;
using Unity.Mathematics;


public class MonsterSpawnSytem : ComponentSystem
{
    protected override void OnCreate()
    {
        base.OnCreate();
        var entity = EntityManager.CreateEntity(typeof(SpawnMonsterComponent));
        EntityManager.SetComponentData(entity, new SpawnMonsterComponent(){
            maxCount = 5, interval = 2.0f
        });
    }

    protected override void OnUpdate()
    {
        var monsterSpwan = GetSingleton<SpawnMonsterComponent>();

        if(monsterSpwan.maxCount <= monsterSpwan.currentCount)
        {
            return;
        } 

        if(Time.ElapsedTime - monsterSpwan.lastSpawnTime < monsterSpwan.interval)
        {
            return;
        }

        var spwanEventEntity = EntityManager.CreateEntity(typeof(SpawnEvent));
        var random = EntityManager.GetComponentObject<RandomComponent>(GetSingletonEntity<RandomComponent>()).random;

        EntityManager.SetComponentData(spwanEventEntity, new SpawnEvent(){
            isUser = false, 
            position = new float3(RandomUtils.Random(random, 10), 0, RandomUtils.Random(random, 10)),
            dir = quaternion.RotateY(RandomUtils.Random(random, 2 * math.PI)),
            aiInterval = 3f,
            despawnTime = 5 + (float)Time.ElapsedTime
        });

        monsterSpwan.currentCount++;
        monsterSpwan.lastSpawnTime = (float)Time.ElapsedTime;

        SetSingleton(monsterSpwan);
    }
}
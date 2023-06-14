using Unity.Entities;
using Unity.Mathematics;


public class MonsterSpawnSytem : ComponentSystem
{
    protected override void OnCreate()
    {
        base.OnCreate();
        var entity = EntityManager.CreateEntity(typeof(SpawnMonsterComponent));
        EntityManager.SetComponentData(entity, new SpawnMonsterComponent(){
            maxCount = 1000, interval = 0.0f, spawnCountPerInterval = 10
        });
    }

    protected override void OnUpdate()
    {
        var monsterSpwan = GetSingleton<SpawnMonsterComponent>();
        var escaped = GetSingleton<LogicTime>().escaped;

        if(monsterSpwan.maxCount <= monsterSpwan.currentCount)
        {
            return;
        } 

        if(escaped - monsterSpwan.lastSpawnTime < monsterSpwan.interval)
        {
            return;
        }

        var spwanEventEntity = EntityManager.CreateEntity(typeof(SpawnEvent));
        var random = EntityManager.GetComponentObject<RandomComponent>(GetSingletonEntity<RandomComponent>()).random;

        for(int i = 0; i < monsterSpwan.spawnCountPerInterval; i++)
        {
            var randomAngle = RandomUtils.Random(random, 2 * math.PI);
            EntityManager.SetComponentData(spwanEventEntity, new SpawnEvent(){
                isUser = false, 
                position = new float3(RandomUtils.Random(random, 10), 0, RandomUtils.Random(random, 10)),
                dir = randomAngle,
                aiInterval = 1f,
                despawnTime = 5 + escaped
            });    
        }

        monsterSpwan.currentCount++;
        monsterSpwan.lastSpawnTime = escaped;

        SetSingleton(monsterSpwan);
    }
}
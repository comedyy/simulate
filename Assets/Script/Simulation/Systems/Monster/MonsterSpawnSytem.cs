using Unity.Entities;
using Unity.Mathematics;


public class MonsterSpawnSytem : ComponentSystem
{
    protected override void OnCreate()
    {
        base.OnCreate();
        var entity = EntityManager.CreateEntity(typeof(SpawnMonsterComponent));
        EntityManager.SetComponentData(entity, new SpawnMonsterComponent(){
            maxCount = 999, interval = fp.Create(0), spawnCountPerInterval = 10
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
            var randomAngle = RandomUtils.Random(random, 2 * fpMath.PI);
            var sin = fpMath.sin(randomAngle);
            var cos = fpMath.cos(randomAngle);

            EntityManager.SetComponentData(spwanEventEntity, new SpawnEvent(){
                isUser = false, 
                position = new fp3(RandomUtils.Random(random, 10), 0, RandomUtils.Random(random, 10)),
                dir = new fp3(cos, 0, sin),
                aiInterval = fp.Create(1),
                despawnTime = 5 + escaped,
                hp = 50,atk = 5
            });    
        }

        monsterSpwan.currentCount++;
        monsterSpwan.lastSpawnTime = escaped;

        SetSingleton(monsterSpwan);
    }
}
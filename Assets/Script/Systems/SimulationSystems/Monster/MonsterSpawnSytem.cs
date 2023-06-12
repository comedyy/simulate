using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

internal class MonsterGenerateSytem : ComponentSystem
{
    protected override void OnCreate()
    {
        base.OnCreate();
        var entity = EntityManager.CreateEntity(typeof(SpawnMonsterComponent));
        EntityManager.SetComponentData(entity, new SpawnMonsterComponent(){
            maxCount = 10, interval = 2.0f
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
        EntityManager.SetComponentData(spwanEventEntity, new SpawnEvent(){
            isUser = false, 
            position = new float3((World as BattleWorld).Random(10), 0, (World as BattleWorld).Random(10)),
            dir = quaternion.RotateY((World as BattleWorld).Random(2 * math.PI)),
        });

        monsterSpwan.currentCount++;
        monsterSpwan.lastSpawnTime = (float)Time.ElapsedTime;

        SetSingleton(monsterSpwan);
    }
}
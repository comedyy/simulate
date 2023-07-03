using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class MonsterSpawnSytem : ComponentSystem
{
    EntityQuery _queryEnemyCount;
    protected override void OnCreate()
    {
        base.OnCreate();
        var entity = EntityManager.CreateEntity(typeof(SpawnMonsterComponent));
        EntityManager.SetComponentData(entity, new SpawnMonsterComponent(){
            maxCount = 100, interval = fp.Create(1), spawnCountPerInterval = 10
        });

        _queryEnemyCount = GetEntityQuery(ComponentType.ReadOnly<RvoSimulatorComponet>());
    }

    protected override void OnUpdate()
    {
        var monsterSpwan = GetSingleton<SpawnMonsterComponent>();
        var escaped = GetSingleton<LogicTime>().escaped;

        if(monsterSpwan.maxCount <= _queryEnemyCount.CalculateEntityCount())
        {
            return;
        } 

        if(escaped - monsterSpwan.lastSpawnTime < monsterSpwan.interval)
        {
            return;
        }

        for(int i = 0; i < monsterSpwan.spawnCountPerInterval; i++)
        {
            var random = EntityManager.GetComponentObject<RandomComponent>(GetSingletonEntity<RandomComponent>()).random;
            var foundPos = GetRandomSpawnPos(random, out var pos);
            if(!foundPos) continue;

            var spwanEventEntity = EntityManager.CreateEntity(typeof(SpawnEvent));

            var randomAngle = RandomUtils.Random(random, 2 * fpMath.PI);
            var sin = fpMath.sin(randomAngle);
            var cos = fpMath.cos(randomAngle);

            EntityManager.SetComponentData(spwanEventEntity, new SpawnEvent(){
                isUser = false, 
                position = pos,
                dir = new fp3(cos, 0, sin),
                aiInterval = fp.Create(1),
                despawnTime = 5 + escaped,
                hp = 50,atk = 5
            });    
        }

        monsterSpwan.lastSpawnTime = escaped;

        SetSingleton(monsterSpwan);
    }

    List<fp3> list = new List<fp3>();
    private bool GetRandomSpawnPos(System.Random random, out fp3 p)
    {

        var listUser = GetSingleton<UserListComponent>().allUser;
        if(listUser.length == 0)
        {
            p = default;
            return false;
        }

        var pos = EntityManager.GetComponentData<LTransformComponet>(listUser[0]).position;
        fp3 min = pos;
        fp3 max = pos;
        list.Add(pos);

        for(int i = 1; i < listUser.length; i++)
        {
            var pos1 = EntityManager.GetComponentData<LTransformComponet>(listUser[i]).position;
            list.Add(pos1);

            min = fpMath.Min(min, pos1);
            max = fpMath.Max(max, pos1);
        }

        var range = 25;
        min = min - new fp3(range, 0, range);
        max = max + new fp3(range, 0, range);

        for(int i = 0; i < 10; i++)
        {
            var posx = new fp3(RandomUtils.Random(random, min.x, max.x), 0, RandomUtils.Random(random, min.z, max.z));
            var isError = false;
            foreach (var userPos in list)
            {
                if(fpMath.distancesq(posx, userPos) < 20 * 20) 
                {
                    isError = true;
                    continue;
                }
            }

            if(!isError)
            {
                p = posx;
                return true;
            }
        }

        p = default;
        return false;
    }
}
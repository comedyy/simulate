using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class MonsterSpawnSytem : ComponentSystem
{
    protected override void OnCreate()
    {
        base.OnCreate();
        var entity = EntityManager.CreateEntity(typeof(SpawnMonsterComponent));
        EntityManager.SetComponentData(entity, new SpawnMonsterComponent(){
            maxCount = 100, interval = fp.Create(1), spawnCountPerInterval = 10
        });
    }

    protected override void OnUpdate()
    {
        var monsterSpwan = GetSingleton<SpawnMonsterComponent>();
        var escaped = GetSingleton<LogicTime>().escaped;

        if(monsterSpwan.maxCount <= MoveByDirSystem.Count)
        {
            return;
        } 

        if(escaped - monsterSpwan.lastSpawnTime < monsterSpwan.interval)
        {
            return;
        }

        var random = GetSingleton<RandomComponent>().random;
        for(int i = 0; i < monsterSpwan.spawnCountPerInterval; i++)
        {
            var foundPos = GetRandomSpawnPos(ref random, out var pos);
            if(!foundPos) continue;

            var spwanEventEntity = EntityManager.CreateEntity(typeof(SpawnEvent));

            var randomAngle = random.NextFp(0, 2 * fpMath.PI);
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
        
        SetSingleton(new RandomComponent(){random = random});

        monsterSpwan.lastSpawnTime = escaped;

        SetSingleton(monsterSpwan);
    }

    List<fp3> list = new List<fp3>();
    private bool GetRandomSpawnPos(ref fpRandom random, out fp3 p)
    {
        list.Clear();
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
            var posx = new fp3(random.NextFp(min.x, max.x), 0, random.NextFp(min.z, max.z));
            var isError = false;
            foreach (var userPos in list)
            {
                var distanceSq = fpMath.distancesq(posx, userPos);
                if(distanceSq < 20 * 20) 
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
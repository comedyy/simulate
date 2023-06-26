using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;


public class HurtSystem : ComponentSystem
{
    List<Entity> _deadEntities = new List<Entity>();
    protected override void OnUpdate()
    {
        _deadEntities.Clear();

        var rvoObj = this.GetSingletonObject<RvoSimulatorComponet>();
        var buffer = EntityManager.GetBuffer<HurtComponent>(GetSingletonEntity<HurtComponent>());
        var bufferVHurt = EntityManager.GetBuffer<VHurtComponent>(GetSingletonEntity<VHurtComponent>());
        var bufferDead = EntityManager.GetBuffer<DeSpawnEventComponent>(GetSingletonEntity<DeSpawnEventComponent>());

        for(int i = 0; i < buffer.Length; i++)
        {
            var ev = buffer[i];
            var entity = ev.target;
            if(!EntityManager.Exists(entity))
            {
                continue;
            }

            var hpComponent = EntityManager.GetComponentData<HpComponent>(entity);

            if(hpComponent.hp <= 0) continue;

            hpComponent.hp -= ev.value;
            EntityManager.SetComponentData(entity, hpComponent);
            bufferVHurt.Add(new VHurtComponent(){
                target = entity, value = ev.value
            });

            if(hpComponent.hp > 0) continue;

            _deadEntities.Add(entity);
            bufferDead.Add(new DeSpawnEventComponent(){entity = entity});

            if(EntityManager.HasComponent<LRvoComponent>(entity))
            {
                var rvoComponent = EntityManager.GetComponentData<LRvoComponent>(entity);
                rvoObj.RemoveAgent(rvoComponent.rvoId);
            }
            else
            {
                var list = GetSingleton<UserListComponent>().allUser;
                for(int j = 0; j < list.length; j++)
                {
                    if(list[j] == entity)
                    {
                        list.RemoveAt(j);
                        break;
                    }
                }
                SetSingleton(new UserListComponent(){
                    allUser = list
                });
            }
        };

        buffer.Clear();

        for(int i = 0; i < _deadEntities.Count; i++)
        {
            EntityManager.DestroyEntity(_deadEntities[i]);
        }
    }
}
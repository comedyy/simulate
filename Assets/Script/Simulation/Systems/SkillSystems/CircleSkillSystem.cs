using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;


public class CircleSkillSystem : ComponentSystem
{
    List<Entity> tempEntities = new List<Entity>();
    protected override void OnUpdate()
    {   
        LogicTime logic = GetSingleton<LogicTime>();
        tempEntities.Clear();
        var buffer = EntityManager.GetBuffer<HurtComponent>(GetSingletonEntity<HurtComponent>());

        var listUser = GetSingleton<UserListComponent>().allUser;
        Entities.WithNone<UserTag>().ForEach((Entity entity, ref LTransformComponet trans, ref AtkComponent atk, ref SkillComponent skill)=>{

            if(logic.escaped - skill.preHurtTime < skill.interval) return;
            skill.preHurtTime = logic.escaped;
            var myPos = trans.position;

            for(int i = 0; i < listUser.length; i++)
            {
                var item = listUser[i];
                var pos = EntityManager.GetComponentData<LTransformComponet>(item).position;
                if(math.distancesq(myPos, pos) > skill.range * skill.range)
                {
                    continue;
                }

                buffer.Add(new HurtComponent(){
                    target = entity,
                    value = atk.atk 
                });
            }
        });

        var checkSum = this.GetSingletonObject<CheckSumComponet>().checkSum;
        var rvo = this.GetSingletonObject<RvoSimulatorComponet>().rvoSimulator;
        Entities.ForEach((ref LTransformComponet trans, ref AtkComponent atk, ref SkillComponent skill, ref UserTag tag)=>{
            if(logic.escaped - skill.preHurtTime < skill.interval) return;
            skill.preHurtTime = logic.escaped;

            rvo.FindAgents(new RVO.Vector2(trans.position.x, trans.position.z), skill.range, tempEntities);
            foreach (var item in tempEntities)
            {
                checkSum.targetFindCheckSum.CheckValue(item, item.GetHashCode());
                buffer.Add(new HurtComponent(){
                    target = item,
                    value = atk.atk 
                });
            }
        });
    }
}
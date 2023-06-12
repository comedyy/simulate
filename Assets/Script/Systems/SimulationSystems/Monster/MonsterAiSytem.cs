using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

internal class MonsterAiSytem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref MonsterAiComponent ai, ref LMoveByDirComponent moveByDirComponent)=>{
            if(Time.ElapsedTime < ai.lastTurnTime + ai.randomTurnInterval)
            {
                return;
            }

            moveByDirComponent.dir = quaternion.RotateY((World as BattleWorld).Random(2 * math.PI));
            ai.lastTurnTime = (float)Time.ElapsedTime;
        });
    }
}
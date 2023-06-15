using Unity.Entities;
using Unity.Mathematics;

// 往主角方向冲
public class MonsterAiSytem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var escaped = GetSingleton<LogicTime>().escaped;
        var listUser = GetSingleton<UserListComponent>().allUser;

        Entities.ForEach((ref MonsterAiComponent ai, ref LMoveByDirComponent moveByDirComponent, ref LTransformComponet trans)=>{

            var targetPos =  EntityManager.GetComponentData<LTransformComponet>(listUser[0]).position;
            var distanceSq = math.distancesq(trans.position, targetPos);
            var entityTarget = listUser[0];
            for(int i = 0; i < listUser.length; i++)
            {
                var current = listUser[i];
                var pos = EntityManager.GetComponentData<LTransformComponet>(listUser[i]).position;
                var dis = math.distancesq(trans.position, pos);
                if(dis < distanceSq)
                {
                    entityTarget = current;
                    distanceSq = dis;
                    targetPos = pos;
                }
            }

            moveByDirComponent.dir = math.normalize(targetPos - trans.position);
        });
    }
}
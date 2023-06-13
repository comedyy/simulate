using Unity.Entities;
using Unity.Mathematics;


public class InputUserPositionSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref MessageUpdatePosEvent message)=>{
            EntityManager.SetComponentData(message.entity, new LMoveByPosComponent(){
                pos = message.pos
            });

            EntityManager.DestroyEntity(entity);
        });
    }
}
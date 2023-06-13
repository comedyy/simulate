using Unity.Entities;
using Unity.Mathematics;

public class CreateControllerSystem : ComponentSystemBase
{
    protected override void OnCreate()
    {
        base.OnCreate();
        
        var entity = EntityManager.CreateEntity();
        EntityManager.AddComponentData(entity, new SpawnEvent()
        {
            position = float3.zero,
            dir = quaternion.identity,
            isUser = true
        });
    }

    public override void Update()
    {
        
    }
}
using Unity.Entities;
using UnityEngine;

internal class VSpawnTargetSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref VSpawnEvent ev)=>{
            var prefab = EntityManager.HasComponent<ControllerTag>(ev.target) ? Resources.Load<GameObject>("Role") : Resources.Load<GameObject>("Monster");
            var lTransformCom = EntityManager.GetComponentData<LTransformComponet>(ev.target);
            var com = new GameObjectBindingComponent(){
                obj = GameObject.Instantiate(prefab, lTransformCom.position, lTransformCom.rotation)
            };
            EntityManager.SetComponentData(ev.target, com);

            EntityManager.DestroyEntity(entity);
        });
    }
}
using Unity.Entities;
using UnityEngine;

public class VSpawnTargetSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref VSpawnEvent ev)=>{
            var prefab = ev.isUser ? Resources.Load<GameObject>("Role") : Resources.Load<GameObject>("Monster");
            var lTransformCom = EntityManager.GetComponentData<LTransformComponet>(ev.target);
            var com = new GameObjectBindingComponent(){
                obj = GameObject.Instantiate(prefab, lTransformCom.position, lTransformCom.rotation),
            };
            EntityManager.AddComponentData(ev.target, com);

            if(ev.isUser)
            {
                EntityManager.AddComponentData(EntityManager.CreateEntity(), new ControllerHolder(){
                    controller = ev.target
                });
            }

            EntityManager.DestroyEntity(entity);
        });
    }
}
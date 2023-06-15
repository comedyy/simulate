using Unity.Entities;
using UnityEngine;

public class VSpawnTargetSystem : ComponentSystem
{
    public int UserId { get; internal set; }

    protected override void OnCreate()
    {
        base.OnCreate();

    }

    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref VSpawnEvent ev)=>{
            GameObject prefab = null;
            var isController = ev.id == UserId;
            if(isController) prefab = Resources.Load<GameObject>("Controller");
            else if(ev.isUser) prefab = Resources.Load<GameObject>("Role");
            else prefab = Resources.Load<GameObject>("Monster");

            var lTransformCom = EntityManager.GetComponentData<LTransformComponet>(ev.target);
            var com = new GameObjectBindingComponent(){
                obj = GameObject.Instantiate(prefab, lTransformCom.position, lTransformCom.rotation),
            };
            EntityManager.AddComponentData(ev.target, com);

            if(isController)
            {
                SetSingleton(new ControllerHolder(){
                    controller = ev.target
                });

                com.objFollow = GameObject.Instantiate(Resources.Load<GameObject>("ControllerFollow"), lTransformCom.position, lTransformCom.rotation);
            }

            var entityBinding = GetSingletonEntity<BindingComponet>();
            var binding = EntityManager.GetComponentObject<BindingComponet>(entityBinding);
            binding.allObject.Add(ev.target, com.obj);


            EntityManager.DestroyEntity(entity);
        });
    }
}
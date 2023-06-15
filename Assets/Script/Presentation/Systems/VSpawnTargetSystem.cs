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
        var entity = GetSingletonEntity<SpawnEventComponent>();
        var buffer = EntityManager.GetBuffer<SpawnEventComponent>(entity);
        for(int i = 0; i < buffer.Length; i++)
        {
            var ev = buffer[i];

            GameObject prefab = null;
            var isController = ev.id == UserId;
            if(isController) prefab = Resources.Load<GameObject>("Controller");
            else if(ev.isUser) prefab = Resources.Load<GameObject>("Role");
            else prefab = Resources.Load<GameObject>("Monster");

            var lTransformCom = EntityManager.GetComponentData<LTransformComponet>(ev.entity);
            var com = new GameObjectBindingComponent(){
                obj = GameObject.Instantiate(prefab, lTransformCom.position, lTransformCom.rotation),
            };
            EntityManager.SetComponentData(ev.entity, com);

            if(isController)
            {
                SetSingleton(new ControllerHolder(){
                    controller = ev.entity
                });

                com.objFollow = GameObject.Instantiate(Resources.Load<GameObject>("ControllerFollow"), lTransformCom.position, lTransformCom.rotation);
            }

            var entityBinding = GetSingletonEntity<BindingComponet>();
            var binding = EntityManager.GetComponentObject<BindingComponet>(entityBinding);
            binding.allObject.Add(ev.entity, com.obj);
        }
    }
}
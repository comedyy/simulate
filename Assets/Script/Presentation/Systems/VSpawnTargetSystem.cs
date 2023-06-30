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
            if(!EntityManager.Exists(ev.entity))
            {
                continue;
            }

            GameObject prefab = null;
            var isController = ev.id == UserId;
            if(ev.isUser) prefab = Resources.Load<GameObject>("Controller");
            else prefab = Resources.Load<GameObject>("Monster");

            var lTransformCom = EntityManager.GetComponentData<LTransformComponet>(ev.entity);
            var com = new GameObjectBindingComponent(){
                obj = GameObject.Instantiate(prefab, lTransformCom.position, Quaternion.LookRotation(lTransformCom.rotation, Vector3.up)),
            };

            var animator = com.obj.GetComponent<Animator>();
            if(animator != null)
            {
                com.animator = animator;
            }
            EntityManager.SetComponentData(ev.entity, com);

            if(isController)
            {
                SetSingleton(new ControllerHolder(){
                    controller = ev.entity
                });

                var isInputUser = true;
                if(Application.isEditor) isInputUser = ev.id == 1;

                if(isInputUser)
                {
                    com.objFollow = GameObject.Instantiate(Resources.Load<GameObject>("ControllerFollow"), lTransformCom.position, Quaternion.LookRotation(lTransformCom.rotation, Vector3.up));
                }
            }

            var entityBinding = GetSingletonEntity<BindingComponet>();
            var binding = EntityManager.GetComponentObject<BindingComponet>(entityBinding);
            binding.allObject.Add(ev.entity, com.obj);
        }
    }
}
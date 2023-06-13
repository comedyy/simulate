using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;


public class BattleWorld : World
{
    public BattleWorld(string name) : base(name)
    {
        EntityManager.AddComponentData(EntityManager.CreateEntity(), new RandomComponent(){
            random = new Random(0)
        });

        InitInititationSystem();
        InitSimulationSystem();

#if !ONLY_LOGIC
        InitPresentationSystem();
#endif
    }

    private void InitInititationSystem()
    {
        CreateSystem<CreateControllerSystem>();

        var group = GetOrCreateSystem<CustomSystems1>();
        GetOrCreateSystem<InitializationSystemGroup>().AddSystemToUpdateList(group);
        
        group.AddSystemToUpdateList(CreateSystem<UpdateWorldTimeSystem>());
    }

    public void InitSimulationSystem()
    {
        var group = GetOrCreateSystem<CustomSystems2>();
        GetOrCreateSystem<SimulationSystemGroup>().AddSystemToUpdateList(group);
        FixedRateUtils.EnableFixedRateWithCatchUp(group, 0.1f);

        group.AddSystemToUpdateList(CreateSystem<UpdateLogicTimeSystem>()); // 更新时间
        group.AddSystemToUpdateList(CreateSystem<InputUserPositionSystem>()); // 玩家输入


        group.AddSystemToUpdateList(CreateSystem<MonsterSpawnSytem>());
        group.AddSystemToUpdateList(CreateSystem<MonsterAiSytem>());
        group.AddSystemToUpdateList(CreateSystem<MosnterDespawnSystem>());
        
        group.AddSystemToUpdateList(CreateSystem<SpawnTargetSystem>());
        
        group.AddSystemToUpdateList(CreateSystem<RecordPrePositionSystem>());
        group.AddSystemToUpdateList(CreateSystem<MoveByDirSystem>());
        group.AddSystemToUpdateList(CreateSystem<MoveByPosSystem>());

        group.AddSystemToUpdateList(CreateSystem<MoveByPosSystem>());

        // cal CheckSum
        group.AddSystemToUpdateList(CreateSystem<CalHashSystem>());
    }

    public void InitPresentationSystem()
    {
        var group = GetOrCreateSystem<CustomSystems3>();
        GetOrCreateSystem<PresentationSystemGroup>().AddSystemToUpdateList(group);

        // add all simulation systems
        group.AddSystemToUpdateList(CreateSystem<VSpawnTargetSystem>());
        group.AddSystemToUpdateList(CreateSystem<VDespawnSystem>());
        group.AddSystemToUpdateList(CreateSystem<VLerpTransformSystem>());
        group.AddSystemToUpdateList(CreateSystem<VCameraFollowSystem>());
        group.AddSystemToUpdateList(CreateSystem<ControllerMoveSystem>());
    }
}

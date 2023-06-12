using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class BattleWorld : World
{
    public BattleWorld(string name) : base(name)
    {
        InitInititationSystem();
        InitSimulationSystem();
        InitPresentationSystem();
    }

    private void InitInititationSystem()
    {
        CreateSystem<CreateControllerSystem>();

        var group = GetOrCreateSystem<InitializationSystemGroup>();
        group.AddSystemToUpdateList(CreateSystem<UpdateWorldTimeSystem>());
        group.AddSystemToUpdateList(CreateSystem<ControllerMoveSystem>());
    }

    public void InitSimulationSystem()
    {
        var group = GetOrCreateSystem<SimulationSystemGroup>();
        FixedRateUtils.EnableFixedRateWithCatchUp(group, 0.1f);

        group.AddSystemToUpdateList(CreateSystem<SpawnTargetSystem>());
        group.AddSystemToUpdateList(CreateSystem<InputUserPositionSystem>());
        
        group.AddSystemToUpdateList(CreateSystem<RecordPrePositionSystem>());
        group.AddSystemToUpdateList(CreateSystem<MoveByDirSystem>());
        group.AddSystemToUpdateList(CreateSystem<MoveByPosSystem>());
    }

    public void InitPresentationSystem()
    {
        var group = GetOrCreateSystem<PresentationSystemGroup>();

        // add all simulation systems
        group.AddSystemToUpdateList(CreateSystem<VSpawnTargetSystem>());
        group.AddSystemToUpdateList(CreateSystem<VLerpTransformSystem>());
        group.AddSystemToUpdateList(CreateSystem<VCameraFollowSystem>());
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class BattleWorld : World
{
    System.Random _random;

    public BattleWorld(string name) : base(name)
    {
        _random = new System.Random(0);

        InitInititationSystem();
        InitSimulationSystem();
        InitPresentationSystem();
    }

    private void InitInititationSystem()
    {
        CreateSystem<CreateControllerSystem>();

        var group = GetOrCreateSystem<CustomSystems1>();
        GetOrCreateSystem<InitializationSystemGroup>().AddSystemToUpdateList(group);
        
        group.AddSystemToUpdateList(CreateSystem<UpdateWorldTimeSystem>());
        group.AddSystemToUpdateList(CreateSystem<ControllerMoveSystem>());
    }

    public void InitSimulationSystem()
    {
        var group = GetOrCreateSystem<CustomSystems2>();
        GetOrCreateSystem<SimulationSystemGroup>().AddSystemToUpdateList(group);
        FixedRateUtils.EnableFixedRateWithCatchUp(group, 0.1f);

        group.AddSystemToUpdateList(CreateSystem<MonsterGenerateSytem>());
        group.AddSystemToUpdateList(CreateSystem<SpawnTargetSystem>());
        group.AddSystemToUpdateList(CreateSystem<InputUserPositionSystem>());
        
        group.AddSystemToUpdateList(CreateSystem<RecordPrePositionSystem>());
        group.AddSystemToUpdateList(CreateSystem<MoveByDirSystem>());
        group.AddSystemToUpdateList(CreateSystem<MoveByPosSystem>());
    }

    public void InitPresentationSystem()
    {
        var group = GetOrCreateSystem<CustomSystems3>();
        GetOrCreateSystem<PresentationSystemGroup>().AddSystemToUpdateList(group);

        // add all simulation systems
        group.AddSystemToUpdateList(CreateSystem<VSpawnTargetSystem>());
        group.AddSystemToUpdateList(CreateSystem<VLerpTransformSystem>());
        group.AddSystemToUpdateList(CreateSystem<VCameraFollowSystem>());
    }

    public float Random()
    {
        return _random.Next(0, 10000) / 10000f;
    }

    public float Random(float random)
    {
        return random * Random();
    }
}

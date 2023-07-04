using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;


public class BattleWorld : World
{
    BattleEndFlag flag = new BattleEndFlag();
    public bool IsEnd => flag.isEnd;

    public BattleWorld(string name, CheckSumMgr checksum, bool randomFixedCount, fp logicFrameInterval, LocalFrame localServer, int userId, int userCount, bool userAutoMove) : base(name)
    {
        EntityManager.AddComponentData(EntityManager.CreateEntity(), new CheckSumComponet(){
            checkSum = checksum
        });

        EntityManager.AddComponentData(EntityManager.CreateEntity(), new BindingComponet());

        // init systems 
        InitiazationSystem.logicFrameInterval = logicFrameInterval;
        InitiazationSystem.userCount = userCount;

        var init = CreateSystem<InitiazationSystem>();
        init.Init(userId);

        InitMoveSystem(userAutoMove, localServer);

        // update systems
        InitSimulationSystem(localServer, randomFixedCount);

#if !ONLY_LOGIC
        InitPresentationSystem(localServer, userId);
#endif
    }

    private void InitMoveSystem(bool userAutoMove, LocalFrame localServer)
    {
        var group = GetOrCreateSystem<CustomSystems3>();
        var moveSytem = CreateSystem<ControllerMoveSystem>();
        moveSytem.localServer = localServer;
        moveSytem.userAutoMove = userAutoMove;
        group.AddSystemToUpdateList(moveSytem);
    }

    public void InitSimulationSystem(LocalFrame frame, bool randomFixedCount)
    {
        var group = GetOrCreateSystem<FixedTimeSystemGroup>();
        group.InitLogicTime(frame, flag, randomFixedCount);
        GetOrCreateSystem<Unity.Entities.SimulationSystemGroup>().AddSystemToUpdateList(group);

        var inputSystem = CreateSystem<InputUserPositionSystem>();
        inputSystem.GetAllMessage = frame.GetFrameInput;
        group.AddSystemToUpdateList(inputSystem); // 玩家输入

        group.AddSystemToUpdateList(CreateSystem<CircleSkillSystem>());
        group.AddSystemToUpdateList(CreateSystem<HurtSystem>());

        group.AddSystemToUpdateList(CreateSystem<MonsterSpawnSytem>());
        group.AddSystemToUpdateList(CreateSystem<MonsterAiSytem>());
        
        group.AddSystemToUpdateList(CreateSystem<SpawnTargetSystem>());
        
        group.AddSystemToUpdateList(CreateSystem<MoveByDirSystem>());

        var timeoutSystem = CreateSystem<GameTimeoutSystem>();
        timeoutSystem.flag = flag;
        group.AddSystemToUpdateList(timeoutSystem);

        // Job burst Check

        // cal CheckSum
        group.AddSystemToUpdateList(CreateSystem<CalHashSystem>());
    }

    public void InitPresentationSystem(LocalFrame localServer, int userId)
    {
        var group = GetOrCreateSystem<CustomSystems3>();
        GetOrCreateSystem<PresentationSystemGroup>().AddSystemToUpdateList(group);

        // add all simulation systems
        group.AddSystemToUpdateList(CreateSystem<UpdateWorldTimeSystem>());

        var system = CreateSystem<VSpawnTargetSystem>();
        system.UserId = userId;

        group.AddSystemToUpdateList(system);
        group.AddSystemToUpdateList(CreateSystem<VHpColorSystem>());
        group.AddSystemToUpdateList(CreateSystem<VDespawnSystem>());
        group.AddSystemToUpdateList(CreateSystem<VLerpTransformSystem>());
        group.AddSystemToUpdateList(CreateSystem<VCameraFollowSystem>());
    }
}

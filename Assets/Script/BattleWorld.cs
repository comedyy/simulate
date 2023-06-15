using System.Collections;
using System.Collections.Generic;
using Unity.Entities;


public class BattleWorld : World
{
    BattleEndFlag flag = new BattleEndFlag();
    public bool IsEnd => flag.isEnd;

    public BattleWorld(string name, CheckSum checksum, float logicFrameInterval, LocalFrame localServer, int userId) : base(name)
    {
        EntityManager.AddComponentData(EntityManager.CreateEntity(), new CheckSumComponet(){
            checkSum = checksum
        });

        EntityManager.AddComponentData(EntityManager.CreateEntity(), new BindingComponet());

        // init systems 
        InitiazationSystem.logicFrameInterval = logicFrameInterval;
        CreateSystem<InitiazationSystem>();
        // update systems
        InitSimulationSystem(checksum, localServer);

#if !ONLY_LOGIC
        InitPresentationSystem(localServer, userId);
#endif
    }


    public void InitSimulationSystem(CheckSum checksum, LocalFrame frame)
    {
        var group = GetOrCreateSystem<FixedTimeSystemGroup>();
        group.InitLogicTime(frame, flag);
        GetOrCreateSystem<Unity.Entities.SimulationSystemGroup>().AddSystemToUpdateList(group);

        var inputSystem = CreateSystem<InputUserPositionSystem>();
        inputSystem.GetAllMessage = frame.GetFrameInput;
        group.AddSystemToUpdateList(inputSystem); // 玩家输入


        group.AddSystemToUpdateList(CreateSystem<MonsterSpawnSytem>());
        group.AddSystemToUpdateList(CreateSystem<MonsterAiSytem>());
        // group.AddSystemToUpdateList(CreateSystem<MosnterDespawnSystem>());
        
        group.AddSystemToUpdateList(CreateSystem<SpawnTargetSystem>());
        
        // group.AddSystemToUpdateList(CreateSystem<RecordPrePositionSystem>());
        group.AddSystemToUpdateList(CreateSystem<MoveByDirSystem>());
        // group.AddSystemToUpdateList(CreateSystem<MoveByosSystem>());

        var timeoutSystem = CreateSystem<GameTimeoutSystem>();
        timeoutSystem.flag = flag;
        group.AddSystemToUpdateList(timeoutSystem);

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
        group.AddSystemToUpdateList(CreateSystem<VDespawnSystem>());
        group.AddSystemToUpdateList(CreateSystem<VLerpTransformSystem>());
        group.AddSystemToUpdateList(CreateSystem<VCameraFollowSystem>());

        var moveSytem = CreateSystem<ControllerMoveSystem>();
        moveSytem.localServer = localServer;
        group.AddSystemToUpdateList(moveSytem);
    }
}

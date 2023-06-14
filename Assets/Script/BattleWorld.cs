using System.Collections;
using System.Collections.Generic;
using Unity.Entities;


public class BattleWorld : World
{
    BattleEndFlag flag = new BattleEndFlag();
    public bool IsEnd => flag.isEnd;

    public BattleWorld(string name, CheckSum checksum, float logicFrameInterval, LocalFrame localServer) : base(name)
    {
        // init systems 
        CreateSystem<InitiazationSystem>();

        // update systems
        InitSimulationSystem(checksum, logicFrameInterval, localServer);

#if !ONLY_LOGIC
        InitPresentationSystem(localServer);
#endif
    }


    public void InitSimulationSystem(CheckSum checksum, float logicFrameInterval, LocalFrame frame)
    {
        var group = GetOrCreateSystem<FixedTimeSystemGroup>();
        group.InitLogicTime(logicFrameInterval, frame);
        GetOrCreateSystem<Unity.Entities.SimulationSystemGroup>().AddSystemToUpdateList(group);

        var inputSystem = CreateSystem<InputUserPositionSystem>();
        inputSystem.GetAllMessage = frame.GetFrameInput;
        group.AddSystemToUpdateList(inputSystem); // 玩家输入


        group.AddSystemToUpdateList(CreateSystem<MonsterSpawnSytem>());
        group.AddSystemToUpdateList(CreateSystem<MonsterAiSytem>());
        group.AddSystemToUpdateList(CreateSystem<MosnterDespawnSystem>());
        
        group.AddSystemToUpdateList(CreateSystem<SpawnTargetSystem>());
        
        group.AddSystemToUpdateList(CreateSystem<RecordPrePositionSystem>());
        group.AddSystemToUpdateList(CreateSystem<MoveByDirSystem>());
        group.AddSystemToUpdateList(CreateSystem<MoveByPosSystem>());

        var timeoutSystem = CreateSystem<GameTimeoutSystem>();
        timeoutSystem.flag = flag;
        group.AddSystemToUpdateList(timeoutSystem);

        // cal CheckSum
        var checksumSystem = CreateSystem<CalHashSystem>();
        checksumSystem._checkSum = checksum;
        group.AddSystemToUpdateList(checksumSystem);
    }

    public void InitPresentationSystem(LocalFrame localServer)
    {
        var group = GetOrCreateSystem<CustomSystems3>();
        GetOrCreateSystem<PresentationSystemGroup>().AddSystemToUpdateList(group);

        // add all simulation systems
        group.AddSystemToUpdateList(CreateSystem<UpdateWorldTimeSystem>());
        group.AddSystemToUpdateList(CreateSystem<VSpawnTargetSystem>());
        group.AddSystemToUpdateList(CreateSystem<VDespawnSystem>());
        group.AddSystemToUpdateList(CreateSystem<VLerpTransformSystem>());
        group.AddSystemToUpdateList(CreateSystem<VCameraFollowSystem>());

        var moveSytem = CreateSystem<ControllerMoveSystem>();
        moveSytem.localServer = localServer;
        group.AddSystemToUpdateList(moveSytem);
    }
}

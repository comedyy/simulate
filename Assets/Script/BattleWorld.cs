using System.Collections;
using System.Collections.Generic;
using Unity.Entities;


public class BattleWorld : World
{
    BattleEndFlag flag = new BattleEndFlag();
    public bool IsEnd => flag.isEnd;

    public BattleWorld(string name, CheckSum checksum) : base(name)
    {
        // init systems 
        CreateSystem<InitiazationSystem>();

        // update systems
        InitSimulationSystem(checksum);

#if !ONLY_LOGIC
        InitPresentationSystem();
#endif
    }


    public void InitSimulationSystem(CheckSum checksum)
    {
        var group = GetOrCreateSystem<FixedTimeSystemGroup>();
        GetOrCreateSystem<Unity.Entities.SimulationSystemGroup>().AddSystemToUpdateList(group);

        group.AddSystemToUpdateList(CreateSystem<InputUserPositionSystem>()); // 玩家输入


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

    public void InitPresentationSystem()
    {
        var group = GetOrCreateSystem<CustomSystems3>();
        GetOrCreateSystem<PresentationSystemGroup>().AddSystemToUpdateList(group);

        // add all simulation systems
        group.AddSystemToUpdateList(CreateSystem<UpdateWorldTimeSystem>());
        group.AddSystemToUpdateList(CreateSystem<VSpawnTargetSystem>());
        group.AddSystemToUpdateList(CreateSystem<VDespawnSystem>());
        group.AddSystemToUpdateList(CreateSystem<VLerpTransformSystem>());
        group.AddSystemToUpdateList(CreateSystem<VCameraFollowSystem>());
        group.AddSystemToUpdateList(CreateSystem<ControllerMoveSystem>());
    }
}

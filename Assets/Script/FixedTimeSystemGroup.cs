using System;
using System.Collections.Generic;
using Unity.Entities;

[DisableAutoCreation]
public class FixedTimeSystemGroup : ComponentSystemGroup
{
    float _battleStartTime;
    bool _firstTickFinished;
    LocalFrame _localFrame;
    BattleEndFlag _flag;
    bool _needRandomFixedCount;
    protected override void OnCreate()
    {
        base.OnCreate();

        _battleStartTime = UnityEngine.Time.time;
        _firstTickFinished = false;
    }

    internal void InitLogicTime(LocalFrame localFrame, BattleEndFlag flag, bool needRandomFixedCount)
    {
        _localFrame = localFrame;
        _flag = flag;
        _needRandomFixedCount = needRandomFixedCount;
    }
    
    protected override void OnUpdate()
    {
        var buffer = EntityManager.GetBuffer<DeSpawnEventComponent>(GetSingletonEntity<DeSpawnEventComponent>());
        buffer.Clear();
        var bufferSpawn = EntityManager.GetBuffer<SpawnEventComponent>(GetSingletonEntity<SpawnEventComponent>());
        bufferSpawn.Clear();
        var bufferHurt = EntityManager.GetBuffer<VHurtComponent>(GetSingletonEntity<VHurtComponent>());
        bufferHurt.Clear();

        int count = UnityEngine.Random.Range(0, 5);

        var elapsedTime = UnityEngine.Time.time - _battleStartTime;
        while (true)
        {
            var logicTime = GetSingleton<LogicTime>();
            var lastTime = logicTime.escaped;
            if(logicTime.frameCount >= _localFrame.ReceivedServerFrame)
            {
                break;
            }

            if(_flag.isEnd) return; // 游戏已经结束

            var condition = true;
            if(_needRandomFixedCount) condition = count-- > 0;
            else condition = !_firstTickFinished || elapsedTime - lastTime >= logicTime.deltaTime;

            if (condition)
            // if((count--) > 0)
            {
                _firstTickFinished = true;
                
                logicTime.escaped += logicTime.deltaTime;
                logicTime.frameCount ++;
                SetSingleton(logicTime);

                World.SetTime(new Unity.Core.TimeData(float.NaN, float.NaN));   // 用来防止别人用到了Time对象，需要使用LogicTime， 更早发现问题

                base.OnUpdate();  // Update
            }
            else
            {
                break;
            }
        }
    }

    
    public override void SortSystemUpdateList()
    {
        var backup = new List<ComponentSystemBase>();
        backup.AddRange( m_systemsToUpdate );
        base.SortSystemUpdateList(  );
        m_systemsToUpdate.Clear(  );
        m_systemsToUpdate.AddRange( backup );
    }
}
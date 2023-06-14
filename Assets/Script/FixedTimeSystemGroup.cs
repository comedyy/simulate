using System;
using System.Collections.Generic;
using Unity.Entities;

public class FixedTimeSystemGroup : ComponentSystemGroup
{
    float _battleStartTime;
    bool _firstTickFinished;
    LocalFrame _localFrame;
    protected override void OnCreate()
    {
        base.OnCreate();

        _battleStartTime = UnityEngine.Time.time;
        _firstTickFinished = false;
    }
    
    internal void InitLogicTime(LocalFrame localFrame)
    {
        _localFrame = localFrame;
    }
    
    protected override void OnUpdate()
    {
        var elapsedTime = UnityEngine.Time.time - _battleStartTime;
        while (true)
        {
            var logicTime = GetSingleton<LogicTime>();
            var lastTime = logicTime.escaped;
            if(logicTime.frameCount >= _localFrame.ReceivedServerFrame)
            {
                break;
            }

            if (!_firstTickFinished || elapsedTime - lastTime >= logicTime.deltaTime)
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
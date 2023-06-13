using System.Collections.Generic;
using Unity.Entities;

public class FixedTimeSystemGroup : ComponentSystemGroup
{
    protected override void OnCreate()
    {
        base.OnCreate();
        EntityManager.AddComponentData(EntityManager.CreateEntity(), new LogicTime(){
            deltaTime = 0.1f
        });
    }
    
    protected override void OnUpdate()
    {
        var elapsedTime = World.Time.ElapsedTime;
        while (true)
        {
            var logicTime = GetSingleton<LogicTime>();
            var lastTime = logicTime.escaped;

            var isFirstTick = elapsedTime == 0 && lastTime == 0;
            if (isFirstTick || elapsedTime - lastTime >= logicTime.deltaTime)
            {
                logicTime.escaped += logicTime.deltaTime;
                logicTime.frameCount ++;
                SetSingleton(logicTime);

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
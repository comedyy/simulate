using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Game.Battle.CommonLib;
using RVO;
using Unity.Collections;
using Unity.Entities;

[StructLayout(LayoutKind.Sequential)]
public struct rvoInt2
{
    public int x;
    public int y;
}

public class RvoSimulatorComponet  : IComponentData
{
    public int id;
    RVO.Simulator rvoSimulator;
    Dictionary<int, Entity> _allEntities = new Dictionary<int, Entity>();

    public int AddAgent(Vector2 pos, fp neighborDist, int maxNeighbors, fp timeHorizon, fp timeHorizonObst, fp radius, fp maxSpeed, Vector2 velocity, Entity entity)
    {
        if(rvoSimulator != null) rvoSimulator.addAgent(pos, neighborDist, maxNeighbors, timeHorizon, timeHorizonObst, radius, maxSpeed, velocity, entity);

        var velocity1 = new rvoInt2(){ x = (int)velocity.x().rawValue, y = (int)velocity.y().rawValue};
        var agentIndex = MSPathSystem.AddAgent(id, pos.x(), pos.y(), neighborDist, maxNeighbors, timeHorizon, timeHorizonObst, radius, maxSpeed, 1, velocity.x(), velocity.y());
        
        _allEntities.Add(agentIndex, entity);

        return agentIndex;
    }

    public void setTimeStep(fp stepTime)
    {
        if(rvoSimulator != null) 
        {
            rvoSimulator.setTimeStep(stepTime); return;
        }

        MSPathSystem.SetTimeStep(id, stepTime);
    }

    public void RemoveAgent(int index)
    {
        if(rvoSimulator != null) 
        {
            rvoSimulator.removeAgent(index); return;
        }

        MSPathSystem.RemoveAgent(id, index);

        _allEntities.Remove(index);
    }

    public void DoStep()
    {
        if(rvoSimulator != null) 
        {
            rvoSimulator.doStep(); return;
        }

        MSPathSystem.DoStep(id);
    }


    public void SetAgentPrefVelocity(int index, Vector2 vec)
    {
        if(rvoSimulator != null)
        {
            rvoSimulator.setAgentPrefVelocity(index, vec); return;
        } 

        MSPathSystem.SetAgentVelocityPref(id, index, vec.x(), vec.y());
    }

    public Vector2 GetAgentPosition(int index)
    {
        if(rvoSimulator != null) 
        {
            return rvoSimulator.getAgentPosition(index);
        }

        var pos = new AgentVector2();
        MSPathSystem.GetAgentPosition(id, index, ref pos);
        var x = new fp(){rawValue = UnityEngine.Mathf.FloorToInt(pos.x * fp.one.rawValue)};
        var y = new fp(){rawValue = UnityEngine.Mathf.FloorToInt(pos.y * fp.one.rawValue)};
        return new Vector2(x, y);
    }

    public void FindAgents(Vector2 pos, fp range, List<Entity> list)
    {
        if(rvoSimulator != null)
        {
            rvoSimulator.FindAgents(pos, range, list); return;
        }

        NativeArray<int> array = new Unity.Collections.NativeArray<int>(1024, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        int count = MSPathSystem.GetNearByAgents(id, pos.x(), pos.y(), array, range);

        for(int i = 0; i < count; i++)
        {
            var x = array[i];
            list.Add(_allEntities[x]);
        }
    }

    internal void ShutDown()
    {
        if(rvoSimulator != null) return;

        MSPathSystem.Shutdown(id);
    }
}

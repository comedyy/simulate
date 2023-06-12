using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct VLerpTransformCopmnet : IComponentData
{
    public float3 preLogicPos;
    public quaternion preLogicRatation;
    public float lerpTime;
}

public class GameObjectBindingComponent : IComponentData, IEquatable<GameObjectBindingComponent>
{
    public GameObject obj;

    public bool Equals(GameObjectBindingComponent other)
    {
        return other.obj == obj;
    }

    public override int GetHashCode()
    {
        return obj.GetHashCode();
    }
}

public struct VSpawnEvent : IComponentData
{
    public Entity target;
    public bool isUser;
}

public struct VDespawnEvent : IComponentData
{
    public Entity target;
}


public struct ControllerHolder : IComponentData
{
    public Entity controller;
}

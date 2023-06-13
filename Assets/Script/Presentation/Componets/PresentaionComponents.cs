using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class GameObjectBindingComponent : IComponentData, IEquatable<GameObjectBindingComponent>
{
    public GameObject obj;
    public GameObject objFollow;

    public bool Equals(GameObjectBindingComponent other)
    {
        return other.obj == obj;
    }

    public override int GetHashCode()
    {
        return obj.GetHashCode();
    }
}

public struct ControllerHolder : IComponentData
{
    public Entity controller;
}

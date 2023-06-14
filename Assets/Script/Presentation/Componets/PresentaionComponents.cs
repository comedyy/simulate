using System;
using System.Collections.Generic;
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

public class BindingComponet : IComponentData
{
    public Dictionary<Entity, GameObject> allObject = new Dictionary<Entity, GameObject>();
}
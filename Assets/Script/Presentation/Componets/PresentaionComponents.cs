using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BindingComponet : IComponentData
{
    public Dictionary<Entity, GameObject> allObject = new Dictionary<Entity, GameObject>();
}
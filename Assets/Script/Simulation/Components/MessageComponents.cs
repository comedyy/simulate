using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct MessageItem
{
    public int3 pos;
    public int id;
}
using System.Collections.Generic;
using System.IO;
using Unity.Entities;
using Unity.Mathematics;

public enum MsgType{
    StartBattle = 1,
    FrameMsg = 2,
    ServerFrameMsg = 3,
    HashMsg = 4,
}

public struct FrameHashItem
{
    public int hash;
    public List<int> listValue;
    public List<Entity> listEntity;

    public void Write(BinaryWriter writer)
    {
        writer.Write(hash);
        writer.Write(listValue.Count);
        for(int i = 0; i < listValue.Count; i++)
        {
            writer.Write(listValue[i]);
        }

        writer.Write(listEntity.Count);
        for(int i = 0; i < listEntity.Count; i++)
        {
            writer.Write(listEntity[i].Index);
            writer.Write(listEntity[i].Version);
        }
    }

    public static FrameHashItem GetItem(BinaryReader reader)
    {
        var x = new FrameHashItem();
        x.hash = reader.ReadInt32();
        var listCount = reader.ReadInt32();
        x.listValue = new List<int>();
        for(int i = 0; i < listCount; i++)
        {
            x.listValue.Add(reader.ReadInt32());
        }

        var listCount1 = reader.ReadInt32();
        x.listEntity = new List<Entity>();
        for(int i = 0; i < listCount1; i++)
        {
            x.listEntity.Add(new Entity(){
                Index = reader.ReadInt32(),
                Version = reader.ReadInt32(),
            });
        }

        return x;
    } 

    public static bool operator==(FrameHashItem item1, FrameHashItem item2)
    {
        if(item1.hash != item2.hash) return false;

        if(item1.listEntity.Count != item2.listEntity.Count)return false;
        for(int i =0; i < item1.listEntity.Count; i++)
        {
            if(item1.listEntity[i] != item1.listEntity[i]) return false;
        }
        
        if(item1.listValue.Count != item2.listValue.Count)return false;
        for(int i =0; i < item1.listValue.Count; i++)
        {
            if(item1.listValue[i] != item1.listValue[i]) return false;
        }

        return true;
    }

    public static bool operator!=(FrameHashItem item1, FrameHashItem item2)
    {
        return !(item1 == item2);
    }

    public override string ToString()
    {
        return $"Hash:{hash} listValue：{string.Join("!", listValue)} listEntity：{string.Join("!", listEntity)}";
    }
}

public struct FrameHash
{
    public int frame;
    public int id;
    public FrameHashItem hashPos;
    public FrameHashItem hashHp;
    public FrameHashItem hashFindtarget;
    public FrameHashItem preRvo;

    public byte[] ToBytes()
    {
        MemoryStream steam = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(steam);
        writer.Write((byte)MsgType.HashMsg);
        writer.Write(frame);
        writer.Write(id);
        hashPos.Write(writer);
        hashHp.Write(writer);
        hashFindtarget.Write(writer);
        preRvo.Write(writer);

        return steam.ToArray();
    }

    public void From(byte[] bytes)
    {
        MemoryStream steam = new MemoryStream(bytes);
        BinaryReader reader = new BinaryReader(steam);
        var msgType = reader.ReadByte();
        frame = reader.ReadInt32();
        id = reader.ReadInt32();
        hashPos = FrameHashItem.GetItem(reader);
        hashHp = FrameHashItem.GetItem(reader);
        hashFindtarget = FrameHashItem.GetItem(reader);
        preRvo = FrameHashItem.GetItem(reader);
    }
}


public struct PackageItem
{
    public int frame;
    public MessageItem messageItem;

    public byte[] ToBytes()
    {
        MemoryStream steam = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(steam);
        writer.Write((byte)MsgType.FrameMsg);
        writer.Write(frame);
        writer.Write(messageItem.id);
        writer.Write(messageItem.pos.x);
        writer.Write(messageItem.pos.y);
        writer.Write(messageItem.pos.z);

        return steam.ToArray();
    }

    public void From(byte[] bytes)
    {
        MemoryStream steam = new MemoryStream(bytes);
        BinaryReader reader = new BinaryReader(steam);
        var msgType = reader.ReadByte();
        frame = reader.ReadInt32();
        messageItem = new MessageItem(){
            id = reader.ReadInt32(),
            pos = new int3(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32())
        };
    }
}

public struct BattleStartMessage
{
    public int total;
    public int userId;

    public byte[] ToBytes()
    {
        MemoryStream steam = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(steam);
        writer.Write((byte)MsgType.StartBattle);
        writer.Write(total);
        writer.Write(userId);

        return steam.ToArray();
    }

    public void From(byte[] bytes)
    {
        MemoryStream steam = new MemoryStream(bytes);
        BinaryReader reader = new BinaryReader(steam);
        var msgType = reader.ReadByte();
        total = reader.ReadInt32();
        userId = reader.ReadInt32();
    }
}

public struct ServerPackageItem
{
    public int frame;
    public List<MessageItem> list;

    public byte[] ToBytes()
    {
        MemoryStream steam = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(steam);
        writer.Write((byte)MsgType.ServerFrameMsg);
        writer.Write(frame);
        if(list != null) 
        {
            writer.Write(list.Count);
            for(int i = 0; i < list.Count; i++)
            {
                var messageItem = list[i];
                writer.Write(messageItem.id);
                writer.Write(messageItem.pos.x);
                writer.Write(messageItem.pos.y);
                writer.Write(messageItem.pos.z);
            }
        }
        else
        {
            writer.Write(0);
        }

        return steam.ToArray();
    }

    public void From(byte[] bytes)
    {
        MemoryStream steam = new MemoryStream(bytes);
        BinaryReader reader = new BinaryReader(steam);
        var msgType = reader.ReadByte();
        frame = reader.ReadInt32();
        var count = reader.ReadInt32();
        if(count > 0)
        {
            list = new List<MessageItem>();
            for(int i = 0; i < count; i++)
            {
                var messageItem = new MessageItem(){
                    id = reader.ReadInt32(),
                    pos = new int3(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32())
                };
                list.Add(messageItem);
            }
        }
    }
}

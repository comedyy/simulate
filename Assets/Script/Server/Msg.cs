using System.Collections.Generic;
using System.IO;

public enum MsgType{
    StartBattle = 1,
    FrameMsg = 2,
    ServerFrameMsg = 3,
    HashMsg = 4,
}

public struct FrameHash
{
    public int frame;
    public int id;
    public int hashPos;
    public int hashHp;
    public int hashFindtarget;

    public byte[] ToBytes()
    {
        MemoryStream steam = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(steam);
        writer.Write((byte)MsgType.HashMsg);
        writer.Write(frame);
        writer.Write(id);
        writer.Write(hashPos);
        writer.Write(hashHp);
        writer.Write(hashFindtarget);

        return steam.ToArray();
    }

    public void From(byte[] bytes)
    {
        MemoryStream steam = new MemoryStream(bytes);
        BinaryReader reader = new BinaryReader(steam);
        var msgType = reader.ReadByte();
        frame = reader.ReadInt32();
        id = reader.ReadInt32();
        hashPos = reader.ReadInt32();
        hashHp = reader.ReadInt32();
        hashFindtarget = reader.ReadInt32();
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
        writer.Write((int)(messageItem.pos.x * 100));
        writer.Write((int)(messageItem.pos.y * 100));
        writer.Write((int)(messageItem.pos.z * 100));

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
            pos = new Unity.Mathematics.float3()
            {
                x = reader.ReadInt32() / 100f,
                y = reader.ReadInt32() / 100f,
                z = reader.ReadInt32() / 100f,
            }
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
                writer.Write((int)(messageItem.pos.x * 100));
                writer.Write((int)(messageItem.pos.y * 100));
                writer.Write((int)(messageItem.pos.z * 100));
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
                    pos = new Unity.Mathematics.float3()
                    {
                        x = reader.ReadInt32() / 100f,
                        y = reader.ReadInt32() / 100f,
                        z = reader.ReadInt32() / 100f,
                    }
                };
                list.Add(messageItem);
            }
        }
    }
}

using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class NetMakeMove : NetMessage
{
    public int originalX;
    public int originalY;
    public int DestinationX;
    public int DestinationY;
    public int TeamID;
    public NetMakeMove()
    {
        Code = OpCode.Make_Move;

    }

    public NetMakeMove(DataStreamReader Reader)
    {
        Code = OpCode.Make_Move;
        DeSerialise(Reader);
    }

    public override void Serialise(ref DataStreamWriter Writer)
    {
        Writer.WriteByte((byte)Code);
        Writer.WriteInt(originalX);
        Writer.WriteInt(originalY);
        Writer.WriteInt(DestinationX);
        Writer.WriteInt(DestinationY);
        Writer.WriteInt(TeamID);
    }
    public override void DeSerialise(DataStreamReader Reader)
    {
        originalX = Reader.ReadInt();
        originalY = Reader.ReadInt();
        DestinationX = Reader.ReadInt();
        DestinationY = Reader.ReadInt();
        TeamID = Reader.ReadInt();
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_Make_Move?.Invoke(this);
    }
    public override void ReceivedOnServer(NetworkConnection CNN)
    {
        NetUtility.S_Make_Move?.Invoke(this, CNN);
    }
}

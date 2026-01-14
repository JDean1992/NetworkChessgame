using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class NetRematch : NetMessage
{
    
    public int TeamID;
    public byte WantRematch;
    public NetRematch()
    {
        Code = OpCode.Make_Move;

    }

    public NetRematch(DataStreamReader Reader)
    {
        Code = OpCode.Rematch;
        DeSerialise(Reader);
    }

    public override void Serialise(ref DataStreamWriter Writer)
    {
        Writer.WriteByte((byte)Code);
        Writer.WriteInt(TeamID);
        Writer.WriteByte(WantRematch);
    }
    public override void DeSerialise(DataStreamReader Reader)
    {
        
        TeamID = Reader.ReadInt();
        WantRematch = Reader.ReadByte();
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_Rematch?.Invoke(this);
    }
    public override void ReceivedOnServer(NetworkConnection CNN)
    {
        NetUtility.S_Rematch?.Invoke(this, CNN);
    }
}

using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class NetWelcome : NetMessage
{
    public int AssignedTeam { set; get; }

    public NetWelcome()
    {
        Code = OpCode.Welcome;
    }

    public NetWelcome(DataStreamReader reader)
    {
        Code = OpCode.Welcome;
        DeSerialise(reader);
    }

    public override void Serialise(ref DataStreamWriter Writer)
    {
        Writer.WriteByte((byte)Code);
        Writer.WriteInt(AssignedTeam);
    }
    public override void DeSerialise(DataStreamReader Reader)
    {
        AssignedTeam = Reader.ReadInt();
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_Welcome?.Invoke(this);
    }
    public override void ReceivedOnServer(NetworkConnection CNN)
    {
        NetUtility.S_Welcome?.Invoke(this, CNN);
    }
}

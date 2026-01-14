using Unity.Collections;
using Unity.Networking.Transport;

public class NetKeepAlive : NetMessage
{
    public NetKeepAlive()
    {
        Code = OpCode.Keep_Alive;

    }

    public NetKeepAlive(DataStreamReader Reader)
    {
        Code= OpCode.Keep_Alive;
        DeSerialise(Reader);
    }

    public override void Serialise(ref DataStreamWriter Writer)
    {
        Writer.WriteByte((byte)Code);
    }
    public override void DeSerialise(DataStreamReader Reader)
    {
        
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_Keep_Alive?.Invoke(this);
    }
    public override void ReceivedOnServer(NetworkConnection CNN)
    {
        NetUtility.S_Keep_Alive?.Invoke(this, CNN);
    }
}

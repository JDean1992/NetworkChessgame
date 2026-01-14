using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class NetStartGame : NetMessage
{
   

    public NetStartGame()
    {
        Code = OpCode.Start_Game;
    }

    public NetStartGame(DataStreamReader reader)
    {
        Code = OpCode.Start_Game;
        DeSerialise(reader);
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
        NetUtility.C_Start_Game?.Invoke(this);
    }
    public override void ReceivedOnServer(NetworkConnection CNN)
    {
        NetUtility.S_Start_Game?.Invoke(this, CNN);
    }
}

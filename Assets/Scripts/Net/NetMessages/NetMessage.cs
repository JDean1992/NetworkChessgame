using System.Reflection.Emit;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;



public class NetMessage 
{
    public OpCode Code { set; get; }

    public virtual void Serialise(ref DataStreamWriter Writer)
    {
        Writer.WriteByte((byte)Code);
    }

    public virtual void DeSerialise(DataStreamReader Reader)
    {

    }

    public virtual void ReceivedOnClient()
    {

    }

    public virtual void ReceivedOnServer(NetworkConnection CNN)
    {

    }
}

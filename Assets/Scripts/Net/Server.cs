using System;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.VisualScripting;
using UnityEngine;


public class Server : MonoBehaviour
{
    #region singleton implementation
    public static Server instance { set; get; }

    private void Awake()
    {
        instance = this;
    }
    #endregion

    public NetworkDriver Driver;
    public NativeList<NetworkConnection> Connections;

    private bool IsActive = false;
    private const float KeepAliveTickRate = 20.0f;
    private float LastKeptAlive;

    public Action ConnectionDropped;

    //methods
    public void Init(ushort Port)
    {
        Driver = NetworkDriver.Create();
        NetworkEndpoint EndPoint = NetworkEndpoint.AnyIpv4;
        EndPoint.Port = Port;

        if(Driver.Bind(EndPoint) != 0)
        {
            Debug.Log("Unable to bind on port" + EndPoint.Port);
            return;
        }
        else
        {
            Driver.Listen();
            Debug.Log("Currently listiening on port" + EndPoint.Port);
        }

        Connections = new NativeList<NetworkConnection>(2, Allocator.Persistent);
        IsActive = true;
    }

    public void Shutdown()
    {
        if(IsActive)
        {
            Driver.Dispose();
            Connections.Dispose();
            IsActive = false;
        }
    }

    public void OnDestroy()
    {
        Shutdown();
    }

    public void Update()
    {
        if (!IsActive)
            return;

        KeepAlive();

        Driver.ScheduleUpdate().Complete();
        CleanUPConnections();
        AcceptNewConnections();
        UpdateMessagePump();
    }

    private void KeepAlive()
    {
        if(Time.time - LastKeptAlive > KeepAliveTickRate)
        {
            LastKeptAlive = Time.time;
            Brodcast(new NetKeepAlive());
        }
    }

    private void CleanUPConnections()
    {
        for(int i = 0; i < Connections.Length; i++)
        {
            if (!Connections[i].IsCreated)
            {
                Connections.RemoveAtSwapBack(i);
                --i;
            }
        }
    }

    private void AcceptNewConnections()
    {
        // accepts new connections
        NetworkConnection C;
        while((C = Driver.Accept()) != default(NetworkConnection))
        {
            Connections.Add(C);
        }
    }

    private void UpdateMessagePump()
    {
        DataStreamReader Stream;
        for(int i = 0; i < Connections.Length;i++)
        {
                 NetworkEvent.Type CMD;
                while((CMD = Driver.PopEventForConnection(Connections[i], out Stream)) != NetworkEvent.Type.Empty)
            {
                        if(CMD == NetworkEvent.Type.Data)
                    {
                    NetUtility.OnData(Stream, Connections[i], this);
                    }
                         else if(CMD == NetworkEvent.Type.Disconnect)
                    {
                    Debug.Log(" client has disconnected");
                    Connections[i] = default(NetworkConnection);
                    ConnectionDropped?.Invoke();
                    Shutdown();
                    }
            }
                   
            
        }
    }


    public void SendToClient(NetworkConnection connection, NetMessage msg)
    {
        DataStreamWriter Writer;
        Driver.BeginSend(connection, out Writer);
        msg.Serialise(ref Writer);
        Driver.EndSend(Writer);
    }

    public void Brodcast(NetMessage msg)
    {
        for(int i = 0; i < Connections.Length; i++)
        {
            if (Connections[i].IsCreated)
            {
                //Debug.Log($"Sending{msg.Code} to : {Connections[i].InternalId}");
                SendToClient(Connections[i], msg);
            }
        }
    }

    
}

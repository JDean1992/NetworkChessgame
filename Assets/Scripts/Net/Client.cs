using System;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class Client : MonoBehaviour
{
    #region singleton implementation
    public static Client instance { set; get; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            //makes sure only one client exists
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    public NetworkDriver Driver;
    public NetworkConnection Connection;


    private bool IsActive;

    public Action ConnectionDropped;

    


    public void Init(string IP, ushort Port)
    {
        //creates the driver and connects to server and marks the client active
        Driver = NetworkDriver.Create();
        NetworkEndpoint EndPoint = NetworkEndpoint.Parse(IP, Port);

        Connection = Driver.Connect(EndPoint);

        Debug.Log("Attempting to connect to server on " + EndPoint.Address); 
        IsActive = true;

        RegisterToEvent();
    }

    public void Shutdown()
    {
        if (IsActive)
        {
            UnRegisterToEvent();
            Driver.Dispose();
            IsActive = false;
            Connection = default(NetworkConnection);
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

        

        Driver.ScheduleUpdate().Complete();
        CheckAlive();
        
        
        UpdateMessagePump();
    }

    private void CheckAlive()
    {
        if(!Connection.IsCreated && IsActive)
            {
            Debug.Log("Lost Connection to server");
            ConnectionDropped?.Invoke();
            Shutdown();
            }
        
    }

    private void UpdateMessagePump()
    {
        DataStreamReader Stream;
         NetworkEvent.Type CMD;
            while ((CMD = Connection.PopEvent(Driver, out Stream)) != NetworkEvent.Type.Empty)
            {
                if (CMD == NetworkEvent.Type.Connect)
                {
                SendToServer(new NetWelcome());
                Debug.Log("we are connected");
                }
                else if (CMD == NetworkEvent.Type.Data)
                {
                NetUtility.OnData(Stream, default(NetworkConnection));
                }
                else if (CMD == NetworkEvent.Type.Disconnect)
                {
                Debug.Log("Client got disconnected");
                Connection = default(NetworkConnection);
                ConnectionDropped?.Invoke();
                Shutdown();
                }
            }
    }
    public void SendToServer(NetMessage msg)
    {
        if (!IsActive)
        {
            Debug.LogWarning("Client not active, cannot send");
            return;
        }

        if (!Driver.IsCreated)
        {
            Debug.LogWarning("Driver not created");
            return;
        }

        if (!Connection.IsCreated)
        {
            Debug.LogWarning("Connection not created");
            return;
        }

        DataStreamWriter writer;
        Driver.BeginSend(Connection, out writer);
        msg.Serialise(ref writer);
        Driver.EndSend(writer);
    }

    private void RegisterToEvent()
    {
       NetUtility.C_Keep_Alive += OnKeepAlive;
    }

    private void UnRegisterToEvent()
    {
        NetUtility.C_Keep_Alive -= OnKeepAlive;
    }
    private void OnKeepAlive(NetMessage msg)
    {
        SendToServer(msg);
    }

}

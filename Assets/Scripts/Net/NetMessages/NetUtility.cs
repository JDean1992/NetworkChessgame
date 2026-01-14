using System;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public enum OpCode
{
    Keep_Alive = 1,
    Welcome = 2,
    Start_Game = 3,
    Make_Move = 4,
    Rematch = 5,
    Promotion = 6
     
}

public static class NetUtility
{
    public static void OnData(DataStreamReader Stream, NetworkConnection Connection, Server Server = null)
    {
        NetMessage msg = null;
        var opCode = (OpCode)Stream.ReadByte();
        switch (opCode)
        {
            case OpCode.Keep_Alive: msg = new NetKeepAlive(Stream); break;
            case OpCode.Welcome: msg = new NetWelcome(Stream); break;
            case OpCode.Start_Game: msg = new NetStartGame(Stream); break;
            case OpCode.Make_Move: msg = new NetMakeMove(Stream); break;
            case OpCode.Rematch: msg = new NetRematch(Stream); break;
            case OpCode.Promotion: msg = new NetPromotion(Stream); break;
            default:
                Debug.Log("Message received has no opcode");
                break;

        }
        if (Server != null)
            msg.ReceivedOnServer(Connection);
        else
            msg.ReceivedOnClient();

    }



    //net messages

    public static Action<NetMessage> C_Keep_Alive;
    public static Action<NetMessage> C_Welcome;
    public static Action<NetMessage> C_Start_Game;
    public static Action<NetMessage> C_Make_Move;
    public static Action<NetMessage> C_Rematch;
    public static Action<NetMessage> C_Promotion;
    public static Action<NetMessage, NetworkConnection> S_Keep_Alive;
    public static Action<NetMessage, NetworkConnection> S_Welcome;
    public static Action<NetMessage, NetworkConnection> S_Start_Game;
    public static Action<NetMessage, NetworkConnection> S_Make_Move;
    public static Action<NetMessage, NetworkConnection> S_Rematch;
    public static Action<NetMessage, NetworkConnection> S_Promotion;

}

using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class NetPromotion : NetMessage
{
    public Vector2Int Position;  
    public ChessPieceType NewType; 
    public int TeamID;            

    public NetPromotion()
    {
        Code = OpCode.Promotion; 
    }

    public NetPromotion(DataStreamReader Reader)
    {
        Code = OpCode.Promotion;
        DeSerialise(Reader);
    }

    public override void Serialise(ref DataStreamWriter writer)
    {
        writer.WriteInt(TeamID);
        writer.WriteInt((int)NewType);
        writer.WriteInt(Position.x);
        writer.WriteInt(Position.y);
    }

    public override void DeSerialise(DataStreamReader reader)
    {
        TeamID = reader.ReadInt();
        NewType = (ChessPieceType)reader.ReadInt();
        Position = new Vector2Int(reader.ReadInt(), reader.ReadInt());
    }
}

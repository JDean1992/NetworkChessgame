using System.Collections.Generic;
using UnityEngine;

public enum ChessPieceType
{
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4,
    Queen = 5,
    King = 6
}



public class ChessPiece : MonoBehaviour
{
    public int Team;
    public int CurrentX;
    public int CurrentY;
    public ChessPieceType PieceType;

    private Vector3 DesiredPosition;
    private Vector3 DesiredScale = Vector3.one;

    private void Start()
    {
        //rotates the black pieces to face centre of the board
        transform.rotation = Quaternion.Euler((Team == 0) ? Vector3.zero : new Vector3(0, 180, 0));
    }

    private void Update()
    {
       transform.position = Vector3.Lerp(transform.position, DesiredPosition, Time.deltaTime * 10);
        transform.localScale = Vector3.Lerp(transform.localScale, DesiredScale, Time.deltaTime * 10);
    }
    //returns a list of the avalable moves for selected piece
    public virtual List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int TileCountX, int TileCountY)
    {
        List<Vector2Int> ReturnValue = new List<Vector2Int>();
        ReturnValue.Add(new Vector2Int(3, 3));
        ReturnValue.Add(new Vector2Int(3, 4));
        ReturnValue.Add(new Vector2Int(4, 3));
        ReturnValue.Add(new Vector2Int(4, 4));

        return ReturnValue;
    }

    public virtual SpecialMoves GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> MoveList, ref List<Vector2Int> AvailableMoves)
    {
        return SpecialMoves.None;
    }


    public virtual void SetPosition(Vector3 Position, bool force = false)
    {
        DesiredPosition = Position;
        if (force)
        {
            transform.position = DesiredPosition;
        }
    }

    public virtual void SetScale(Vector3 Scale, bool force = false)
    {
        DesiredScale = Scale;
        if (force)
        {
            transform.localScale = DesiredScale;
        }
    }
}

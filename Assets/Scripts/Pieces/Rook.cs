using System.Collections.Generic;
using UnityEngine;

public class Rook : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int TileCountX, int TileCountY)
    {
        List<Vector2Int> ReturnValues = new List<Vector2Int>();

        // DOWN
        for (int y = CurrentY - 1; y >= 0; y--)
        {
            if (board[CurrentX, y] == null)
                ReturnValues.Add(new Vector2Int(CurrentX, y));
            else
            {
                if (board[CurrentX, y].Team != Team)
                    ReturnValues.Add(new Vector2Int(CurrentX, y));
                break;
            }
        }

        // UP
        for (int y = CurrentY + 1; y < TileCountY; y++)
        {
            if (board[CurrentX, y] == null)
                ReturnValues.Add(new Vector2Int(CurrentX, y));
            else
            {
                if (board[CurrentX, y].Team != Team)
                    ReturnValues.Add(new Vector2Int(CurrentX, y));
                break;
            }
        }

        // LEFT
        for (int x = CurrentX - 1; x >= 0; x--)
        {
            if (board[x, CurrentY] == null)
                ReturnValues.Add(new Vector2Int(x, CurrentY));
            else
            {
                if (board[x, CurrentY].Team != Team)
                    ReturnValues.Add(new Vector2Int(x, CurrentY));
                break;
            }
        }

        // RIGHT
        for (int x = CurrentX + 1; x < TileCountX; x++)
        {
            if (board[x, CurrentY] == null)
                ReturnValues.Add(new Vector2Int(x, CurrentY));
            else
            {
                if (board[x, CurrentY].Team != Team)
                    ReturnValues.Add(new Vector2Int(x, CurrentY));
                break;
            }
        }

        return ReturnValues;
    }
}

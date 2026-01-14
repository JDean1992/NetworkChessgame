using System.Collections.Generic;
using UnityEngine;

public class Knight : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int TileCountX, int TileCountY)
    {
        List<Vector2Int> ReturnValues = new List<Vector2Int>();


        //Top Right
        int x = CurrentX + 1;
        int y = CurrentY + 2;

        if(x < TileCountX && y < TileCountY)
        {
            if (board[x,y] == null || board[x,y].Team != Team)
                ReturnValues.Add(new Vector2Int(x,y));
        }

        x = CurrentX + 2;
        y = CurrentY + 1;

        if (x < TileCountX && y < TileCountY)
        {
            if (board[x, y] == null || board[x, y].Team != Team)
                ReturnValues.Add(new Vector2Int(x, y));
        }


        //Top Left

        x = CurrentX - 1;
        y = CurrentY + 2;

        if (x >= 0 && y < TileCountY)
        {
            if (board[x, y] == null || board[x, y].Team != Team)
                ReturnValues.Add(new Vector2Int(x, y));
        }

        x = CurrentX - 2;
        y = CurrentY + 1;

        if (x >= 0 && y < TileCountY)
        {
            if (board[x, y] == null || board[x, y].Team != Team)
                ReturnValues.Add(new Vector2Int(x, y));
        }


        //Bottom Right

        x = CurrentX + 1;
        y = CurrentY - 2;

        if (x < TileCountX && y >= 0)
        {
            if (board[x, y] == null || board[x, y].Team != Team)
                ReturnValues.Add(new Vector2Int(x, y));
        }

        x = CurrentX + 2;
        y = CurrentY - 1;

        if (x < TileCountX && y >= 0)
        {
            if (board[x, y] == null || board[x, y].Team != Team)
                ReturnValues.Add(new Vector2Int(x, y));
        }

        //Bottom Left

        x = CurrentX - 1;
        y = CurrentY - 2;

        if (x >= 0 && y >= 0)
            if (board[x, y] == null || board[x, y].Team != Team)
                ReturnValues.Add(new Vector2Int(x, y));

        x = CurrentX - 2;
        y = CurrentY - 1;

        if (x >= 0 && y >= 0)
            if (board[x, y] == null || board[x, y].Team != Team)
                ReturnValues.Add(new Vector2Int(x, y));




        return ReturnValues;
    }
}
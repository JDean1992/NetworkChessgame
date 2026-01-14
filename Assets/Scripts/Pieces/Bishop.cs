using System.Collections.Generic;
using UnityEngine;

public class Bishop : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int TileCountX, int TileCountY)
    {
        List<Vector2Int> ReturnValues = new List<Vector2Int>();


        //Top Right
        for(int x = CurrentX + 1, y = CurrentY + 1; x < TileCountX && y < TileCountY; x++, y++)
        {
            if (board[x, y] == null)
                ReturnValues.Add(new Vector2Int(x, y));

            else
            {
                if (board[x, y].Team != Team)
                    ReturnValues.Add(new Vector2Int(x, y));

                break;
            }
            
        }

        //Top Left
        for (int x = CurrentX - 1, y = CurrentY + 1; x >= 0 && y < TileCountY; x--, y++)
        {
            if (board[x, y] == null)
                ReturnValues.Add(new Vector2Int(x, y));

            else
            {
                if (board[x, y].Team != Team)
                    ReturnValues.Add(new Vector2Int(x, y));

                break;
            }

        }

        //Bottom Right
        for (int x = CurrentX + 1, y = CurrentY - 1; x < TileCountX && y >= 0; x++, y--)
        {
            if (board[x, y] == null)
                ReturnValues.Add(new Vector2Int(x, y));

            else
            {
                if (board[x, y].Team != Team)
                    ReturnValues.Add(new Vector2Int(x, y));

                break;
            }

        }

        //Bottom Left
        for (int x = CurrentX - 1, y = CurrentY - 1; x >= 0 && y >= 0; x--, y--)
        {
            if (board[x, y] == null)
                ReturnValues.Add(new Vector2Int(x, y));

            else
            {
                if (board[x, y].Team != Team)
                    ReturnValues.Add(new Vector2Int(x, y));

                break;
            }

        }



        return ReturnValues;
    }



    
}

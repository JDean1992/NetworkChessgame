using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int TileCountX, int TileCountY)
    {
        List<Vector2Int> ReturnValues = new List<Vector2Int>();

        int direction = (Team == 0) ? 1 : -1;
       

        //one in front
        if (board[CurrentX, CurrentY + direction] == null)
            ReturnValues.Add(new Vector2Int(CurrentX, CurrentY + direction));


        //two in front
        if (board[CurrentX, CurrentY + direction] == null)
        {
            //white team
            if (Team == 0 && CurrentY == 1 && board[CurrentX, CurrentY + (direction * 2)] == null)
            {
                ReturnValues.Add(new Vector2Int(CurrentX, CurrentY + (direction * 2)));
            }
            if (Team == 1 && CurrentY == 6 && board[CurrentX, CurrentY + (direction * 2)] == null)
            {
                ReturnValues.Add(new Vector2Int(CurrentX, CurrentY + (direction * 2)));
            }
        }



        //capture move
        if(CurrentX != TileCountX -1)
        {
            if (board[CurrentX + 1, CurrentY + direction] != null && board[CurrentX + 1, CurrentY + direction].Team != Team)
            {
                ReturnValues.Add(new Vector2Int(CurrentX + 1, CurrentY + direction));
            }
        }

        if (CurrentX != 0)
        {
            if (board[CurrentX - 1, CurrentY + direction] != null && board[CurrentX - 1, CurrentY + direction].Team != Team)
            {
                ReturnValues.Add(new Vector2Int(CurrentX - 1, CurrentY + direction));
            }
        }

        return ReturnValues;
    }


    public override SpecialMoves GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> MoveList, ref List<Vector2Int> AvailableMoves)
    {
        int Direction = (Team == 0) ? 1 : -1;


        if ((Team == 0 && CurrentY == 6) || (Team == 1 && CurrentY == 1))
            return SpecialMoves.Promotion;




        //En Passant
        if (MoveList.Count > 0)
        {
            Vector2Int[] LastMove = MoveList[MoveList.Count - 1];

            if (board[LastMove[1].x, LastMove[1].y].PieceType == ChessPieceType.Pawn)
            {
                // checks if the last pawn move was by two or not(in either direction)
                if (Mathf.Abs(LastMove[0].y - LastMove[1].y) == 2)
                {
                    if (board[LastMove[1].x, LastMove[1].y].Team != Team)
                    {
                        //if both pawns are on the same Y
                        if (LastMove[1].y == CurrentY)
                        {
                            if(LastMove[1].x == CurrentX - 1)  //landed left
                            {
                                AvailableMoves.Add(new Vector2Int(CurrentX - 1, CurrentY + Direction));
                                return SpecialMoves.EnPassant;
                            }
                            if (LastMove[1].x == CurrentX + 1)  //landed right
                            {
                                AvailableMoves.Add(new Vector2Int(CurrentX + 1, CurrentY + Direction));
                                return SpecialMoves.EnPassant;
                            }

                        }
                    }
                }
            }
        }








        return SpecialMoves.None;
    }
}

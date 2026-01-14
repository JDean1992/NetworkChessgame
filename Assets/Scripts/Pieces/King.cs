using System.Collections.Generic;
using UnityEngine;

public class King : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int TileCountX, int TileCountY)
    {
        List<Vector2Int> ReturnValues = new List<Vector2Int>();


        //All Rights
        if (CurrentX + 1 < TileCountX)
        {
            //Right
            if (board[CurrentX + 1, CurrentY] == null)
                ReturnValues.Add(new Vector2Int(CurrentX + 1, CurrentY));
            else if (board[CurrentX + 1, CurrentY].Team != Team)
                ReturnValues.Add(new Vector2Int(CurrentX + 1, CurrentY));


            //Top Right
            if (CurrentY + 1 < TileCountY)
                if (board[CurrentX + 1, CurrentY + 1] == null)
                    ReturnValues.Add(new Vector2Int(CurrentX + 1, CurrentY + 1));
                else if (board[CurrentX + 1, CurrentY + 1].Team != Team)
                    ReturnValues.Add(new Vector2Int(CurrentX + 1, CurrentY + 1));

            // Bottom Right
            if (CurrentY - 1 >= 0)
                if (board[CurrentX + 1, CurrentY - 1] == null)
                    ReturnValues.Add(new Vector2Int(CurrentX + 1, CurrentY - 1));
                else if (board[CurrentX + 1, CurrentY - 1].Team != Team)
                    ReturnValues.Add(new Vector2Int(CurrentX + 1, CurrentY - 1));
        }



        //All Lefts
        if (CurrentX - 1 >= 0)
        {
            //Left
            if (board[CurrentX - 1, CurrentY] == null)
                ReturnValues.Add(new Vector2Int(CurrentX - 1, CurrentY));
            else if (board[CurrentX - 1, CurrentY].Team != Team)
                ReturnValues.Add(new Vector2Int(CurrentX - 1, CurrentY));


            //Top Left
            if (CurrentY + 1 < TileCountY)
                if (board[CurrentX - 1, CurrentY + 1] == null)
                    ReturnValues.Add(new Vector2Int(CurrentX - 1, CurrentY + 1));
                else if (board[CurrentX - 1, CurrentY + 1].Team != Team)
                    ReturnValues.Add(new Vector2Int(CurrentX - 1, CurrentY + 1));

            // Bottom Left
            if (CurrentY - 1 >= 0)
                if (board[CurrentX - 1, CurrentY - 1] == null)
                    ReturnValues.Add(new Vector2Int(CurrentX - 1, CurrentY - 1));
                else if (board[CurrentX - 1, CurrentY - 1].Team != Team)
                    ReturnValues.Add(new Vector2Int(CurrentX - 1, CurrentY - 1));

        }


        //Top
        if (CurrentY + 1 < TileCountY)
        {
            if (board[CurrentX, CurrentY + 1] == null || board[CurrentX, CurrentY + 1].Team != Team)
                ReturnValues.Add(new Vector2Int(CurrentX, CurrentY + 1));
        }

        //Down
        if (CurrentY - 1 >= 0)
        {
            if (board[CurrentX, CurrentY - 1] == null || board[CurrentX, CurrentY - 1].Team != Team)
                ReturnValues.Add(new Vector2Int(CurrentX, CurrentY - 1));
        }






        return ReturnValues;
    }


    public override SpecialMoves GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> MoveList, ref List<Vector2Int> AvailableMoves)
    {

        SpecialMoves SM = SpecialMoves.None;

        var KingMove = MoveList.Find(m => m[0].x == 4 && m[0].y == ((Team == 0) ? 0 : 7));
        var LeftRook = MoveList.Find(m => m[0].x == 0 && m[0].y == ((Team == 0) ? 0 : 7));
        var RightRook = MoveList.Find(m => m[0].x == 7 && m[0].y == ((Team == 0) ? 0 : 7));

        if (KingMove == null && CurrentX == 4)
        {
            //White team
            if (Team == 0)
            {
                //Left Rook 
                if (LeftRook == null)
                    if (board[0, 0].PieceType == ChessPieceType.Rook)
                        if (board[0, 0].Team == 0)
                            if (board[3, 0] == null)
                                if (board[2, 0] == null)
                                    if (board[1, 0] == null)
                                    {
                                        AvailableMoves.Add(new Vector2Int(2, 0));
                                        SM = SpecialMoves.Castling;
                                    }
                
                //Right Rook 
                if (RightRook == null)
                     if (board[7, 0].PieceType == ChessPieceType.Rook)
                        if (board[7, 0].Team == 0)
                            if (board[5, 0] == null)
                                if (board[6, 0] == null)
                                {
                                    AvailableMoves.Add(new Vector2Int(6, 0));
                                    SM = SpecialMoves.Castling;
                                }
                
            }
            else
            {
                //Left Rook 
                if (LeftRook == null)
                    if (board[0, 7].PieceType == ChessPieceType.Rook)
                        if (board[0, 7].Team == 1)
                            if (board[3, 7] == null)
                                if (board[2, 7] == null)
                                    if (board[1, 7] == null)
                                    {
                                        AvailableMoves.Add(new Vector2Int(2, 7));
                                        SM = SpecialMoves.Castling;
                                    }
                
                //Right Rook 
                if (RightRook == null)
                     if (board[7, 7].PieceType == ChessPieceType.Rook)
                        if (board[7, 7].Team == 1)
                            if (board[5, 7] == null)
                                if (board[6, 7] == null)
                                {
                                    AvailableMoves.Add(new Vector2Int(6, 7));
                                    SM = SpecialMoves.Castling;
                                }
                
            }
            
        }
        return SM;
    }
}

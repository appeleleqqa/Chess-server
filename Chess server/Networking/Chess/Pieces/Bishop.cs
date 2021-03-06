﻿using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_server
{
    // a class representing a bishop piece
    class Bishop
    {
        /// <summary>
        /// gets all possible moves for this piece
        /// </summary>
        /// <param name="position">the index on the board</param>
        /// <param name="board">the board</param>
        /// <returns>all possible moves</returns>
        public static List<string> PossibleMoves(Vector2 position, char[,] board)
        {
            List<string> possibleMoves = new List<string>();
            int x, y;
            returnVals val;
            //checks the diagonal line that is going up and right
            for (x = (int)position.X + 1, y = (int)position.Y - 1; x < 8  && y >=0; x++, y--) 
            {
                val = IsMoveValid(position, new Vector2(x, y), board);
                if (val == returnVals.Valid)
                    possibleMoves.Add(ChessGame.PositionToString(new Vector2(x, y)));
                else if (val == returnVals.PieceEat)
                {
                    possibleMoves.Add(ChessGame.PositionToString(new Vector2(x, y)));
                    break; //bishop can't go over enemy pieces
                }
                else if (val == returnVals.SomethingInTheWay)
                    break; //bishop can't go over its own pieces

            }
            //checks the diagonal line that is going up and left
            for (x = (int)position.X - 1, y = (int)position.Y - 1; x >= 0 && y >= 0; x--, y--)
            {
                val = IsMoveValid(position, new Vector2(x, y), board);
                if (val == returnVals.Valid)
                    possibleMoves.Add(ChessGame.PositionToString(new Vector2(x, y)));
                else if (val == returnVals.PieceEat)
                {
                    possibleMoves.Add(ChessGame.PositionToString(new Vector2(x, y)));
                    break;
                }
                else if (val == returnVals.SomethingInTheWay)
                    break;

            }
            //checks the diagonal line that is going down and right
            for (x = (int)position.X + 1, y = (int)position.Y + 1; x < 8 && y < 8; x++, y++)
            {
                val = IsMoveValid(position, new Vector2(x, y), board);
                if (val == returnVals.Valid)
                    possibleMoves.Add(ChessGame.PositionToString(new Vector2(x, y)));
                else if (val == returnVals.PieceEat)
                {
                    possibleMoves.Add(ChessGame.PositionToString(new Vector2(x, y)));
                    break;
                }
                else if (val == returnVals.SomethingInTheWay)
                    break;

            }
            //checks the diagonal line that is going down and left
            for (x = (int)position.X - 1, y = (int)position.Y + 1; x >= 0 && y < 8; x--, y++)
            {
                val = IsMoveValid(position, new Vector2(x, y), board);
                if (val == returnVals.Valid)
                    possibleMoves.Add(ChessGame.PositionToString(new Vector2(x, y)));
                else if (val == returnVals.PieceEat)
                {
                    possibleMoves.Add(ChessGame.PositionToString(new Vector2(x, y)));
                    break;
                }
                else if (val == returnVals.SomethingInTheWay)
                    break;
            }
            return possibleMoves;
        }

        /// <summary>
        /// checks if a move is valid
        /// </summary>
        /// <param name="src">the starting index on the board</param>
        /// <param name="dst">the destenation index on the board</param>
        /// <param name="board">the board</param>
        /// <returns>if a move was valid and why</returns>
        public static returnVals IsMoveValid(Vector2 src, Vector2 dst, char[,] board)
        {
            bool isWhite = Char.IsUpper(board[(int)src.X, (int)src.Y]);
            //check if everything is withing the board
            if (src.X >= 0 && src.X <= 7 && src.Y >= 0 && src.Y <= 7 && dst.X >= 0 && dst.X <= 7 && dst.Y >= 0 && dst.Y <= 7)
            {
                //check if this is a diagonal line
                if (Math.Abs(src.X - dst.X) == Math.Abs(src.Y - dst.Y))
                {
                    //checks if there is nothing in its way
                    if (board[(int)dst.X, (int)dst.Y] == '#')
                    {
                            return returnVals.Valid;
                    }
                    //checks if the thing in its way is an enemy piece
                    if (Char.IsUpper(board[(int)dst.X, (int)dst.Y]) ^ isWhite)
                    {
                        return returnVals.PieceEat;
                    }
                    return returnVals.SomethingInTheWay;
                }
                return returnVals.InvalidMovement;
            }
            return returnVals.OutOfBounds;
        }
    }
}

using System;
using System.Numerics;
using System.Collections.Generic;

namespace Chess_server
{
    //a class representing a knight piece
    class Knight
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
            Queue<Vector2> moves = new Queue<Vector2>();
            returnVals val;
            //all possible knight moves
            moves.Enqueue(new Vector2(position.X +1, position.Y + 2));
            moves.Enqueue(new Vector2(position.X +1, position.Y - 2));
            moves.Enqueue(new Vector2(position.X - 1, position.Y + 2));
            moves.Enqueue(new Vector2(position.X - 1, position.Y - 2));
            moves.Enqueue(new Vector2(position.X + 2, position.Y + 1));
            moves.Enqueue(new Vector2(position.X - 2, position.Y + 1));
            moves.Enqueue(new Vector2(position.X + 2, position.Y - 1));
            moves.Enqueue(new Vector2(position.X - 2, position.Y - 1));
            foreach (Vector2 move in moves)
            {
                val = IsMoveValid(position, move, board);
                if (val == returnVals.Valid)
                    possibleMoves.Add(ChessGame.PositionToString(move));
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
                //valid knight movement
                if ((Math.Abs(src.X - dst.X) == 2 && Math.Abs(src.Y - dst.Y) == 1) || (Math.Abs(src.X - dst.X) == 1 && Math.Abs(src.Y - dst.Y) == 2))
                {
                    //checks if there is something blocking it from moving
                    if (board[(int)dst.X, (int)dst.Y] == '#' || (Char.IsUpper(board[(int)dst.X, (int)dst.Y]) ^ Char.IsUpper(board[(int)src.X, (int)src.Y])))
                    {
                        return returnVals.Valid;
                    }
                    return returnVals.SomethingInTheWay;
                }
                return returnVals.InvalidMovement;
            }
            return returnVals.OutOfBounds;
        }
    }
}

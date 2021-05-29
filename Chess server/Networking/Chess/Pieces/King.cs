using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_server
{
    //a class representing a King piece
    class King
    {
        /// <summary>
        /// gets all possible moves for this piece
        /// </summary>
        /// <param name="position">the index on the board</param>
        /// <param name="board">the board</param>
        /// <returns>all possible moves</returns>
        public static List<string> PossibleMoves(Vector2 position, char[,] board)
        {
            //todo:add castling
            List<string> possibleMoves = new List<string>();
            int x, y;
            returnVals val;
            //all upwards moves
            for(x = (int)position.X - 1, y = (int)position.Y + 1; x <= position.X + 1; x++)
            {
                if (x >= 0 && x < 8)
                {
                    val = IsMoveValid(position, new Vector2(x, y), board);
                    if (val == returnVals.Valid)
                        possibleMoves.Add(ChessGame.PositionToString(new Vector2(x, y)));
                }
            }
            //all downwards moves
            for (x = (int)position.X - 1, y = (int)position.Y - 1; x <= position.X + 1; x++)
            {
                if (x >= 0 && x < 8)
                {
                    val = IsMoveValid(position, new Vector2(x, y), board);
                    if (val == returnVals.Valid)
                        possibleMoves.Add(ChessGame.PositionToString(new Vector2(x, y)));
                }
            }
            //left
            if(position.X != 0)//out of bounds
            {
                val = IsMoveValid(position, position - new Vector2(1, 0), board);
                if (val == returnVals.Valid)
                    possibleMoves.Add(ChessGame.PositionToString(position - new Vector2(1, 0)));
            }
            //right
            if (position.Y != 7)//out of bounds
            {
                val = IsMoveValid(position, position + new Vector2(1, 0), board);
                if (val == returnVals.Valid)
                    possibleMoves.Add(ChessGame.PositionToString(position + new Vector2(1, 0)));
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
                //valid king movement
                if ((Math.Abs(src.X - dst.X) <= 1 && Math.Abs(src.Y - dst.Y) <= 1) && !(Math.Abs(src.X - dst.X) == 0 && Math.Abs(src.Y - dst.Y) == 0))
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

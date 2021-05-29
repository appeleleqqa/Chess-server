using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_server
{
    //a class representing a Pawn piece
    class Pawn
    {
        /// <summary>
        /// gets all possible moves for this piece
        /// </summary>
        /// <param name="position">the index on the board</param>
        /// <param name="board">the board</param>
        /// <returns>all possible moves</returns>
        public static List<string> PossibleMoves(Vector2 position, char[,] board)
        {
            //todo: Passant & promotion
            List<string> possibleMoves = new List<string>();
            bool isWhite = Char.IsUpper(board[(int)position.X, (int)position.Y]);
            Queue<Vector2> moves = new Queue<Vector2>();
            returnVals val;
            //all pawn moves
            moves.Enqueue(new Vector2(position.X , position.Y + (isWhite? -1 : 1)));
            moves.Enqueue(new Vector2(position.X , position.Y + (isWhite? -2 : 2)));
            moves.Enqueue(new Vector2(position.X  + 1, position.Y + (isWhite? -1 : 1)));
            moves.Enqueue(new Vector2(position.X  - 1, position.Y + (isWhite? -1 : 1)));
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
            //check if everything is withing the board
            if (src.X >= 0 && src.X <= 7 && src.Y >= 0 && src.Y <= 7 && dst.X >= 0 && dst.X <= 7 && dst.Y >= 0 && dst.Y <= 7)
            //valid knight movement
            {
                bool isWhite = Char.IsUpper(board[(int)src.X, (int)src.Y]);
                bool Valid = false;
                //general movemnt(-1 is moving up which is correct for the white player and 1 is down which is correct for the black player)
                Valid = (int)(dst.Y - src.Y) == (isWhite ? -1 : 1) && src.X == dst.X && board[(int)dst.X, (int)dst.Y] == '#';
                //double move on first movement(checked by starting y(6 for white(which is 2 on board) and 1 for black(7 on board))
                Valid = Valid || ((src.Y == ( isWhite ? 6 : 1)) && (dst.Y - src.Y == (isWhite ? -2 : 2)) && src.X == dst.X && board[(int)src.X, (int)src.Y + (isWhite ? -1 : 1)] == '#' && board[(int)dst.X, (int)dst.Y] == '#');
                //eating a piece
                Valid = Valid || (Char.IsUpper(board[(int)dst.X, (int)dst.Y]) != isWhite && board[(int)dst.X, (int)dst.Y] != '#' && (dst.Y - src.Y) == (isWhite ? -1 : 1) && Math.Abs(src.X - dst.X) == 1);
                if (Valid)
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

using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_server
{
    class Rook
    {
        public static List<string> PossibleMoves(Vector2 position, char[,] board)
        {
            List<string> possibleMoves = new List<string>();
            int x, y;
            returnVals val;
            //checks for right line
            for (x = (int)position.X + 1, y = (int)position.Y; x < 8; x++)
            {
                val = IsMoveValid(position, new Vector2(x, y), board);
                if (val == returnVals.Valid)
                    possibleMoves.Add(ChessGame.PositionToString(new Vector2(x, y)));
                else if (val == returnVals.SomethingInTheWay)
                    break;
            }
            //checks for left line
            for (x = (int)position.X - 1, y = (int)position.Y; x >= 0; x--)
            {
                val = IsMoveValid(position, new Vector2(x, y), board);
                if (val == returnVals.Valid)
                    possibleMoves.Add(ChessGame.PositionToString(new Vector2(x, y)));
                else if (val == returnVals.SomethingInTheWay)
                    break;
            }
            //checks for a line down
            for (x = (int)position.X, y = (int)position.Y + 1; y < 8; y++)
            {
                val = IsMoveValid(position, new Vector2(x, y), board);
                if (val == returnVals.Valid)
                    possibleMoves.Add(ChessGame.PositionToString(new Vector2(x, y)));
                else if (val == returnVals.SomethingInTheWay)
                    break;
            }
            //checks for a line up
            for (x = (int)position.X, y = (int)position.Y - 1; y >= 0; y--)
            {
                val = IsMoveValid(position, new Vector2(x, y), board);
                if (val == returnVals.Valid)
                    possibleMoves.Add(ChessGame.PositionToString(new Vector2(x, y)));
                else if (val == returnVals.SomethingInTheWay)
                    break;
            }
            return possibleMoves;
        }

        public static returnVals IsMoveValid(Vector2 src, Vector2 dst, char[,] board)
        {
            //check if everything is withing the board
            if (src.X >= 0 && src.X <= 7 && src.Y >= 0 && src.Y <= 7 && dst.X >= 0 && dst.X <= 7 && dst.Y >= 0 && dst.Y <= 7)
            {
                //valid rook movemnt
                if ((src.X == dst.X ^ src.Y == dst.Y))
                {
                    //checks if there is something blocking it from moving
                    if (board[(int)dst.X, (int)dst.Y] == '#' || (Char.IsUpper(board[(int)dst.X, (int)dst.Y]) ^ Char.IsUpper(board[(int)src.X, (int)src.Y])))
                        return returnVals.Valid;// add chess check(king in danger)
                    return returnVals.SomethingInTheWay;
                }
                return returnVals.InvalidMovement;
            }
            return returnVals.OutOfBounds;
        }
    }
}

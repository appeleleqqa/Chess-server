using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;

namespace Chess_server
{
    //all the possible values IsMoveValid can return
    enum returnVals
    {
        OutOfBounds,
        Valid,
        SomethingInTheWay,
        Check,
        InvalidMovement,
        PieceEat
    }

    // a class representing a cvhess game
    class ChessGame
    {
        char[,] board = new char[8,8];
        string hostName = string.Empty;
        bool turn = false;
        bool msgAvg = false;
        bool msgRecieved = false;
        string move = string.Empty;
        
        
        /// <summary>
        /// opens a new game
        /// </summary>
        /// <param name="host">the name of the host</param>
        public ChessGame(string host)
        {
            InitializeBoard();
            hostName = host;
            turn = true;
            msgAvg = false;//no message avillable
            msgRecieved = false;//no one recieved yet
        }


        /// <summary>
        /// puts the pieces into the board array
        /// </summary>
        void InitializeBoard()
        {
            string tmpBoard = "rnbkqbnr" +
                              "pppppppp" +
                              "########" +
                              "########" +
                              "########" +
                              "########" +
                              "PPPPPPPP" +
                              "RNBKQBNR";
            int place = 0;
            foreach (char piece in tmpBoard)
            {
                board[place % 8, place / 8] = piece;
                place++;
            }
        }

        /// <summary>
        /// returns the axis of a piece with a given position
        /// </summary>
        /// <param name="pos">the index on the board</param>
        /// <returns>the axis</returns>
        public static string PositionToString(Vector2 pos)
        {
            return ((char)(pos.X  + (int)'a')).ToString() + (8-pos.Y).ToString();
        }

        /// <summary>
        /// returns the position of a piece with a given axis
        /// </summary>
        /// <param name="str">the axis</param>
        /// <returns></returns>
        public static Vector2 StringToPosition(string str)
        {
            int x = (int)str[0] - (int)'a';
            int y = 8 - ((int)str[1] - (int)'0');// 7 on board is 1 in the axis and 8 in the axis is 0 on the board
            return new Vector2(x, y);
        }

        /// <summary>
        /// returns all possible moves for a player
        /// </summary>
        /// <param name="isWhite">the players color</param>
        /// <param name="board">the board</param>
        /// <returns>all possible moves</returns>
        static Dictionary<string, List<string>> AllPossibleMoves(bool isWhite, char[,] board)
        {
            Dictionary<string, List<string>> possibleMoves = new Dictionary<string, List<string>>();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    //checks if the piece is the same color as the player
                    if (isWhite == Char.IsUpper(board[j, i]))
                    {
                        switch (Char.ToUpper(board[j, i]))
                        {
                            case 'P':
                                AddToDict(possibleMoves, PositionToString(new Vector2(j, i)), Pawn.PossibleMoves(new Vector2(j, i), board));
                                break;
                            case 'R':
                                AddToDict(possibleMoves, PositionToString(new Vector2(j, i)), Rook.PossibleMoves(new Vector2(j, i), board));
                                break;
                            case 'N':
                                AddToDict(possibleMoves, PositionToString(new Vector2(j, i)), Knight.PossibleMoves(new Vector2(j, i), board));
                                break;
                            case 'B':
                                AddToDict(possibleMoves, PositionToString(new Vector2(j, i)), Bishop.PossibleMoves(new Vector2(j, i), board));
                                break;
                            case 'Q':
                                AddToDict(possibleMoves, PositionToString(new Vector2(j, i)), Queen.PossibleMoves(new Vector2(j, i), board));
                                break;
                            case 'K':
                                AddToDict(possibleMoves, PositionToString(new Vector2(j, i)), King.PossibleMoves(new Vector2(j, i), board));
                                break;
                        }
                    }
                }
            }
            return possibleMoves;
        }

        /// <summary>
        /// adds the moves to the dict only if there are 1 or more possiblities
        /// </summary>
        /// <param name="dict">the dict</param>
        /// <param name="key">the key add</param>
        /// <param name="val">the value of the key</param>
        static void AddToDict(Dictionary<string, List<string>> dict, string key, List<string> val)
        {
            if(val.Count > 0)
            {
                dict.Add(key, val);
            }
        }

        /// <summary>
        /// moves a piece on the board
        /// </summary>
        /// <param name="src">the source axis</param>
        /// <param name="dst">the destenation axis</param>
        /// <param name="board">the board</param>
        public static void MovePiece(string src, string dst, char[,] board)
        {
            Vector2 srcV = StringToPosition(src);
            Vector2 dstV = StringToPosition(dst);
            //todo: add castling
            board[(int)dstV.X, (int)dstV.Y] = board[(int)srcV.X, (int)srcV.Y];
            board[(int)srcV.X, (int)srcV.Y] = '#';
        }

        /// <summary>
        /// checks if a square is in danger
        /// (one of the destenations of the possible moves is the same as the square's)
        /// </summary>
        /// <param name="axis">the square's axis</param>
        /// <param name="possibleMoves">all the possible moves</param>
        /// <returns>if the given axis is under a threat</returns>
        public static bool IsInDanger(string axis, Dictionary<string, List<string>> possibleMoves)
        {
            foreach(List<string> moves in possibleMoves.Values)
            {
                foreach(string move in moves)
                {
                    //if a piece can move to the axis
                    if (move == axis)
                        return true;
                }
            }
            return false;
        }


        /// <summary>
        /// checks if a move will cause the player to get into a check
        /// </summary>
        /// <param name="isWhite">the players color</param>
        /// <param name="src">move source axis</param>
        /// <param name="dst">move destenation axis</param>
        /// <param name="board">the board</param>
        /// <returns>whether or not the move will cause a check</returns>
        public static bool CheckForCheck(bool isWhite, string src, string dst, char[,] board)
        {
            var boardCopy = board.Clone() as char[,];
            
            //move piece on board copy
            MovePiece(src, dst, boardCopy);
            string KingAxis = string.Empty;
            //find king
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    //checks if the piece is the same color as the player
                    if (boardCopy[j, i] == (isWhite ? 'K' : 'k'))
                    {
                        KingAxis = PositionToString(new Vector2(j, i));
                        break;
                    }
                }
            }
            // check if the king is in danger after the move has been done
            Dictionary<string, List<string>> moves = AllPossibleMoves(!isWhite, boardCopy);
            return IsInDanger(KingAxis, moves);
        }

        /// <summary>
        /// starts the game for a certain player
        /// </summary>
        /// <param name="username">player's name</param>
        /// <param name="stream">the network's stream</param>
        public void Play(string username, NetworkStream stream)
        {
            //decides who will be white by who is the host
            bool isWhite = username == hostName;
            while(true)
            {
                //if its the players turn
                if (turn == isWhite)
                {
                    //get all possible moves
                    Dictionary<string, List<string>> moves = AllPossibleMoves(isWhite, board);
                    List<string> InvalidMoves = new List<string>();//used to prevent a runtime error caused by changing a list while iterating through it
                    //check if one of the moves results in a Check
                    foreach(KeyValuePair<string, List<string>> pair in moves)
                    {
                        foreach(string dst in pair.Value)
                        {
                            if (CheckForCheck(isWhite, pair.Key, dst, board))
                                InvalidMoves.Add(dst);
                        }
                        //delete all invalid moves
                        foreach (string dst in InvalidMoves)
                        {
                            moves[pair.Key].Remove(dst);
                        }
                        InvalidMoves = new List<string>();
                    }
                    //again we use invalidMoves to prevent a runtime error
                    foreach (KeyValuePair<string, List<string>> pair in moves)
                    {
                        if (moves[pair.Key].Count == 0)
                            InvalidMoves.Add(pair.Key);
                    }
                    //delete all pieces that have no moves from the dict
                    foreach (string src in InvalidMoves)
                    {
                        moves.Remove(src);
                    }
                    // if the player has no possible moves he had lost
                    if (moves.Count == 0)
                    {
                        string msg = ((int)(isWhite? msgCodes.BlackWon : msgCodes.WhiteWon)).ToString();
                        stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                        move = "lose";
                        msgAvg = true;
                        return;
                    }
                    //if the player has possible moves
                    else
                    {
                        //sedns the moves to the player
                        MovesListMsg movesListMsg = new MovesListMsg();
                        movesListMsg.Code = (int)msgCodes.AllMoves;
                        movesListMsg.Moves = moves;
                        string msg = JsonConvert.SerializeObject(movesListMsg);
                        stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                        //receive move from player and check if its correct
                        MoveMsg moveMsg = JsonConvert.DeserializeObject<MoveMsg>(Server.ReceiveMsg(stream));
                        msg = moveMsg.Move;
                        while (true)
                        {
                            if (moves.ContainsKey(msg.Substring(0, 2)))
                            {
                                if (moves[msg.Substring(0, 2)].Contains(msg.Substring(2, 2)))
                                {
                                    //if its a correct move continues to move the piece
                                    break;
                                }
                            }
                            stream.Write(Encoding.ASCII.GetBytes(((int)msgCodes.InvalidMove).ToString()), 0, ((int)msgCodes.InvalidMove).ToString().Length);
                            moveMsg = JsonConvert.DeserializeObject<MoveMsg>(Server.ReceiveMsg(stream));
                            msg = moveMsg.Move;
                        }
                        MovePiece(msg.Substring(0, 2), msg.Substring(2, 2), board);
                        //sends the move that the player made to the other player
                        move = msg;
                        turn = !turn;//change the turn
                        msgAvg = true;//tell the other player that this player has made its move
                        while (!msgRecieved) ;//wait for the other player to confirm receing
                        msgRecieved = false;
                    }
                }
                else
                {
                    while (!msgAvg) ;//wait for the other player to make its move
                    msgAvg = false;
                    msgRecieved = true;//tell the other player you have recieved the msg
                    //if the other player lost
                    if (move == "lose")
                    {
                        string msg = ((int)(isWhite ? msgCodes.WhiteWon : msgCodes.BlackWon)).ToString();
                        stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                        
                        return;
                    }
                    //if he made a move
                    else
                    {
                        MoveMsg moveMsg = new MoveMsg();
                        moveMsg.Code = (int)msgCodes.PieceMove;
                        moveMsg.Move = move;
                        string msg = JsonConvert.SerializeObject(moveMsg);
                        stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                    }
                }
            }
        }
    }
}

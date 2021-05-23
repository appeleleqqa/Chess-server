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
    enum returnVals
    {
        OutOfBounds,
        Valid,
        SomethingInTheWay,
        Chess,
        InvalidMovement
    }
    class ChessGame
    {
        char[,] board = new char[8,8];
        string hostName = string.Empty;
        bool turn = false;
        bool msgAvg = false;
        bool msgRecieved = false;
        string move = string.Empty;

        public ChessGame(string host)
        {
            InitializeBoard();
            hostName = host;
            turn = true;
            msgAvg = false;
            msgRecieved = false;
        }

        void InitializeBoard()
        {
            //initialize the board
            string tmpBoard = "rnbqkbnr" +
                              "pppppppp" +
                              "########" +
                              "########" +
                              "########" +
                              "########" +
                              "PPPPPPPP" +
                              "RNBQKBNR";
            int place = 0;
            foreach (char piece in tmpBoard)
            {
                board[place % 8, place / 8] = piece;
                place++;
            }
        }
        public static string PositionToString(Vector2 pos)
        {
            return ((char)(pos.X  + (int)'a')).ToString() + (8-pos.Y).ToString();
        }
        public static Vector2 StringToPosition(string str)
        {
            int x = (int)str[0] - (int)'a';
            int y = 8 - ((int)str[1] - (int)'0');
            return new Vector2(x, y);
        }

        Dictionary<string, List<string>> AllPossibleMoves(bool isWhite)
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

        static void AddToDict(Dictionary<string, List<string>> dict, string key, List<string> val)
        {
            if(val.Count > 0)
            {
                dict.Add(key, val);
            }
        }

        public void MovePiece(string src, string dst)
        {
            Vector2 srcV = StringToPosition(src);
            Vector2 dstV = StringToPosition(dst);
            //todo: add castling
            board[(int)dstV.X, (int)dstV.Y] = board[(int)srcV.X, (int)srcV.Y];
            board[(int)srcV.X, (int)srcV.Y] = '#';
        }

        public void Play(string username, NetworkStream stream)
        {
            bool isWhite = username == hostName;
            while(true)
            {
                if (turn == isWhite)
                {
                    Dictionary<string, List<string>> moves = AllPossibleMoves(isWhite);
                    // loss / draw
                    if (moves.Count == 0)
                    {
                        string msg = ((int)(isWhite? msgCodes.BlackWon : msgCodes.WhiteWon)).ToString();
                        stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                        move = "lose";
                        msgAvg = true;
                        return;
                    }
                    else
                    {
                        MovesListMsg movesListMsg = new MovesListMsg();
                        movesListMsg.Code = (int)msgCodes.AllMoves;
                        movesListMsg.Moves = moves;
                        string msg = JsonConvert.SerializeObject(movesListMsg);
                        stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                        MoveMsg moveMsg = JsonConvert.DeserializeObject<MoveMsg>(Server.ReceiveMsg(stream));
                        msg = moveMsg.Move;
                        while (true)
                        {
                            if (moves.ContainsKey(msg.Substring(0, 2)))
                            {
                                if (moves[msg.Substring(0, 2)].Contains(msg.Substring(2, 2)))
                                {
                                    break;
                                }
                            }
                            stream.Write(Encoding.ASCII.GetBytes(((int)msgCodes.InvalidMove).ToString()), 0, ((int)msgCodes.InvalidMove).ToString().Length);
                            moveMsg = JsonConvert.DeserializeObject<MoveMsg>(Server.ReceiveMsg(stream));
                            msg = moveMsg.Move;
                        }
                        MovePiece(msg.Substring(0, 2), msg.Substring(2, 2));
                        move = msg;
                        turn = !turn;
                        msgAvg = true;
                        while (!msgRecieved) ;
                        msgRecieved = false;
                    }
                }
                else
                {
                    while (!msgAvg) ;
                    msgAvg = false;
                    msgRecieved = true;
                    if (move == "lose")
                    {
                        string msg = ((int)(isWhite ? msgCodes.WhiteWon : msgCodes.BlackWon)).ToString();
                        stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                        return;
                    }
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

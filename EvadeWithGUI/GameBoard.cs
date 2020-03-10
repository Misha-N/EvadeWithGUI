using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvadeWithGUI
{
    public class GameBoard
    {
        #region Board properties
        public int BoardSize { get; set; }
        public int[,] Board { get; set; }
        public GameRules GameRules { get; set; }
        #endregion

        public GameBoard(GameRules rules)
        {
            GameRules = rules;
            BoardSize = GameConstants.boardSize;
            Board = new int[BoardSize, BoardSize];
            InitBoard();
        }
        private void InitBoard()
        {
            GameRules.InitBoardByRules(Board);
        }

        public int Position(int row, int col)
        {
            return Board[row, col];
        }

        public int GetLength(int num)
        {
            return Board.GetLength(num);
        }

        public void MakeMove(List<int> move, GameBoard board)   // ohodnotí tah a přidá jeho výsledek
        {
            int row = move[(int)GameConstants.MoveParts.row];
            int col = move[(int)GameConstants.MoveParts.col];
            int newRow = move[(int)GameConstants.MoveParts.newRow];
            int newCol = move[(int)GameConstants.MoveParts.newCol];

            if (GameRules.ValidMove(row, col, newRow, newCol, board))
            {
                GameRules.EvaluateMove(row, col, newRow, newCol, board, out int result);
                if (move.Count != 7)
                    move.Add(result);
                else
                    move[(int)GameConstants.MoveParts.result] = result;
                //Console.WriteLine("Valid move!");
            }
            else
            {
                if (move.Count != 7)
                    move.Add((int)GameConstants.MoveResult.Fail);
                else
                    move[(int)GameConstants.MoveParts.result] = (int)GameConstants.MoveResult.Fail;
            }

        }

        #region Metody pracující přímo s deskou

        public bool IsKing(int row, int col)
        {
            if (IsWhiteKing(row, col) || IsBlackKing(row, col))
                return true;
            else
                return false;
        }
        public bool IsBlackKing(int row, int col)
        {
            if (Position(row, col) == (int)GameConstants.States.bKing)
                return true;
            else
                return false;
        }
        public bool IsWhiteKing(int row, int col)
        {
            if (Position(row, col) == (int)GameConstants.States.wKing)
                return true;
            else
                return false;
        }
        public bool Occupied(int row, int col)
        {
            if (Position(row, col) != (int)GameConstants.States.empty)
                return true;
            else
            {
                return false;
            }
        }
        public bool Frozen(int row, int col)
        {
            if (Position(row, col) == (int)GameConstants.States.frozen)
                return true;
            else
            {
                return false;
            }
        }
        public bool Enemy(int plRow, int plCol, int opRow, int opCol)
        {
            if (IsWhite(plRow, plCol) && IsBlack(opRow, opCol))
                return true;
            else if (IsBlack(plRow, plCol) && IsWhite(opRow, opCol))
                return true;
            else
                return false;
        }
        public bool IsBlack(int row, int col)
        {
            if (Position(row, col) == (int)GameConstants.States.bMan || Position(row, col) == (int)GameConstants.States.bKing)
                return true;
            else
                return false;
        }
        public bool IsEmpty(int row, int col)
        {
            if (Position(row, col) == (int)GameConstants.States.empty)
                return true;
            else
                return false;
        }
        public bool IsWhite(int row, int col)
        {
            if (Position(row, col) == (int)GameConstants.States.wMan || Position(row, col) == (int)GameConstants.States.wKing)
                return true;
            else
                return false;
        }

        // metody pro změnu stavu pole
        private void Freeze(int row, int col)
        {
            Board[row, col] = (int)GameConstants.States.frozen;
        }
        public void Empty(int row, int col)
        {
            Board[row, col] = (int)GameConstants.States.empty;
        }
        public void Move(int row, int col, int newRow, int newCol)
        {
            Board[newRow, newCol] = Board[row, col];
            Empty(row, col);
        }
        public void Freeze(int row, int col, int newRow, int newCol)
        {
            //Console.WriteLine("Freezed");
            Freeze(newRow, newCol);
            Empty(row, col);
        }

        #endregion

        // pomocné metody poskytující informace o aktuálním stavu desky
        public int CountOfPieces(int playerColor)
        {
            int counter = 0;

            for (int row = 0; row < GetLength(1); row++)
            {
                for (int col = 0; col < GetLength(0); col++)
                {
                    if (playerColor == (int)GameConstants.PlayerColor.White)
                    {
                        if (IsWhite(row, col))
                            counter++;
                    }
                    if (playerColor == (int)GameConstants.PlayerColor.Black)
                    {
                        if (IsBlack(row, col))
                            counter++;
                    }
                }

            }

            return counter;
        }


        public Tuple<int,int> SelectRandomPiece(int playerColor)
        {
            Random rnd = new Random();
            int randomNum = rnd.Next(1, CountOfPieces(playerColor));
            int counter = 0;

            for (int row = 0; row < GetLength(1); row++)
            {
                for (int col = 0; col < GetLength(0); col++)
                {
                    if (playerColor == (int)GameConstants.PlayerColor.White)
                    {
                        if (IsWhite(row, col))
                            counter++;
                        if (counter == randomNum)
                            return Tuple.Create(row, col);
                    }
                    if (playerColor == (int)GameConstants.PlayerColor.Black)
                    {
                        if (IsBlack(row, col))
                            counter++;
                        if (counter == randomNum)
                            return Tuple.Create(row, col);
                    }
                }
            }
            return Tuple.Create(0, 0);
        }


        public List<Tuple<int, int>> AllPlayerPieces(int playerColor)
        {
            List<Tuple<int, int>> allPieces = new List<Tuple<int, int>>(CountOfPieces(playerColor));

            for (int row = 0; row < GetLength(1); row++)
            {
                for (int col = 0; col < GetLength(0); col++)
                {
                    if (playerColor == (int)GameConstants.PlayerColor.White)
                    {
                        if (IsWhite(row, col))
                            allPieces.Add(Tuple.Create(row, col));
                    }
                    if (playerColor == (int)GameConstants.PlayerColor.Black)
                    {
                        if (IsBlack(row, col))
                            allPieces.Add(Tuple.Create(row, col));

                    }
                }
            }

            return allPieces;
        }
    }
}

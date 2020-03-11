using EvadeWithGUI.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static EvadeWithGUI.GameConstants;

namespace EvadeWithGUI
{
    public class GameManager
    {
        // Třída GameManager řídí průběh hry.
        // Ve spolupráci s GameRules kontroluje konec hry.
        // Validuje podobu tahu zadaného uživatelem

        #region GameManager properties
        public GameRules GameRules { get; set; }
        public GameBoard GameBoard { get; set; }
        public string Input { get; set; }
        public Player PlayerOne { get; set; }
        public Player PlayerTwo { get; set; }
        public Player PlayerOnTurn { get; set; }
        public bool GameInProgress { get; set; }
        public bool Thinking { get; set; }
        public string GameStatus { get; set; }

        public List<int> BestMove { get; set; }
        public Stack<List<int>> GameHistory { get; set; }

        public Stack<List<int>> RedoStack { get; set; }



        private string _selectedPosition;
        public string SelectedPosition
        {
            get { return _selectedPosition; }
            set { _selectedPosition = value; }
        }

        #endregion

        //public List<Tuple<int, int>> posibleMoves = new List<Tuple<int, int>>();

        public GameManager()
        {
            GameRules = new GameRules();
            GameBoard = new GameBoard(GameRules);
            GameHistory = new Stack<List<int>>(1000);
            RedoStack = new Stack<List<int>>(1000);
            GameInProgress = false;
            Input = "";
        }

        public void StartGame()
        {
            SetGame();
        }

        public async Task GetInput(int row, int col, CancellationTokenSource cancellationTokenSource)
        {
            SelectedPosition = row.ToString() + col.ToString();
            await GameTurn(cancellationTokenSource);
        }

        public async Task GameTurn(CancellationTokenSource cancellationTokenSource)
        {
                if (PlayerOnTurn.IsAI)
                    await (PlayerOnTurn.TryToPlay(GameBoard, cancellationTokenSource));
                if (!cancellationTokenSource.IsCancellationRequested)
                {

                    if (!PlayerOnTurn.Finished)
                    {
                            await PlayerOnTurn.Selection(ToIntToRow(SelectedPosition), ToIntToCol(SelectedPosition), GameBoard, cancellationTokenSource);
                            if (PlayerOnTurn.PlayerMove[(int)GameConstants.MoveParts.result] == (int)GameConstants.MoveResult.Fail && !PlayerOnTurn.StartPositionSelected)
                                SelectedPosition = "none - Failed Move, Play again.";
                    }
                    if (PlayerOnTurn.Finished)
                    {
                        GameHistory.Push(new List<int>(PlayerOnTurn.PlayerMove));
                        PlayerOnTurn.PlayerMove.ForEach(Console.Write);
                        PlayerOnTurn.Finished = false;
                        CheckEndGame();
                        RedoStack.Clear();
                        BestMove = null;
                        EndTurn();
                    }
                }
        }

        // Počáteční nastavení hry 
        public void SetGame()
        {
            // new Player(color, AI?, IQ (1 - 4))
            PlayerOne = new Player((int)GameConstants.PlayerColor.Black, false, 4);
            PlayerTwo = new Player((int)GameConstants.PlayerColor.White, true, 2);
            PlayerOnTurn = PlayerOne;
            GameInProgress = true;
            GameStatus = "In progress.";
        }
        /*

        public void Settings(string input)
        {
            if (input == "!AI")
                if (PlayerOnTurn.IsAI)
                {
                    PlayerOnTurn.IsAI = false;
                    ReloadUI();
                    Console.WriteLine("AI changed to human player.");
                }
                else
                {
                    PlayerOnTurn.IsAI = true;
                    ReloadUI();
                    Console.WriteLine("Human player changed to AI.");
                }
            if (input == "!IQ")
            {
                Console.WriteLine("Type new IQ value (1-4) and hit ENTER.");
                string newIQ = Console.ReadLine();
                if (newIQ.Length == 1 && Char.GetNumericValue(newIQ[0]) > 0 && Char.GetNumericValue(newIQ[0]) < 5)
                {
                    PlayerOnTurn.IQ = (int)Char.GetNumericValue(newIQ[0]);
                    ReloadUI();
                    Console.WriteLine("Changed IQ of player on turn to " + (int)Char.GetNumericValue(newIQ[0]) + ".");
                }
                else
                    Console.WriteLine("Bad input!");
            }
            if (input == "!HELP")
            {
                List<int> helpMove;
                helpMove = PlayerOnTurn.AI.SmartMove(GameBoard, PlayerOnTurn.PlayerColor, PlayerOnTurn.IQ);
                Console.WriteLine("Nejlepší možný tah:");
                helpMove.ForEach(Console.Write);
                Console.WriteLine();
            }
        }
        */

        #region metody pro validaci a formátování vstupu od uživatele
        // převod vstupu na int hodnotu

        private int ToIntToRow(string input)
        {
            return (int)Char.GetNumericValue(input[0]);
        }
        private int ToIntToCol(string input)
        {
            return (int)Char.GetNumericValue(input[1]);
        }
        #endregion

        public Player PlayerSwap()
        {
            if (PlayerOnTurn == PlayerOne)
                return PlayerTwo;
            else
                return PlayerOne;
        }

        private void EndTurn()
        {
            Console.WriteLine("End of Turn!");
            PlayerOnTurn = PlayerSwap();
            SelectedPosition = "none";
        }

        private void EndGame()
        {
            GameInProgress = false;
        }

        private void CheckEndGame()
        {
            if (GameRules.EGFrozenKings(GameBoard))
            {
                GameStatus = "draw";
                Console.WriteLine("Frozen Kings.");
                EndGame();
            }
            if (GameRules.EGRoyalLine(GameBoard))
            {
                GameStatus = Enum.ToObject(typeof(PlayerColor), PlayerOnTurn.PlayerColor).ToString() + " Player Win!";
                Console.WriteLine("King on Royal Line.");
                EndGame();
            }
            if (GameRules.EGEnemyBlocked(GameBoard, PlayerOnTurn))
            {
                GameStatus = Enum.ToObject(typeof(PlayerColor), PlayerOnTurn.PlayerColor).ToString() + " Player Win!";
                Console.WriteLine("Enemy Blocked.");
                EndGame();
            }
        }

        public void UndoMove()
        {
            if(GameHistory.Count != 0)
            {
                List<int> move = GameHistory.Peek();
                GameBoard.Board[move[0], move[1]] = move[2];
                GameBoard.Board[move[3], move[4]] = move[5];
                RedoStack.Push(GameHistory.Pop());
                PlayerOnTurn = PlayerSwap();
                if (!GameInProgress)
                    GameInProgress = true;
            }
        }

        public void RedoMove()
        {
            if (RedoStack.Count != 0)
            {
                List<int> move = RedoStack.Peek();
                GameBoard.Board[move[0], move[1]] = 0;
                if(move[6] == 1)
                    GameBoard.Board[move[3], move[4]] = 8;
                else
                    GameBoard.Board[move[3], move[4]] = move[2];
                GameHistory.Push(RedoStack.Pop());
                CheckEndGame();
                PlayerOnTurn = PlayerSwap();
            }
        }

        public bool PlayMoveHistory(List<List<int>> moves, int saveHash)
        {
            
            Console.WriteLine(moves.Count);

            if (GetHash(moves) == saveHash)
            {
                foreach (var sublist in moves)
                {
                    RedoStack.Push(sublist);
                }
                while (RedoStack.Count > 0)
                {
                    RedoMove();
                }
                
                return true;
            }
            else
                return false;
        }


        public int GetHash(IEnumerable moves)
        {
            string str = "";
            foreach (List<int> move in moves)
            {
                var result = string.Join("",move);
                str += result;
            }
            
            Console.WriteLine(str);
            Console.WriteLine(str.GetHashCode());

            return str.GetHashCode();

        }


        /*
        public string Help(int row, int col)
        {
            //return gameBoard.PosibleMoves(row, col).ToString();
        }
        */

    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        public string GameStatus { get; set; }

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
            GameInProgress = false;
            Input = "";
        }

        public void StartGame()
        {
            SetGame();
        }

        public void GetInput(int row, int col)
        {
            SelectedPosition = row.ToString() + col.ToString();
            GameTurn();
        }

        public void GameTurn()
        {

                if (PlayerOnTurn.IsAI)
                    PlayerOnTurn.TryToPlay(GameBoard);

                if (!PlayerOnTurn.Finished)
                {
                        PlayerOnTurn.Selection(ToIntToRow(SelectedPosition), ToIntToCol(SelectedPosition), GameBoard);
                        if (PlayerOnTurn.PlayerMove[(int)GameConstants.MoveParts.result] == (int)GameConstants.MoveResult.Fail && !PlayerOnTurn.StartPositionSelected)
                            SelectedPosition = "none - Failed Move, Play again.";
                }
                if (PlayerOnTurn.Finished)
                {
                    PlayerOnTurn.Finished = false;
                    CheckEndGame();
                    EndTurn();
                }
        }

        // Počáteční nastavení hry 
        public void SetGame()
        {
            // new Player(color, AI?, IQ (1 - 4))
            PlayerOne = new Player((int)GameConstants.PlayerColor.Black, false, 3);
            PlayerTwo = new Player((int)GameConstants.PlayerColor.White, true, 3);
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

        private Player PlayerSwap()
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
                GameStatus = "Draw";
                Console.WriteLine("Frozen Kings.");
                EndGame();
            }
            if (GameRules.EGRoyalLine(GameBoard))
            {
                GameStatus = PlayerOnTurn.PlayerColor + " Win";
                Console.WriteLine("King on Royal Line.");
                EndGame();
            }
            if (GameRules.EGEnemyBlocked(GameBoard, PlayerOnTurn))
            {
                GameStatus = PlayerOnTurn.PlayerColor + " Win";
                Console.WriteLine("Enemy Blocked.");
                EndGame();
            }
        }

        /*
        public string Help(int row, int col)
        {
            //return gameBoard.PosibleMoves(row, col).ToString();
        }
        */

    }
}

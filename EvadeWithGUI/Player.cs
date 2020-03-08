using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvadeWithGUI
{
    public class Player
    {
        // Třída Player spravuje tah hráče. 
        // U lidského hráče kontroluje zda hráč táhne svými figurkami a zda je tah validní
        // U AI hráče provádí tah ve spolupráci s třídou BrainAI

        #region Player properties
        public int IQ { get; set; }
        public int PlayerColor { get; set; }
        public bool IsAI { get; set; }
        public BrainAI AI { get; set; }
        public bool Finished { get; set; }
        public bool StartPositionSelected { get; set; }
        public List<int> PlayerMove { get; set; }

        #endregion

        public Player(int color, bool isAI, int IQ)
        {
            PlayerColor = color;
            IsAI = isAI;
            AI = new BrainAI();
            this.IQ = IQ;
            StartPositionSelected = false;
            PlayerMove = new List<int>() { 0, 0, 0, 0, 0, 0, 0 };
        }

        // selekce pozic pro provedení tahu
        public void Selection(int row, int col, GameBoard board)
        {
            if (StartPositionSelected)  // je vybrána počáteční pozice
            {
                PlayerMove[(int)GameConstants.MoveParts.newRow] = row;
                PlayerMove[(int)GameConstants.MoveParts.newCol] = col;
                PlayerMove[(int)GameConstants.MoveParts.newPosition] = board.Position(row, col);

                StartPositionSelected = false;

                TryToPlay(board);   // zkusí zahrát zvolený tah

            }
            else if (!StartPositionSelected)    // není vybrána počáteční pozice
            {
                if (Owner(row, col, board))     // správná barva figurek
                {
                    PlayerMove[(int)GameConstants.MoveParts.row] = row;
                    PlayerMove[(int)GameConstants.MoveParts.col] = col;
                    PlayerMove[(int)GameConstants.MoveParts.piece] = board.Position(row, col);
                    StartPositionSelected = true;
                }
            }
        }
        public void TryToPlay(GameBoard board)
        {
            if (IsAI)
                AIPlay(board);
            else
            {
                // zahraje tah a pokud výsledkem tahu není FAIL ukončí tah hráče
                board.MakeMove(PlayerMove, board);
                if (PlayerMove[(int)GameConstants.MoveParts.result] != (int)GameConstants.MoveResult.Fail)
                {
                    Finished = true;
                }
            }


        }
        public bool Owner(int row, int col, GameBoard board)    // kontroluje vlastnictví figurek
        {
            if (PlayerColor == (int)GameConstants.PlayerColor.Black && board.IsBlack(row, col))
                return true;
            if (PlayerColor == (int)GameConstants.PlayerColor.White && board.IsWhite(row, col))
                return true;
            else
                return false;
        }
        public void AIPlay(GameBoard board) // provede tah AI
        {
            PlayerMove = AI.SmartMove(board, PlayerColor, IQ);

            //System.Threading.Thread.Sleep(1000);


            Console.Write("Nejlepší nalezený tah: ");
            PlayerMove.ForEach(Console.Write);
            Console.WriteLine();


            Console.WriteLine("Stiskněte Enter pro zahrání AI tahu.");


            // provede vybraný nejlepší tah
            PlayerMove.Count();
            board.MakeMove(PlayerMove, board);
            Finished = true;

        }

    }
}

using Caliburn.Micro;
using EvadeWithGUI.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using static EvadeWithGUI.GameConstants;

namespace EvadeWithGUI.ViewModels
{
    public class ShellViewModel: Conductor<object>
    {
        GameManager gm = new GameManager();

        private readonly BackgroundWorker worker = new BackgroundWorker();
        

        public ShellViewModel()
        {
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.WorkerSupportsCancellation = true;

            gm.StartGame();
            BoardItems = new ObservableCollection<BoardItem>();
            AddUnitsFromGameBoard(BoardItems);


        }

        private ObservableCollection<BoardItem> _boardItems;
        public ObservableCollection<BoardItem> BoardItems
        {
            get 
            {
                return _boardItems; 
            }
            set
            {
                _boardItems = value;
                NotifyOfPropertyChange(() => BoardItems);
            }
        }

        public void AddUnitsFromGameBoard(ObservableCollection<BoardItem> boardItems)
        {
            if (gm != null)
            {
                boardItems.Clear();
    
                var gameBoard = gm.GameBoard.Board;
                for (int i = 1; i <= 6; i++)
                {
                    for (int j = 1; j <= 6; j++)
                    {
                        int field = gameBoard[i, j];

                        if (field != (int)GameConstants.States.barrier)
                        {
                            boardItems.Add(new BoardItem() { Row = i, Col = j, PieceType = (GameConstants.States)gameBoard[i, j] });
                        }

                    }
                }
            }

        }

        public BoardItem SelectedBoardItem
        {
            set 
            {
                if(value != null)
                    Selection(value);
            }
        }

        public string SelectedPosition
        {
            get
            {
                return gm.SelectedPosition;
            }

        }

        public string GameStatus
        {
            get
            {
                return gm.GameStatus;
            }

        }

        public string PlayerOnTurn
        {
            get
            {
                return Enum.ToObject(typeof(PlayerColor), gm.PlayerOnTurn.PlayerColor).ToString();
            }

        }

        public Visibility IsAI
        {
            get
            {
                return VisConvert((bool)gm.PlayerOnTurn.IsAI);
            }

        }

        public Visibility VisConvert(bool value)
        {
            bool bValue = (bool)value;
            return (bValue) ? Visibility.Visible : Visibility.Collapsed;
        }


        public bool Thinking
        {
            get
            {
                if (worker.IsBusy)
                    return true;
                else
                    return false;
            }

        }



        public void Selection(BoardItem sender)
        {
            if(!worker.IsBusy)
            {
                if (gm.PlayerOnTurn.IsAI)
                {
                    worker.RunWorkerAsync();
                    NotifyOfPropertyChange(() => Thinking);
                }
                else
                {
                    gm.GetInput(sender.Row, sender.Col);
                    ReloadUI();
                }
            }

        }

        public void OpenRules()
        {
            
        }

        public void ReloadUI()
        {
            AddUnitsFromGameBoard(BoardItems);
            NotifyOfPropertyChange(() => BoardItems);
            NotifyOfPropertyChange(() => PlayerOnTurn);
            NotifyOfPropertyChange(() => GameStatus);
            NotifyOfPropertyChange(() => SelectedPosition);
            NotifyOfPropertyChange(() => IsAI);
            NotifyOfPropertyChange(() => Thinking);
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            gm.GameTurn();
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ReloadUI();
        }









    }
}

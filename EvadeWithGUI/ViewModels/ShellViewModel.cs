using Caliburn.Micro;
using EvadeWithGUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static EvadeWithGUI.GameConstants;

namespace EvadeWithGUI.ViewModels
{
    public class ShellViewModel: Conductor<object>
    {
        GameManager gm;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public ShellViewModel()
        {
            NewGame();

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
                    _ = Selection(value);
            }
        }

        public string SelectedPosition
        {
            get
            {
                return gm.SelectedPosition;
            }

        }


        public string PlayerOnTurn
        {
            get
            {
                return Enum.ToObject(typeof(PlayerColor), gm.PlayerOnTurn.PlayerColor).ToString();
            }

        }

        public void Cancel()
        {
            cancellationTokenSource.Cancel();
        }

        public Visibility IsAI
        {
            get
            {
                return VisConvert((bool)gm.PlayerOnTurn.IsAI);
            }

        }

        public bool IsHuman
        {
            get
            {
                return !gm.PlayerOnTurn.IsAI;
            }

        }

        public Visibility VisConvert(bool value)
        {
            bool bValue = (bool)value;
            return (bValue) ? Visibility.Visible : Visibility.Collapsed;
        }

        private bool _isListBoxEnabled;
        public bool IsListBoxEnabled
        {
            get
            {
                return _isListBoxEnabled;
            }
            set
            {
                _isListBoxEnabled = value;
                NotifyOfPropertyChange(() => IsListBoxEnabled);
            }
        }
        public bool IsThinking
        {
            get
            {
                return !IsListBoxEnabled;
            }
        }

        public ObservableCollection<string> GameHistory
        {
            get
            {
                return HistoryToStrings(gm.GameHistory);
            }
        }

        public ObservableCollection<string> RedoStack
        {
            get
            {
                return HistoryToStrings(gm.RedoStack);
            }
        }

        public ObservableCollection<string> HistoryToStrings(Stack<List<int>> intHistory)
        {
            ObservableCollection<string> stringHistory = new ObservableCollection<string>();
            foreach (List<int> move in intHistory)
            {
                var result = string.Join(",", move);
                stringHistory.Add(result);
            }
            return stringHistory;
        }

        public void MenuUndo()
        {
            Undo();
        }

        public void MenuRedo()
        {
            Redo();
        }


        public void Undo()
        {
            Cancel();
            gm.UndoMove();
            cancellationTokenSource = new CancellationTokenSource();
            ReloadUI();
        }

        public void Redo()
        {
            Cancel();
            gm.RedoMove();
            cancellationTokenSource = new CancellationTokenSource();
            ReloadUI();
        }

        async Task Selection(BoardItem sender)
        {
            cancellationTokenSource = new CancellationTokenSource();

            if (gm.PlayerOnTurn.IsAI)
            {
                AIThinking(true);
                await gm.GameTurn(cancellationTokenSource);
                AIThinking(false);
                ReloadUI();
                
            }
            else
            {
                await gm.GetInput(sender.Row, sender.Col, cancellationTokenSource);
                ReloadUI();
            }
            CheckEndGame();

        }

        public async Task BestMove()
        {
            cancellationTokenSource = new CancellationTokenSource();
            if(!gm.PlayerOnTurn.IsAI)
            {
                    gm.PlayerOnTurn.IsAI = true;
                    ReloadUI();
                    AIThinking(true);
                    await gm.GameTurn(cancellationTokenSource);
                    if (!cancellationTokenSource.IsCancellationRequested)
                    {
                        gm.PlayerSwap().IsAI = false;
                        AIThinking(false);
                        ReloadUI();
                        CheckEndGame();
                    }
                    if (cancellationTokenSource.IsCancellationRequested)
                    {
                        gm.PlayerOnTurn.IsAI = false;
                        AIThinking(false);
                        ReloadUI();
                    }

            }
            

        }


        public void AIThinking(bool value)
        {
            IsListBoxEnabled = !value;
            NotifyOfPropertyChange(() => IsThinking);
        }

        public void ReloadUI()
        {
            AddUnitsFromGameBoard(BoardItems);
            NotifyOfPropertyChange(() => BoardItems);
            NotifyOfPropertyChange(() => PlayerOnTurn);
            NotifyOfPropertyChange(() => SelectedPosition);
            NotifyOfPropertyChange(() => IsAI);
            NotifyOfPropertyChange(() => IsHuman);
            NotifyOfPropertyChange(() => GameHistory);
            NotifyOfPropertyChange(() => RedoStack);
        }

        public void OpenSettings()
        {
            ActivateItem(new SettingsViewModel(this, gm));
        }

        private bool _active;
        public bool Active
        {
            get
            {
                return _active;
            }
            set
            {
                _active = value;
                NotifyOfPropertyChange(() => Active);
            }
        }

        public void NewGame()
        {
            gm = new GameManager();
            gm.StartGame();
            BoardItems = new ObservableCollection<BoardItem>();
            AddUnitsFromGameBoard(BoardItems);
            IsListBoxEnabled = true;
            Active = true;
            ReloadUI();
            OpenSettings();
        }

        public void OpenRules()
        {
            string path = System.IO.Directory.GetCurrentDirectory() + @"\Evade.pdf";
            System.Diagnostics.Process.Start(path);
        }

        public void CheckEndGame()
        {
            if (gm.GameInProgress == false)
            {
                MessageBoxResult result = MessageBox.Show(gm.GameStatus + "\nDo you want to start a New Game?",
                                  "Confirmation",
                                  MessageBoxButton.YesNo,
                                  MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                {
                    NewGame();
                }
                else
                    Application.Current.Shutdown();


            }
        }

        public void Exit()
        {
            MessageBoxResult result = MessageBox.Show("Do you want to exit Evade?",
                                          "Confirmation",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }











}
}

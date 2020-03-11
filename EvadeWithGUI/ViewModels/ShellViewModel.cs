using Caliburn.Micro;
using EvadeWithGUI.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
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
                if(gm.BestMove != null)
                {
                    boardItems.Add(new BoardItem() { Row = gm.BestMove[0], Col = gm.BestMove[1], PieceType = (GameConstants.States)99 });
                    boardItems.Add(new BoardItem() { Row = gm.BestMove[3], Col = gm.BestMove[4], PieceType = (GameConstants.States)99 });
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

        public string BestMoveInfo
        {
            get
            {
                NotifyOfPropertyChange(() => BestMoveShow);
                if (gm.BestMove != null)
                {
                    var result = string.Join(",", gm.BestMove);
                    return "Best Move: " + result;
                }
                else
                    return null;
            }

        }

        public Visibility BestMoveShow
        {
            get
            {
                if (gm.BestMove != null)
                    return VisConvert(true);
                else
                    return VisConvert(false);
            }

        }

        public void Cancel()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
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
                var result = "\u265a" + "\u2654" + string.Join(",", move);
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
            gm.BestMove = null;
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
                    gm.BestMove = await gm.PlayerOnTurn.AIPlay(gm.GameBoard, cancellationTokenSource);
                    if (!cancellationTokenSource.IsCancellationRequested)
                    {
                        gm.PlayerOnTurn.IsAI = false;
                        AIThinking(false);
                        ReloadUI();
                        CheckEndGame();
                    }
                    if (cancellationTokenSource.IsCancellationRequested)
                    {
                        gm.BestMove = null;
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
            NotifyOfPropertyChange(() => BestMoveInfo);
        }

        public void OpenSettings()
        {
            Cancel();
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
            Cancel();
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




        public void SaveGame()
        {
            Cancel();

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XML document|*.xml";
            saveFileDialog.FileName = "Game";
            saveFileDialog.DefaultExt = "xml";

            XDocument xmlDoc = new XDocument(
                new XElement("Game",
                    new XElement("Setting",
                        new XElement("SaveHash", (int)gm.GetHash(gm.GameHistory)),
                        new XElement("IsBlackAI", gm.PlayerOne.IsAI),
                        new XElement("IsWhiteAI", gm.PlayerTwo.IsAI),
                        new XElement("BlackDifficulty", (int)gm.PlayerOne.IQ),
                        new XElement("WhiteDifficulty", (int)gm.PlayerTwo.IQ)

                    ),
                    ExportMoveHistoryToXML().Element("MoveHistory"))
            );


            try
            {
                if (saveFileDialog.ShowDialog() == true)
                {
                    xmlDoc.Save(saveFileDialog.FileName);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Unable to save file \n{e.Message}", "Save game error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        private XDocument ExportMoveHistoryToXML()
        {
            XDocument xmlDoc = new XDocument(new XElement("MoveHistory"));

            string output = "";
            foreach (var list in gm.GameHistory)
            {
                list.ForEach((i) => output += i.ToString());
                xmlDoc.Root.Add(new XElement("Move", output));
                output = "";
            }

            return xmlDoc;
        }

        

        public void LoadGame()
        {
            Cancel();

            OpenFileDialog openFileDlg = new OpenFileDialog();
            openFileDlg.DefaultExt = ".xml";
            openFileDlg.Filter = "XML Files (*.xml)|*.xml";

            GameManager tempgm = new GameManager();
            tempgm.StartGame();
            List<List<string>> moveList = new List<List<string>>();
            XDocument xmlDoc = new XDocument();

            try
            {
                if (openFileDlg.ShowDialog() == true)
                {
                    xmlDoc = XDocument.Load(openFileDlg.FileName);

                    var saveHash = xmlDoc.Descendants("SaveHash")
                        .Select(element => int.Parse(element.Value.ToString()))
                        .FirstOrDefault();

                    moveList.AddRange(xmlDoc.Descendants("Move")
                        .Select(element => element.Value
                            .Select(x => x.ToString())
                            .ToList<string>()));

                    gm.PlayerOne.IsAI = xmlDoc.Descendants("IsBlackAI")
                        .Select(element => bool.Parse(element.Value.ToString()))
                        .FirstOrDefault();

                    gm.PlayerTwo.IsAI = xmlDoc.Descendants("IsWhiteAI")
                        .Select(element => bool.Parse(element.Value.ToString()))
                        .FirstOrDefault();

                    var aiLevelW = xmlDoc.Descendants("WhiteDifficulty")
                        .Select(element => int.Parse(element.Value.ToString()))
                        .FirstOrDefault();

                    var aiLevelB = xmlDoc.Descendants("BlackDifficulty")
                        .Select(element => int.Parse(element.Value.ToString()))
                        .FirstOrDefault();

                    if(!tempgm.PlayMoveHistory(MagicParse(moveList), saveHash))
                        throw new System.InvalidOperationException("Manipulated move list.");



                    if (aiLevelW > 0 && aiLevelW <= 4 && (aiLevelW % 1) == 0 &&
                        aiLevelB > 0 && aiLevelB <= 4 && (aiLevelB % 1) == 0)
                    {
                        gm.PlayerOne.IQ = aiLevelB;
                        gm.PlayerTwo.IQ = aiLevelW;
                    }
                    else
                        throw new System.InvalidOperationException("Invalid AI setting.");

                    gm = tempgm;
                    ReloadUI();


                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Unable to load file \n{e.Message}", "Load game error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public List<List<int>> MagicParse(List<List<string>> input)
        {
            List<List<int>> result = new List<List<int>>();
            foreach (List<string> list in input)
            {
                List<int> tempList = new List<int>();

                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] == "-")
                    {
                        string neg;
                        neg = list[i] + list[i + 1];
                        i++;
                        tempList.Add(int.Parse(neg));
                    }
                    else
                        tempList.Add(int.Parse(list[i]));
                }

                result.Add(tempList);

            }
            return result;
        }













    }
}

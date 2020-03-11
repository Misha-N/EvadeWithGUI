using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EvadeWithGUI.ViewModels
{
    class WinScreenViewModel : Screen
    {
        readonly ShellViewModel mainWindow;
        readonly GameManager manager;

        public WinScreenViewModel(ShellViewModel parent, GameManager gm)
        {
            mainWindow = parent;
            manager = gm;
            parent.Active = false;
        }

        public Visibility BearsWon
        {
            get
            {
                if (manager.PlayerOnTurn.PlayerColor == 1 || manager.GameStatus == "draw")
                    return mainWindow.VisConvert(true);
                else
                    return mainWindow.VisConvert(false);
            }
        }

        public string WinMessage
        {
            get
            {
                if (manager.GameStatus == "draw")
                    return "DRAW!";
                if (manager.PlayerOnTurn.PlayerColor == 1)
                    return "Bears WON!";
                if (manager.PlayerOnTurn.PlayerColor == -1)
                    return "Penguins WON!";
                else
                    return "";
            }
        }

        public Visibility PenguinsWon
        {
            get
            {
                if (manager.PlayerOnTurn.PlayerColor == -1 || manager.GameStatus == "draw")
                    return mainWindow.VisConvert(true);
                else
                    return mainWindow.VisConvert(false);
            }
        }

        public void Exit()
        {
            mainWindow.Exit();
        }

        public void Save()
        {
            mainWindow.SaveGame();
        }




        public void Close()
        {
            mainWindow.Active = true;
            mainWindow.ReloadUI();
            TryClose();
        }
    }
}

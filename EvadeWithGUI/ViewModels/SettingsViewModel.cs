using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvadeWithGUI.ViewModels
{
    class SettingsViewModel: Screen
    {
        readonly ShellViewModel mainWindow;
        readonly GameManager manager;

        public SettingsViewModel(ShellViewModel parent, GameManager gm)
        {
            mainWindow = parent;
            manager = gm;
            parent.Active = false;
        }

        public bool WhiteIsAI
        {
            get
            {
                return manager.PlayerTwo.IsAI;
            }
            set
            {
                manager.PlayerTwo.IsAI = value;
                NotifyOfPropertyChange(() => WhiteIsAI);
            }
        }

        public bool BlackIsAI
        {
            get
            {
                return manager.PlayerOne.IsAI;
            }
            set
            {
                manager.PlayerOne.IsAI = value;
                NotifyOfPropertyChange(() => BlackIsAI);
            }
        }

        public int WhiteInteligence
        {
            get
            {
                return manager.PlayerTwo.IQ;
            }
            set
            {
                manager.PlayerTwo.IQ = value;
                NotifyOfPropertyChange(() => WhiteInteligence);
            }
        }

        public int BlackInteligence
        {
            get
            {
                return manager.PlayerOne.IQ;
            }
            set
            {
                manager.PlayerOne.IQ = value;
                NotifyOfPropertyChange(() => BlackInteligence);
            }
        }
        public void Close()
        {
            mainWindow.Active = true;
            mainWindow.ReloadUI();
            TryClose();
        }
    }
}

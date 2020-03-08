using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evade
{
    class GameHistory
    {
        private Stack history = new System.Collections.Stack();

        public void AddToHistory(List<int> move)
        {
            history.Push(move);
        }

        public void DisplayHistory()
        {
            foreach (List<int> move in history)
                Console.WriteLine(move);
        }

    }
}

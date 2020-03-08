using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvadeWithGUI
{
    public static class GameConstants
    {
        // velikost hracího pole
        public static int boardSize = 6 + 2;
        public static int depth = 3;
        public static int min = -1000;
        public static int max = +1000;

    public enum States
    {
        wKing = -2,
        wMan = -1,
        empty = 0,
        bMan = 1,
        bKing = 2,
        barrier = 9,
        frozen = 8
    }

    public enum MoveResult
    {
        Moved = 0,
        Frozen = 1,
        Fail = -1
    }

    public enum PlayerColor
    {
        White = -1,
        Black = 1
    }


    public static Dictionary<string, string> moves = new Dictionary<string, string>()
        {
            {"A1","1,1"},{"A2","1,2"},{"A3","1,3"},{"A4","1,4"},{"A5","1,5"},{"A6","1,6"},
            {"B1","2,1"},{"B2","2,2"},{"B3","2,3"},{"B4","2,4"},{"B5","2,5"},{"B6","2,6"},
            {"C1","3,1"},{"C2","3,2"},{"C3","3,3"},{"C4","3,4"},{"C5","3,5"},{"C6","3,6"},
            {"D1","4,1"},{"D2","4,2"},{"D3","4,3"},{"D4","4,4"},{"D5","4,5"},{"D6","4,6"},
            {"E1","5,1"},{"E2","5,2"},{"E3","5,3"},{"E4","5,4"},{"E5","5,5"},{"E6","5,6"},
            {"F1","6,1"},{"F2","6,2"},{"F3","6,3"},{"F4","6,4"},{"F5","6,5"},{"F6","6,6"}
        };

    public static Dictionary<int, string> UI = new Dictionary<int, string>()
        {
            {(int)States.barrier," "},
            {(int)States.frozen,"x"},
            {(int)States.empty,"0"},
            {(int)States.wMan,"1"},
            {(int)States.wKing,"3"},
            {(int)States.bMan,"2"},
            {(int)States.bKing,"4"}
        };

    public enum MoveParts
    {
        row = 0,
        col = 1,
        piece = 2,
        newRow = 3,
        newCol = 4,
        newPosition = 5,
        result = 6
    }

}
}

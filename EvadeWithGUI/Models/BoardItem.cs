using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EvadeWithGUI.GameConstants;

namespace EvadeWithGUI.Models
{
    public class BoardItem
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public States PieceType { get; set; }

    }
}

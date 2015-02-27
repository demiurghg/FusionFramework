using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWarsLibrary.Fields
{
    public class AStarMod : AStar
    {
        public override int GetDistanceBetweenNeighbours(Cell from, Cell to)
        {
            return ((from.Type == CellType.DEEP) && (from.Type == to.Type)) ? 1 : 9999;
        }
    }
}

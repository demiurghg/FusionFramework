using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary.Field
{
    public enum CellType
    {
        LAND = 0,
        SHALLOW = 1,
        DEEP = 2
    }

    public class Cell
    {
        List<Cell> neighbours;
        public List<Cell> Neighbours { get { return neighbours; } }

        List<Cell> topology;
        public List<Cell> Topology { get { return topology; } }
        
        CellType type;
        public CellType Type { get { return type; } internal set { type = value; } }

        int i;
        public int I { get { return i; } internal set { i = value; } }

        int j;
        public int J { get { return j; } internal set { j = value; } }

        float x;
        internal float X { get { return x + Config.OffsetX; } set { x = value; } }

        float y;
        internal float Y { get { return y + Config.OffsetY; } set { y = value; } }

        double noise;
        internal double Noise { get { return noise; } set { noise = value; } }

        int depth;
        internal int Depth
        {
            get { return depth; }
            set { depth = value; }
        }

        public Cell(int i, int j, CellType type)
        {
            neighbours = new List<Cell>();
            this.i = i;
            this.j = j;
            x = i % 2 == 0 ? j * Config.HEX_SIZE : j * Config.HEX_SIZE + Config.HEX_SIZE / 2;
            y = (int)(i * Config.HEX_SIZE * (float)Math.Cos( Math.PI/6 ));// 3 / 4;
            this.type = type;
            noise = 0;
        }

        internal void addNeighbor(Cell cell) 
        {
            neighbours.Add(cell);
        }

        internal void createTopology()
        {
            Cell[] temp = new Cell[6];
            foreach (Cell neighbour in neighbours)
            {
                if (neighbour.Type == CellType.LAND) continue;
                if ((neighbour.I == I) && (neighbour.J == J + 1)) temp[0] = neighbour;
                if ((neighbour.I == I) && (neighbour.J == J - 1)) temp[3] = neighbour;
                if (I % 2 == 0)
                {
                    if ((neighbour.I == I - 1) && (neighbour.J == J)) temp[1] = neighbour;
                    if ((neighbour.I == I - 1) && (neighbour.J == J - 1)) temp[2] = neighbour;
                    if ((neighbour.I == I + 1) && (neighbour.J == J - 1)) temp[4] = neighbour;
                    if ((neighbour.I == I + 1) && (neighbour.J == J)) temp[5] = neighbour;
                }
                else
                {
                    if ((neighbour.I == I - 1) && (neighbour.J == J + 1)) temp[1] = neighbour;
                    if ((neighbour.I == I - 1) && (neighbour.J == J)) temp[2] = neighbour;
                    if ((neighbour.I == I + 1) && (neighbour.J == J)) temp[4] = neighbour;
                    if ((neighbour.I == I + 1) && (neighbour.J == J + 1)) temp[5] = neighbour;
                }
            }
            topology = temp.ToList<Cell>();
        }
    }
}

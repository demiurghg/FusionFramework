using SubmarinesWars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary.Field
{
    public class GameField
    {
        private int _height;
        private int _width;

        public int Height
        {
            get { return _height; }
        }
        public int Width
        {
            get { return _width; }
        }

        Cell[,] field;
        public Cell[,] Field { get { return field; } }

        double[,] primalNoise;

        public GameField()
        {
            _height = Config.FIELD_HEIGHT;
            _width = Config.FIELD_WIDTH;
            createField();
            createLandscape();
            createTopology();
            //createDepth();
        }

        void createField()
        {
            field = new Cell[_height, _width];
            primalNoise = new double[_height, _width];

            for (int i = 0; i < _height; i++)
                for (int j = 0; j < _width; j++)
                    field[i, j] = new Cell(i, j, CellType.DEEP);

            for (int i = 0; i < _height; i++)
                for (int j = 0; j < _width; j++)
                {
                    Cell cell = field[i, j];
                    if (i - 1 >= 0) cell.addNeighbor(field[i - 1, j]);
                    if (i + 1 < _height) cell.addNeighbor(field[i + 1, j]);
                    if (j - 1 >= 0) cell.addNeighbor(field[i, j - 1]);
                    if (j + 1 < _width) cell.addNeighbor(field[i, j + 1]);
                    if (i % 2 == 1)
                    {
                        if ((i - 1 >= 0) & (j + 1 < _width)) cell.addNeighbor(field[i - 1, j + 1]);
                        if ((i + 1 < _height) & (j + 1 < _width)) cell.addNeighbor(field[i + 1, j + 1]);
                    }
                    else
                    {
                        if ((i - 1 >= 0) & (j - 1 >= 0)) cell.addNeighbor(field[i - 1, j - 1]);
                        if ((i + 1 < _height) & (j - 1 >= 0)) cell.addNeighbor(field[i + 1, j - 1]);
                    }
                }
        }

        internal void createLandscape()
        {
            //Console.WriteLine(Config.SEED);
            PerlinNoiseGenerator.Seed = Config.SEED == 0 ? new Random().Next(Int32.MaxValue) : Config.SEED;
            //Console.WriteLine(PerlinNoiseGenerator.Seed);
            //Console.WriteLine(Int32.MaxValue);
            for (int i = 0; i < _height; i++)
                for (int j = 0; j < _width; j++)
                {
                    CellType type = CellType.DEEP;
                    double perlineNoise = PerlinNoiseGenerator.Noise(i, j);
                    if (perlineNoise > 80) type = CellType.LAND;
                    else
                        if (perlineNoise > 20) type = CellType.SHALLOW;
                    field[i, j].Type = type;
                }
            checkField();
        }

        internal void createTopology() {
            for (int i = 0; i < _height; i++)
                for (int j = 0; j < _width; j++)
                    field[i, j].createTopology();
        }

        //void createDepth()
        //{
        //    for (int i = 0; i < _height; i++)
        //        for (int j = 0; j < _width; j++)
        //            if (field[i, j].Type == CellType.LAND)
        //                field[i, j].Depth = 1;
        //            else
        //                if (field[i, j].Type == CellType.SHALLOW)
        //                    field[i, j].Depth = 0;
        //                else
        //                    field[i, j].Depth = Int16.MinValue;
        //    for (int k = 0; k < 100; k++)
        //        for (int i = 0; i < _height; i++)
        //            for (int j = 0; j < _width; j++)
        //                if (field[i, j].Type == CellType.DEEP)
        //                {
        //                    int max = field[i, j].Depth;
        //                    foreach (Cell c in field[i, j].Neighbours)
        //                        if (c.Type != CellType.LAND)
        //                            if (max < c.Depth)
        //                                max = c.Depth;
        //                    if (max > field[i, j].Depth)
        //                        field[i, j].Depth = max - 1;
        //                }
        //}

        Random rndS = new Random();

        Boolean checkField()
        {
            int k = Int16.MaxValue;
            int[,] was = null;
            
            while (k > (_height * _width / 10))
            {
                k = 0;
                Cell start = field[rndS.Next(_height), rndS.Next(_width)];
                while (start.Type == CellType.LAND)
                    start = start.Neighbours[rndS.Next(start.Neighbours.Count)];
                List<Cell> queue = new List<Cell>();
                was = new int[_height, _width];
                queue.Add(start);
                Cell current;
                while (queue.Count != 0)
                {
                    current = queue[0];
                    if (was[current.I, current.J] == 0)
                    {
                        was[current.I, current.J] = 1;
                        foreach (Cell neighbour in current.Neighbours)
                            if ((neighbour.Type != CellType.LAND) && (was[neighbour.I, neighbour.J] == 0))
                                queue.Add(neighbour);
                    }
                    queue.RemoveAt(0);
                }
                for (int i = 0; i < _height; i++)
                    for (int j = 0; j < _width; j++)
                        if ((was[i, j] == 0) && (field[i, j].Type != CellType.LAND))
                            k++;
            }
            for (int i = 0; i < _height; i++)
                for (int j = 0; j < _width; j++)
                    if ((was[i, j] == 0) && (field[i, j].Type != CellType.LAND))
                        field[i, j].Type = CellType.LAND;
            return true;
        }

        internal void calculateNoise()
        {
            initialNoise();
            clearNoise();
            addNoise();
            for (int i = 0; i < 100; i++)
            {
                calcNoise();
                addNoise();
            }
            clearPrimalNoise();
        }

        void initialNoise()
        {
            for (int i = 0; i < _height; i++)
                for (int j = 0; j < _width; j++)
                    if (primalNoise[i, j] < field[i, j].Noise) primalNoise[i, j] = field[i, j].Noise;
        }

        internal void addNewNoise(Cell cell, double noise)
        {
            if (primalNoise[cell.I, cell.J] < noise) primalNoise[cell.I, cell.J] = noise;
        }

        internal void coolNoise()
        {
            for (int i = 0; i < _height; i++)
                for (int j = 0; j < _width; j++)
                    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    //field[i, j].Noise = field[i, j].Noise / 1.2;
                    field[i, j].Noise = 0;
                    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        }
        
        void addNoise()
        {
            for (int i = 0; i < _height; i++)
                for (int j = 0; j < _width; j++)
                    if (primalNoise[i,j] > field[i,j].Noise) field[i, j].Noise = primalNoise[i, j];
        }

        void calcNoise()
        {
            double[,] newNoise = new double[_height, _width];
            for (int i = 0; i < _height; i++)
                for (int j = 0; j < _width; j++)
                    if (field[i, j].Type != CellType.LAND)
                    {
                        double sum = field[i,j].Noise;
                        double count = 0;
                        foreach (Cell neighbor in field[i, j].Neighbours)
                        {
                            if (neighbor.Type == CellType.LAND)
                            {
                                count += 0.1;
                            }
                            if (neighbor.Type == CellType.SHALLOW)
                            {
                                sum += neighbor.Noise;
                                count++;
                            }
                            if (neighbor.Type == CellType.DEEP)
                            {
                                sum += neighbor.Noise;
                                count++;
                            }
                        }
                        if (field[i, j].Type == CellType.SHALLOW) sum *= 0.95;
                        double avg = sum / (count+1);
                        newNoise[i, j] += avg;
                    }
            for (int i = 0; i < _height; i++)
                for (int j = 0; j < _width; j++)
                    field[i, j].Noise = newNoise[i, j];
        }

        void clearNoise()
        {
            for (int i = 0; i < _height; i++)
                for (int j = 0; j < _width; j++)
                    field[i, j].Noise = 0;
        }

        void clearPrimalNoise()
        {
            for (int i = 0; i < _height; i++)
                for (int j = 0; j < _width; j++)
                    primalNoise[i, j] = 0;
        }

        public List<Cell> getPath(Cell from, Cell to, Func<Cell, Cell, float> weightFunc = null)
        {
            return new AStar().FindPath(this, from, to, weightFunc);
        }
    }
}

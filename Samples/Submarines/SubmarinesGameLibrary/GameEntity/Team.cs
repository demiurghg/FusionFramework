using Fusion;
using Fusion.Mathematics;
using Fusion.Graphics;
using SubmarinesWars.SubmarinesGameLibrary.GameEntity;
using SubmarinesWars.SubmarinesGameLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubmarinesWars.SubmarinesGameLibrary.Field;
using SubmarinesWars.SubmarinesGameLibrary.ArtificialIntelligence;

namespace SubmarinesWars.SubmarinesGameLibrary.GameEntity
{
    public class Team : EntityCollection
    {
        private int _teamId;
        internal int TeamId
        {
            get { return _teamId; }
        }

        private Color _color;
        internal Color Color
        {
            get { return _color; }
        }

        private AI _ai;
        internal AI AI
        {
            get { return _ai; }
        }

        private GameField _field;
        internal GameField Field
        {
            get { return _field; }
        }

        private bool _lose;
        internal bool Lose
        {
            get { return _lose; }
        }

        internal int launchedTorpedo = 0;
        internal int placedMines = 0;

        public Team(int teamId, AI ai, GameField field)
            : base()
        {
            _teamId = teamId;
            _ai = ai;
            _field = field;
        }

        internal void Initialize(EntityCollection parent, Texture2D submarineTexture)
        {
            Parent = parent;
            Cell baseCell = null;
            switch (_teamId)
            {
                case 0:
                    baseCell = _field.Field[Config.FIELD_HEIGHT - 1, 0];
                    _color = Color.Red;
                    break;
                case 1:
                    baseCell = _field.Field[0, Config.FIELD_WIDTH - 1];
                    _color = Color.Yellow;
                    break;
            }
            createBase(baseCell);
            for (int i = 0; i < 5; i++)
                createSubmarine(submarineTexture);
        }

        internal void InitForReplay(EntityCollection parent)
        {
            Parent = parent;
            switch (_teamId)
            {
                case 0:
                    _color = Color.Red;
                    break;
                case 1:
                    _color = Color.Yellow;
                    break;
            }
        }

        void createSubmarine(Texture2D submarineTexture)
        {
            List<BaseC> baseCell = new List<BaseC>();
            foreach (VisibleObject obj in Collection)
                if (obj is BaseC) baseCell.Add((BaseC)obj);
            List<Cell> basePoints = new List<Cell>();
            foreach (BaseC baseC in baseCell)
                basePoints.Add(baseC.Cell);
            List<Cell> queue = new List<Cell>();
            foreach (Cell basePoint in basePoints)
                foreach (Cell cell in basePoint.Neighbours)
                    if (cell.Type != CellType.LAND)
                        if (!basePoints.Contains(cell))
                            queue.Add(cell);
            Boolean f = true;
            Cell spawnPoint = queue[0];
            while (f)
            {
                f = false;
                foreach (Submarine bs in ((EntityCollection)Parent).getSubmarines())
                    if ((bs.Cell.I == spawnPoint.I) && (bs.Cell.J == spawnPoint.J))
                    {
                        f = true;
                        break;
                    }
                if (f)
                {
                    foreach (Cell cell in spawnPoint.Neighbours)
                        if (cell.Type != CellType.LAND)
                            if (!basePoints.Contains(cell))
                                if (!queue.Contains(cell))
                                    queue.Add(cell);
                    queue.RemoveAt(0);
                    spawnPoint = queue[0];
                }
            }
            addToCollection(new Submarine(spawnPoint, this, submarineTexture));
        }

        internal override void detectNoise()
        {
            List<Tuple<Submarine, List<Tuple<Cell, double>>>> noiseDetected = new List<Tuple<Submarine, List<Tuple<Cell, double>>>>();
            foreach (Submarine sub in Submarines)
            {
                List<Tuple<Cell, double>> noise = sub.detectNoise();
                if (noise != null)
                    noiseDetected.Add(new Tuple<Submarine, List<Tuple<Cell, double>>>(sub, noise));
            }
           _ai.DetectNoise(noiseDetected);
        }
        
        Random rnd = new Random();
        void createBase(Cell initialCell)
        {
            List<Cell> basePoints = new List<Cell>();
            Cell temp = initialCell;
            if (temp.Type == CellType.LAND)
            {
                List<Cell> queue = new List<Cell>();
                queue.AddRange(temp.Neighbours);
                Boolean find = false;
                temp = queue[0];
                while (!find)
                    if (temp.Type != CellType.LAND)
                        find = true;
                    else
                    {
                        foreach (Cell neighbour in temp.Neighbours)
                            if (!queue.Contains(neighbour))
                                queue.Add(neighbour);
                        queue.RemoveAt(0);
                        temp = queue[0];
                    }
            }
            
            basePoints.Add(temp);
            for (int i = 0; i < 2; i++)
            {
                Cell c = temp.Neighbours[rnd.Next(temp.Neighbours.Count)];
                Boolean f = false;
                while ((c.Type == CellType.LAND) || (!f))
                {
                    c = temp.Neighbours[rnd.Next(temp.Neighbours.Count)];
                    f = true;
                    foreach (Cell curBase in basePoints)
                        if ((curBase.I == c.I) && (curBase.J == c.J))
                        {
                            f = false;
                            break;
                        }
                }
                basePoints.Add(c);
                temp = c;
            }
            foreach (Cell cell in basePoints) {
                this.addToCollection(new BaseC(cell, this));
            }
        }

        internal override void Remove(VisibleObject obj)
        {
            if (Collection.Contains(obj))
            {
                if (obj is Submarine)
                {
                    Submarine sub = obj as Submarine;
                    ((EntityCollection)Parent).addToCollection(new SunkSubmarine(sub.Cell, this.Color));
                }
                Collection.Remove(obj);
            }
        }

        internal override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Submarines.Count == 0)
                _lose = true;
        }

        internal override void GlobalUpdate()
        {
            base.GlobalUpdate();
            detectNoise();
        }

        internal override void Draw(SpriteBatch sb, DebugStrings ds, int order, StereoEye stereoEye)
        {
            base.Draw(sb, ds, order, stereoEye);
            
            int k = 0;
            if (Config.HEX_SIZE < 24) k = 1;

            int j = 1;
            if (Config.HEX_SIZE < 24) j = 0;

            int pinY = ((int)Field.Field[Config.FIELD_HEIGHT - 1, Config.FIELD_WIDTH - 1].Y + Config.HEX_SIZE * 11 / 8) + TeamId * k * Config.HEX_SIZE;
            int pinX = (int)Field.Field[0, 0].X + TeamId * j * (int)Field.Field[0, (Config.FIELD_WIDTH - 1) / 2].X;
            object[] o = new object[Submarines.Count * 4];
            int i = 0;
            String str = "";
            foreach (Submarine sub in Submarines)
            {
                o[i] = Config.Names[sub.Number];
                str += "{" + i++ + "} - ";
                o[i] = sub.Health;
                str += "{" + i++ + "}/";
                o[i] = sub.TorpedoCount;
                str += "{" + i++ + "}/";
                o[i] = sub.MinesCount;
                str += "{" + i++ + "}   ";
            }
			sb.DrawDebugString(pinX, pinY, string.Format(str, o), Color);
        }

        internal override List<Submarine> getSubmarines()
        {
            List<Submarine> submarines = new List<Submarine>();
            foreach (VisibleObject obj in Collection)
                if (obj is Submarine) submarines.Add((Submarine)obj);
            return submarines;
        }

        internal override List<Mine> getMines()
        {
            List<Mine> mines = new List<Mine>();
            foreach (VisibleObject obj in Collection)
                if (obj is Mine) mines.Add((Mine)obj);
            return mines;
        }

        internal List<BaseC> getBases()
        {
            List<BaseC> bases = new List<BaseC>();
            foreach (VisibleObject obj in Collection)
                if (obj is BaseC) bases.Add((BaseC)obj);
            return bases;
        }

        public List<Submarine> Submarines
        {
            get { return getSubmarines(); }
        }
        public List<Mine> Mines
        {
            get { return getMines(); }
        }
        public List<BaseC> Bases
        {
            get { return getBases(); }
        }

        internal override VisibleObject Copy(VisibleObject parent)
        {
            Team newTeam = new Team(TeamId, AI, Field);
            newTeam.InitForReplay((EntityCollection)parent);
            foreach (VisibleObject obj in Collection)
                newTeam.addToCollection(obj.Copy(newTeam));
            return newTeam;
        }
    }
}



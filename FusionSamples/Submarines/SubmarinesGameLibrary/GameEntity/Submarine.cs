using Fusion;
using Fusion.Mathematics;
using Fusion.Graphics;
using SubmarinesWars.SubmarinesGameLibrary.Field;
using SubmarinesWars.SubmarinesGameLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary.GameEntity
{
    public class Submarine : Entity
    {
        internal static int count;

        private int _torpedoCount;
        private int _minesCount;
        private int _healths;
        private int _number;

        public int TorpedoCount
        {
            get { return _torpedoCount; }
        }
        public int MinesCount
        {
            get { return _minesCount; }
        }
        public int Health
        {
            get { return _healths; }
        }

        internal int Number
        {
            get { return _number; }
        }

        Texture2D _health;

        public Team Team
        {
            get { return (Team)base.Parent; }
            internal set { base.Parent = (EntityCollection)value; }
        }

        public Submarine(Cell cell, Team team, Texture2D texture)
        {
            this.Cell = cell;
            this.X = cell.X - Config.OffsetX;
            this.Y = cell.Y - Config.OffsetY;
            this.Parent = team;
            this._healths = 3;
            this.Texture = texture;
            this._health = LogicService.health;
            Order = 5;
            _number = count;
            count++;
            _torpedoCount = Config.TORPEDO_COUNT;
            _minesCount = Config.MINES_COUNT;
        }

        internal Submarine(Cell cell, Team team, Texture2D texture, int number, int health, int tCount, int mCount)
        {
            this.Cell = cell;
            this.X = cell.X - Config.OffsetX;
            this.Y = cell.Y - Config.OffsetY;
            this.Parent = team;
            this._healths = health;
            this.Texture = texture;
            this._health = LogicService.health;
            Order = 5;
            _number = number;
            _torpedoCount = tCount;
            _minesCount = mCount;
        }

        internal void damage()
        {
            _healths--;
            if (_healths == 0) die();
        }

        internal void die()
        {
            ToRemove = true;
        }

        internal List<Tuple<Cell, double>> detectNoise()
        {
            GameField field = ((Team)Parent).Field;
            List<Tuple<Cell, Double>> noise = new List<Tuple<Cell, Double>>();
            bool origin = true;
            foreach (Cell neighbour in Cell.Neighbours)
                if (neighbour.Type != CellType.LAND)
                {
                    noise.Add(new Tuple<Cell, double>(neighbour, neighbour.Noise));
                    if (Cell.Noise < neighbour.Noise) origin = false;
                }
            if (!origin) 
                return noise;
            else
                return null;
        }

        internal bool launchTorpedo()
        {
            if (_torpedoCount > 0)
            {
                _torpedoCount--;
                return true;
            }
            return false;
        }

        internal bool placeMine()
        {
            if (_minesCount > 0)
            {
                _minesCount--;
                return true;
            }
            return false;
        }

        internal void reload()
        {
            _torpedoCount = Config.TORPEDO_COUNT;
            _minesCount = Config.MINES_COUNT;
        }

        internal override void GlobalUpdate()
        {
        }

        internal override void Update(GameTime gameTime) 
        {
            if (ToRemove)
            {
                Parent.Remove(this);
            }
        }

        internal override void Draw(SpriteBatch sb, DebugStrings ds, StereoEye stereoEye)
        {
            float currentX = X + Config.OffsetX;
            float currentY = Y + Config.OffsetY;

            float offsetScale = 0;
            if (stereoEye == StereoEye.Left)
                offsetScale = -Config.offsetScale;
            if (stereoEye == StereoEye.Right)
                offsetScale = Config.offsetScale;

            sb.Draw(Texture, currentX - offsetScale, currentY, Config.HEX_SIZE, Config.HEX_SIZE, ((Team)Parent).Color);
            float x = ((Team)Parent).TeamId % 2 == 0 ? currentX + 230 * Config.HEX_SIZE / 1000 : currentX + 50 * Config.HEX_SIZE / 1000;
            float y = currentY + 380 * Config.HEX_SIZE / 1000;
            int starSize = Config.HEX_SIZE / 3;
            for (int i = 0; i < _healths; i++)
            {
                sb.Draw(_health, x - offsetScale, y + Config.HEX_SIZE / 3, starSize, starSize, ((Team)Parent).Color);
                x += starSize * 55 / 100 + 1;
            }
        }

        internal override VisibleObject Copy(VisibleObject parent)
        {
            return new Submarine(Cell, (Team)parent, Texture, Number, Health, TorpedoCount, MinesCount);
        }
    }
}

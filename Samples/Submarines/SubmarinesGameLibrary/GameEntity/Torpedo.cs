using Fusion;
using Fusion.Mathematics;
using Fusion.Graphics;
using SubmarinesWars.SubmarinesGameLibrary.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary.GameEntity
{
    internal class Torpedo : Entity
    {
        private Submarine _launcher;
        public Submarine Launcher
        {
            get { return _launcher; }
        }
        private double _noise;
        public double Noise
        {
            get { return _noise; }
        }

        private List<Cell> pathList;
        private Texture2D path;

        public Torpedo(Submarine launcher)
        {
            Cell = launcher.Cell;
            X = Cell.X;
            Y = Cell.Y;
            Texture = LogicService.torpedo;
            _launcher = launcher;
            pathList = new List<Cell>();
            path = LogicService.path;
            _noise = Config.NOISE_BOOM_TORPEDO;
            Order = 4;
        }


        internal override void Update(Fusion.GameTime gameTime)
        {
            if (!pathList.Contains(Cell))
                pathList.Add(Cell);
        }



        internal override void Draw(Fusion.Graphics.SpriteBatch sb, Fusion.Graphics.DebugStrings ds, StereoEye stereoEye)
        {
            float currentX = X + Config.OffsetX;
            float currentY = Y + Config.OffsetY;

            float offsetScale = 0;
            if (stereoEye == StereoEye.Left)
                offsetScale = -Config.offsetScale;
            if (stereoEye == StereoEye.Right)
                offsetScale = Config.offsetScale;

            var color = new Color(255, 255, 128, 255);
            foreach (Cell cell in pathList)
                sb.Draw(path, cell.X - offsetScale, cell.Y, Config.HEX_SIZE, Config.HEX_SIZE, color);
            sb.Draw(Texture, currentX - offsetScale, currentY, Config.HEX_SIZE, Config.HEX_SIZE, color);
        }

        internal override VisibleObject Copy(VisibleObject parent)
        {
            return this;
        }
    }
}

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
    public class Mine : Entity
    {
        private double _noise;
        internal double Noise
        {
            get { return _noise; }
        }

        internal Boolean activated = false;
        internal Boolean detonated = false;

        public Team Team
        {
            get { return (Team)base.Parent; }
            internal set { base.Parent = (EntityCollection)value; }
        }

        internal Mine(Submarine sub)
        {
            Texture = LogicService.mine;
            Parent = sub.Parent;
            Cell = sub.Cell;
            _noise = Config.NOISE_BOOM_MINE;
            Order = 3;
        }

        internal Mine(Cell cell, Team team)
        {
            Texture = LogicService.mine;
            Parent = team;
            Cell = cell;
            _noise = Config.NOISE_BOOM_MINE;
            Order = 3;
        }

        internal void activate()
        {
            activated = true;
        }

        internal override void Draw(Fusion.Graphics.SpriteBatch sb, Fusion.Graphics.DebugStrings ds, StereoEye stereoEye)
        {
            float offsetScale = 0;
            if (stereoEye == StereoEye.Left)
                offsetScale = -Config.offsetScale;
            if (stereoEye == StereoEye.Right)
                offsetScale = Config.offsetScale;
            sb.Draw(Texture, Cell.X - offsetScale, Cell.Y, Config.HEX_SIZE, Config.HEX_SIZE, Team.Color); 
        }

        internal override VisibleObject Copy(VisibleObject parent)
        {
            Mine mine = new Mine(Cell, (Team)parent);
            mine.activate();
            return mine;
        }
    }
}

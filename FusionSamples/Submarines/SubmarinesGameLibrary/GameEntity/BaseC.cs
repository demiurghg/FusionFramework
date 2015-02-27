using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Graphics;
using SubmarinesWars.SubmarinesGameLibrary.Field;
using Fusion;
using Fusion.Mathematics;

namespace SubmarinesWars.SubmarinesGameLibrary.GameEntity
{
    public class BaseC : Entity
    {
        public BaseC(Cell cell, Team team)
        {
            Cell = cell;
            Parent = team;
            Texture = LogicService.baseC;
            Order = 1;
        }

        internal override void Update(GameTime gameTime)
        {
            //throw new NotImplementedException();
        }

        internal override void Draw(SpriteBatch sb, Fusion.Graphics.DebugStrings ds, StereoEye stereoEye)
        {
            float offsetScale = 0;
            if (stereoEye == StereoEye.Left)
                offsetScale = -Config.offsetScale;
            if (stereoEye == StereoEye.Right)
                offsetScale = Config.offsetScale;

            sb.Draw(Texture, Cell.X - offsetScale, Cell.Y, Config.HEX_SIZE, Config.HEX_SIZE, ((Team)Parent).Color);
        }
    }
}

using Fusion;
using Fusion.Graphics;
using Fusion.Mathematics;
using SubmarinesWars.SubmarinesGameLibrary.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary.GameEntity
{
    internal class SunkSubmarine : Entity
    {
        internal Color color;

        public SunkSubmarine(Cell cell, Color color)
        {
            Texture = LogicService.sunk;
            this.color = color;
            this.Cell = cell;
            Order = 2;
        }

        internal override void Update(GameTime gameTime)
        {
            //throw new NotImplementedException();
        }

        internal override void Draw(Fusion.Graphics.SpriteBatch sb, Fusion.Graphics.DebugStrings ds, StereoEye stereoEye)
        {
            float offsetScale = 0;
            if (stereoEye == StereoEye.Left)
                offsetScale = -Config.offsetScale;
            if (stereoEye == StereoEye.Right)
                offsetScale = Config.offsetScale;
            if (Cell.Type == CellType.SHALLOW) offsetScale = 0;
            sb.Draw(Texture, Cell.X + offsetScale, Cell.Y, Config.HEX_SIZE, Config.HEX_SIZE, color);
        }

        internal override VisibleObject Copy(VisibleObject parent)
        {
            return new SunkSubmarine(Cell, color);
        }
    }
}

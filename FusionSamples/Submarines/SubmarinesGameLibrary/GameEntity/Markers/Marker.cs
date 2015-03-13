using Fusion;
using Fusion.Mathematics;
using Fusion.Graphics;
using SubmarinesWars.SubmarinesGameLibrary.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary.GameEntity.Markers
{
    public class Marker : Entity
    {
        public Marker(Texture2D texture, Cell cell)
        {
            Texture = texture;
            Cell = cell;
            Order = 0;
        }

        internal override void Update(Fusion.GameTime gameTime)
        {
            if (ToRemove)
                Parent.Remove(this);
        }

        internal override void GlobalUpdate()
        {
            ToRemove = true;
        }

        internal override void Draw(SpriteBatch sb, DebugStrings ds, StereoEye stereoEye)
        {
            sb.Draw(Texture, Cell.X - 1.4f, Cell.Y - 1.4f, Config.HEX_SIZE + 2.8f, Config.HEX_SIZE + 2.8f, Color.White);
        }

        internal override VisibleObject Copy(VisibleObject parent)
        {
            Marker marker = new Marker(Texture, Cell);
            marker.Parent = (EntityCollection)parent;
            return marker;
        }
    }
}

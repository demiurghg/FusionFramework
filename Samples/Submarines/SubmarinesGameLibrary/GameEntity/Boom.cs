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
    internal class Boom : Entity
    {
        double startedTime;
        EntityCollection parent;

        public Boom(Cell cell, GameTime gameTime, EntityCollection parent)
        {
            Texture = LogicService.boom;
            this.Cell = cell;
            startedTime = gameTime.Total.TotalSeconds;
            this.parent = parent;
            Order = 6;
        }

        internal override void Delay(double delay)
        {
            startedTime += delay;
        }

        internal override void GlobalUpdate()
        {
            if (ToRemove)
                parent.Remove(this);
        }

        internal override void Update(GameTime gameTime)
        {
            if (gameTime.Total.TotalSeconds - startedTime > 1.0f / Config.SPEED) ToRemove = true;
        }

        internal override void Draw(Fusion.Graphics.SpriteBatch sb, Fusion.Graphics.DebugStrings ds, StereoEye stereoEye)
        {
            float offsetScale = 0;
            if (stereoEye == StereoEye.Left)
                offsetScale = -Config.offsetScale;
            if (stereoEye == StereoEye.Right)
                offsetScale = Config.offsetScale;
            sb.Draw(Texture, Cell.X - offsetScale, Cell.Y, Config.HEX_SIZE, Config.HEX_SIZE, new Color(255, 255, 128, 255));
        }
    }
}

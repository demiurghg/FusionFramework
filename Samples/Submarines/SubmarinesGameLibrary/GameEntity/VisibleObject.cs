using Fusion;
using Fusion.Mathematics;
using Fusion.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary.GameEntity
{
    abstract public class VisibleObject
    {
        internal abstract void Draw(SpriteBatch sb, DebugStrings ds, int order, StereoEye stereoEye);
        internal abstract void Draw(SpriteBatch sb, DebugStrings ds, StereoEye stereoEye);
        internal abstract void Update(GameTime gameTime);
        internal abstract void GlobalUpdate();
        internal abstract bool Intersection(VisibleObject obj);
        internal abstract void Remove(VisibleObject obj);
        internal abstract void Delay(double delay);
        internal abstract VisibleObject Copy(VisibleObject parent);
    }
}

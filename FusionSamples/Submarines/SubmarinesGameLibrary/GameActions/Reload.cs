using SubmarinesWars.SubmarinesGameLibrary.GameEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary.GameActions
{
    class Reload : Action
    {
        public Reload(Submarine submarine)
        {
            Entity = submarine;
        }

        public override bool execute(Fusion.GameTime gameTime)
        {
            ((Submarine)Entity).reload();
            return true;
        }
    }
}

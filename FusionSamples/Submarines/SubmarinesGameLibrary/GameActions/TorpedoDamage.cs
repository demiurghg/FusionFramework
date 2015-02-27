using SubmarinesWars.SubmarinesGameLibrary.GameEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary.GameActions
{
    class TorpedoDamage : Action
    {
        public TorpedoDamage(Submarine submarine, ActionsQueue queue)
        {
            Entity = submarine;
            ActionsQueue = queue;
        }

        public override bool execute(Fusion.GameTime gameTime)
        {
            ((Submarine)Entity).damage();
            ((Team)Entity.Parent).AI.NotifyTorpedoDamage((Submarine)Entity);
            return true;
        }
    }
}

using SubmarinesWars.SubmarinesGameLibrary.GameEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary.GameActions
{
    class MineDamage : Action
    {
        public MineDamage(Submarine submarine, ActionsQueue queue)
        {
            Entity = submarine;
            ActionsQueue = queue;
        }

        public override bool execute(Fusion.GameTime gameTime)
        {
            ((Submarine)Entity).damage();
            ((Team)Entity.Parent).AI.NotifyMineDamage((Submarine)Entity);
            if (Entity.ToRemove)
                ActionsQueue.deleteAllFor(Entity);
            return true;
        }
    }
}

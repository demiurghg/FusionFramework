using SubmarinesWars.SubmarinesGameLibrary.GameEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary.GameActions
{
    class Accident : Action
    {
        Submarine sub1;

        public Accident(Submarine sub, Submarine sub1, ActionsQueue queue)
        {
            Entity = sub;
            this.sub1 = sub1;
            ActionsQueue = queue;
        }

        public override bool execute(Fusion.GameTime gameTime)
        {
            if (Entity.Cell == sub1.Cell)
            {
                ((Submarine)Entity).damage();
                ((Team)Entity.Parent).AI.NotifyAccident((Submarine)Entity);
                ActionsQueue.Field.addNewNoise(Entity.Cell, Config.NOISE_ACCIDENT);
                if (Entity.ToRemove) ActionsQueue.deleteAllFor(Entity);
            }
            return true;
        }
    }
}

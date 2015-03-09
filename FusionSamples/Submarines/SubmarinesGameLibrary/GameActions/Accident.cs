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
        public Accident(Submarine sub, ActionsQueue queue)
        {
            Entity = sub;
            ActionsQueue = queue;
        }

        public override bool execute(Fusion.GameTime gameTime)
        {
            ((Submarine)Entity).damage();
            ((Team)Entity.Parent).AI.NotifyAccident((Submarine)Entity);
            ActionsQueue.Field.addNewNoise(Entity.Cell, Config.NOISE_ACCIDENT);
            if (Entity.ToRemove) ActionsQueue.deleteAllFor(Entity); 
            return true;
        }
    }
}

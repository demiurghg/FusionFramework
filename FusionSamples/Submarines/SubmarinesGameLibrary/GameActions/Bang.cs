using Fusion;
using Fusion.Mathematics;
using SubmarinesWars.SubmarinesGameLibrary.Field;
using SubmarinesWars.SubmarinesGameLibrary.GameEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary.GameActions
{
    class Bang : Action
    {
        public Bang(Entity entity, ActionsQueue queue)
        {
            Entity = entity;
            ActionsQueue = queue;
        }

        public override bool execute(GameTime gameTime)
        {
            ActionsQueue.GameCollection.addToCollection(new Boom(Entity.Cell, gameTime, ActionsQueue.GameCollection));
            if (Entity is Torpedo)
                ActionsQueue.Field.addNewNoise(Entity.Cell, ((Torpedo)Entity).Noise);
            if (Entity is Mine)
                ActionsQueue.Field.addNewNoise(Entity.Cell, ((Mine)Entity).Noise);
            ActionsQueue.GameCollection.Remove(Entity);
            return true;
        }
    }
}

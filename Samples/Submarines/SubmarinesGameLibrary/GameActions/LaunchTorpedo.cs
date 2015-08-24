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
    class LaunchTorpedo : Action
    {
        List<Cell> path;
        public LaunchTorpedo(Entity entity, List<Cell> path, ActionsQueue queue)
        {
            if (((Submarine)entity).launchTorpedo())
            {
                ((Submarine)entity).Team.launchedTorpedo++;
                Entity = new Torpedo((Submarine)entity);
                entity.Parent.addToCollection(Entity);
            }
            else
                Entity = null;
            ActionsQueue = queue;
            this.path = path;
        }

        public override bool execute(GameTime gameTime)
        {
            if (Entity != null)
                ActionsQueue.addAction(new ParsePath(Entity, path, ActionsQueue), 1);
            return true;
        }
    }
}

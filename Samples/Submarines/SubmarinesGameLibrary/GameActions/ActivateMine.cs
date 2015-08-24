using Fusion;
using Fusion.Mathematics;
using SubmarinesWars.SubmarinesGameLibrary.GameEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary.GameActions
{
    class ActivateMine : Action
    {
        private List<Submarine> submarines;
        internal void setSubs(List<Submarine> submarines)
        {
            this.submarines = submarines;
        }

        public ActivateMine(Entity mine, ActionsQueue queue)
        {
            Entity = mine;
            ActionsQueue = queue;
        }

        public override bool execute(GameTime gameTime)
        {
            if (Entity != null)
            {
                Mine mine = Entity as Mine;
                mine.activate();
                foreach (Submarine sub in submarines)
                    if (mine.Cell == sub.Cell)
                    {
                        ActionsQueue.addAction(new Bang(mine, ActionsQueue));
                        ActionsQueue.addAction(new MineDamage(sub, ActionsQueue));
                    }
            }                                  
            return true;
        }
    }
}

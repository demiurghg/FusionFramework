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
        public ActivateMine(Entity mine)
        {
            Entity = mine;
        }

        public override bool execute(GameTime gameTime)
        {
            Mine mine = Entity as Mine;
            mine.activate();
            return true;
        }
    }
}

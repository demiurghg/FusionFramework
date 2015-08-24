using Fusion;
using Fusion.Mathematics;
using Fusion.Graphics;
using SubmarinesWars.SubmarinesGameLibrary.Field;
using SubmarinesWars.SubmarinesGameLibrary.GameEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary.GameActions
{
    class PlaceMine : Action
    {
        public PlaceMine(Entity entity, ActionsQueue queue)
        {
            Entity = entity;
            ActionsQueue = queue;
        }

        public override bool execute(GameTime gameTime)
        {
            Submarine sub = Entity as Submarine;
            if ((sub.Cell.Type == CellType.SHALLOW) && (sub.placeMine()))
            {
                sub.Team.placedMines++;
                Mine mine = new Mine(sub);
                sub.Parent.addToCollection(mine);
                ActivateMine act = new ActivateMine(mine, ActionsQueue);
                act.setSubs(((EntityCollection)sub.Team.Parent).getSubmarines());
                ActionsQueue.addAction(act);
            }
            return true;
        }
    }
}

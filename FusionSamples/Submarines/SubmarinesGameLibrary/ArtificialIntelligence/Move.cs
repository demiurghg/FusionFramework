using SubmarinesWars.SubmarinesGameLibrary.Field;
using SubmarinesWars.SubmarinesGameLibrary.GameActions;
using SubmarinesWars.SubmarinesGameLibrary.GameEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary.ArtificialIntelligence
{
    public class Move : AIAction
    {
        public Move(List<Cell> path)
        {
            if (path != null)
                this.path = path;
            else
                this.path = new List<Cell>();
        }

        internal override void execute(Submarine sub, ActionsQueue queue)
        {
            queue.addAction(new SubmarinesGameLibrary.GameActions.ParsePath(sub, path, queue));
        }
    }
}

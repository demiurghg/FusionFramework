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
            this.path = new List<Cell>();
            if (path != null)
                this.path.AddRange(path);
            if (this.path.Count > 4)
                this.path.RemoveRange(4, path.Count - 4);
        }

        internal override void execute(Submarine sub, ActionsQueue queue)
        {
            queue.addAction(new SubmarinesGameLibrary.GameActions.ParsePath(sub, path, queue));
        }
    }
}

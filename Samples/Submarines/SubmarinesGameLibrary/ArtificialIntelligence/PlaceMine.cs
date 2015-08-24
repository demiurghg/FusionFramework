using SubmarinesWars.SubmarinesGameLibrary.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary.ArtificialIntelligence
{
    public class PlaceMine : AIAction
    {
        public PlaceMine(List<Cell> path)
        {
            this.path = new List<Cell>();
            if (path != null)
                this.path.AddRange(path);
            if (this.path.Count > 2)
                this.path.RemoveRange(2, path.Count - 2);
        }

        internal override void execute(GameEntity.Submarine sub, GameActions.ActionsQueue queue)
        {
            queue.addAction(new SubmarinesGameLibrary.GameActions.PlaceMine(sub, queue));
            queue.addAction(new SubmarinesGameLibrary.GameActions.ParsePath(sub, path, queue));
        }
    }
}

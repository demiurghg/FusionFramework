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
    public class LaunchTorpedo : AIAction
    {
        public LaunchTorpedo(List<Cell> path)
        {
            this.path = new List<Cell>();
            if (path != null)
                this.path.AddRange(path);
            if (this.path.Count > 9)
                this.path.RemoveRange(9, path.Count - 9);
        }

        internal override void execute(Submarine sub, ActionsQueue queue)
        {
            queue.addAction(new SubmarinesGameLibrary.GameActions.LaunchTorpedo(sub, path, queue));
        }
    }
}

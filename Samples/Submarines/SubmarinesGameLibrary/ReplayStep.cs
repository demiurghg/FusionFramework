using SubmarinesWars.SubmarinesGameLibrary.GameEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary
{
    internal class ReplayStep
    {
        internal int stepNumb;
        internal EntityCollection collection;
        internal ReplayAction action;

        internal ReplayStep(int stepNumb, EntityCollection collection, ReplayAction action)
        {
            this.stepNumb = stepNumb;
            this.collection = collection;
            this.action = action;
        }
    }
}

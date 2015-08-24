using SubmarinesWars.SubmarinesGameLibrary.ArtificialIntelligence;
using SubmarinesWars.SubmarinesGameLibrary.GameEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary
{
    internal class ReplayAction
    {
        internal AIAction action;
        internal int subNumb;

        public ReplayAction(AIAction action, int subNumb)
        {
            this.action = action;
            this.subNumb = subNumb;
        }
    }
}

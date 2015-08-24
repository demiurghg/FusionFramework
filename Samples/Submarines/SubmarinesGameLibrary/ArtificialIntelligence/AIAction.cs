using SubmarinesWars.SubmarinesGameLibrary.Field;
using SubmarinesWars.SubmarinesGameLibrary.GameActions;
using SubmarinesWars.SubmarinesGameLibrary.GameEntity;
using SubmarinesWars.SubmarinesGameLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary.ArtificialIntelligence
{
    abstract public class AIAction
    {
        internal List<Cell> path;

        abstract internal void execute(Submarine sub, ActionsQueue queue);
    }
}

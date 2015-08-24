using Fusion;
using Fusion.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary.Interfaces
{
    interface IAction
    {
        bool execute(GameTime gameTime);
    }
}

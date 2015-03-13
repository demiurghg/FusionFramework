using SubmarinesWars.SubmarinesGameLibrary.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary.GameEntity.Markers
{
    public class Aim : Marker
    {
        public Aim(Cell cell)
            : base(LogicService.aim, cell)
        { }

        internal override VisibleObject Copy(VisibleObject parent)
        {
            Marker marker = new Aim(Cell);
            marker.Parent = (EntityCollection)parent;
            return marker;
        }
    }
}

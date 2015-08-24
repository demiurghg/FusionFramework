using SubmarinesWars.SubmarinesGameLibrary.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary.GameEntity.Markers
{
    public class Flag : Marker
    {
        public Flag(Cell cell)
            : base(LogicService.flag, cell)
        { }

        internal override VisibleObject Copy(VisibleObject parent)
        {
            Marker marker = new Flag(Cell);
            marker.Parent = (EntityCollection)parent;
            return marker;
        }
    }
}

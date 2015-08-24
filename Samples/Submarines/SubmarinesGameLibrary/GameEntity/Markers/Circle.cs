using SubmarinesWars.SubmarinesGameLibrary.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary.GameEntity.Markers
{
    public class Circle : Marker
    {
        public Circle(Cell cell)
            : base(LogicService.circle, cell)
        { }

        internal override VisibleObject Copy(VisibleObject parent)
        {
            Marker marker = new Circle(Cell);
            marker.Parent = (EntityCollection)parent;
            return marker;
        }
    }
}

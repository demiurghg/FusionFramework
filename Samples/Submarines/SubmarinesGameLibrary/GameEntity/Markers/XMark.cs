using SubmarinesWars.SubmarinesGameLibrary.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary.GameEntity.Markers
{
    public class XMark : Marker
    {
        public XMark(Cell cell)
            : base(LogicService.xMark, cell)
        { }

        internal override VisibleObject Copy(VisibleObject parent)
        {
            Marker marker = new XMark(Cell);
            marker.Parent = (EntityCollection)parent;
            return marker;
        }
    }
}

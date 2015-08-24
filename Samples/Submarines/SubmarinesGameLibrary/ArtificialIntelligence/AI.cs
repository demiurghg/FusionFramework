using SubmarinesWars.SubmarinesGameLibrary.Field;
using SubmarinesWars.SubmarinesGameLibrary.GameEntity;
using SubmarinesWars.SubmarinesGameLibrary.GameEntity.Markers;
using SubmarinesWars.SubmarinesGameLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary.ArtificialIntelligence
{
    public class AI
    {
        private List<Marker> markers = new List<Marker>();
        internal List<Marker> Markers
        {
            get 
            {
                List<Marker> copy = new List<Marker>();
                copy.AddRange(markers);
                markers.Clear();
                return copy; 
            }
        }

        public void addMarker(Marker marker)
        {
            markers.Add(marker);
        }

        public AI()
        {
        }

        public virtual void DetectNoise(List<Tuple<Submarine, List<Tuple<Cell, double>>>> noise)
        {
        }

        public virtual void NotifyTorpedoDamage(GameEntity.Submarine sub)
        {
        }

        public virtual void NotifyMineDamage(GameEntity.Submarine sub)
        {
        }

        public virtual void NotifyAccident(GameEntity.Submarine sub)
        {
        }

        Random rnd = new Random();

        public virtual AIAction NextAction(Submarine sub, GameField field)
        {
            for (int i = 0; i < 5; i++)
                switch (rnd.Next(5))
                {
                    case 0:
                        addMarker(new Aim(field.Field[rnd.Next(field.Height), rnd.Next(field.Width)]));
                        break;
                    case 1:
                        addMarker(new Circle(field.Field[rnd.Next(field.Height), rnd.Next(field.Width)]));
                        break;
                    case 2:
                        addMarker(new Check(field.Field[rnd.Next(field.Height), rnd.Next(field.Width)]));
                        break;
                    case 3:
                        addMarker(new Flag(field.Field[rnd.Next(field.Height), rnd.Next(field.Width)]));
                        break;
                    case 4:
                        addMarker(new XMark(field.Field[rnd.Next(field.Height), rnd.Next(field.Width)]));
                        break;
                }
            List<Cell> path;
            switch (rnd.Next(3))
            {
                case 0 :
                    int count = rnd.Next(3) + 1;
                    int moveX = rnd.Next(Config.FIELD_HEIGHT);
                    int moveY = rnd.Next(Config.FIELD_WIDTH);
                    while (field.Field[moveX, moveY].Type == CellType.LAND)
                    {
                        moveX = rnd.Next(Config.FIELD_HEIGHT);
                        moveY = rnd.Next(Config.FIELD_WIDTH);
                    }
                    path = field.getPath(sub.Cell, field.Field[moveX, moveY]);
                    if (path == null)
                        path = new List<Cell>();
                    return new Move(path);
                case 1 :
                    count = rnd.Next(3) + 1;
                    moveX = rnd.Next(Config.FIELD_HEIGHT);
                    moveY = rnd.Next(Config.FIELD_WIDTH);
                    while (field.Field[moveX, moveY].Type == CellType.LAND)
                    {
                        moveX = rnd.Next(Config.FIELD_HEIGHT);
                        moveY = rnd.Next(Config.FIELD_WIDTH);
                    }
                    path = field.getPath(sub.Cell, field.Field[moveX, moveY]);
                    if (path == null)
                        path = new List<Cell>();
                    return new PlaceMine(path);
                case 2 :
                    int x = rnd.Next(Config.FIELD_HEIGHT);
                    int y = rnd.Next(Config.FIELD_WIDTH);
                    while ((field.Field[x, y].Type == CellType.LAND) || ((x == sub.Cell.I) && (y == sub.Cell.J)))
                    {
                        x = rnd.Next(Config.FIELD_HEIGHT);
                        y = rnd.Next(Config.FIELD_WIDTH);
                    }
                    path = field.getPath(sub.Cell, field.Field[x, y]);
                    return new LaunchTorpedo(path);
                default:
                    return null;
            }
        }
    }
}

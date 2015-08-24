using Fusion;
using Fusion.Mathematics;
using Fusion.Graphics;
using SubmarinesWars.SubmarinesGameLibrary.Field;
using SubmarinesWars.SubmarinesGameLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary.GameEntity
{
    public abstract class Entity : VisibleObject
    {
        private Cell _cell;
        private float _x;
        private float _y;
        private int _order;

        private bool _toRemove = false;

        private EntityCollection _parent;
        private Texture2D _texture;

        internal int Order
        {
            get { return _order; }
            set { _order = value; }
        }

        public Cell Cell
        {
            get { return _cell; }
            internal set { _cell = value; }
        }
        internal float X
        {
            get { return _x; }
            set { _x = value; }
        }
        internal float Y
        {
            get { return _y; }
            set { _y = value; }
        }

        internal bool ToRemove
        {
            get { return _toRemove; }
            set { if (value == true) _toRemove = value; }
        }

        internal EntityCollection Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        internal Texture2D Texture
        {
            get { return _texture; }
            set { _texture = value; }
        }

        internal override void Remove(VisibleObject obj) 
        {
        }

        internal override bool Intersection(VisibleObject obj)
        {
            if (obj is Entity) {
                Entity _obj = (Entity)obj;
                return (this._cell == _obj._cell);
            }
            else 
            {
                return false;
            }
        }

        internal override void Update(GameTime gameTime) { }
        internal override void Draw(SpriteBatch sb, DebugStrings ds, StereoEye stereoEye) { }
        internal override void Draw(SpriteBatch sb, DebugStrings ds, int order, StereoEye stereoEye)
        {
            if (order == _order)
                Draw(sb, ds, stereoEye);
        }
        internal override void Delay(double delay) { }
        internal override void GlobalUpdate() { }
        internal override VisibleObject Copy(VisibleObject parent)
        {
            return this;
        }
    }
}

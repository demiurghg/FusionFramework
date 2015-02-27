using Fusion;
using Fusion.Mathematics;
using SubmarinesWars.SubmarinesGameLibrary.Field;
using SubmarinesWars.SubmarinesGameLibrary.GameEntity;
using SubmarinesWars.SubmarinesGameLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary.GameActions
{
    abstract internal class Action : IAction
    {
        Entity _entity;
        ActionsQueue _queue;
        private double _startTime;
        private double _noise;
        private bool _first = true;

        public bool First
        {
            get { return _first; }
            set { if (value == false) _first = value; }
        }

        public double StartTime
        {
            get { return _startTime; }
            set { _startTime = value; }
        }

        public double Noise
        {
            get { return _noise; }
            set { _noise = value; }
        }

        public Entity Entity
        {
            get { return _entity; }
            set { _entity = value; }
        }

        public ActionsQueue ActionsQueue
        {
            get { return _queue; }
            set { _queue = value; }
        }

        abstract public bool execute(GameTime gameTime);
    }
}

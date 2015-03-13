using Fusion;
using Fusion.Mathematics;
using SubmarinesWars.SubmarinesGameLibrary.Field;
using SubmarinesWars.SubmarinesGameLibrary.GameEntity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary.GameActions
{
    internal class ActionsQueue
    {
        List<Action> _queue;
        EntityCollection _gameCollection;
        GameField _field;

        public EntityCollection GameCollection
        {
            get { return _gameCollection; }
            internal set { _gameCollection = value; }
        }

        public GameField Field
        {
            get { return _field; }
        }

        private int _size;
        public int Size
        {
            get { return _size; }
            private set { _size = value; }
        }

        public ActionsQueue(EntityCollection gameCollection, GameField field)
        {
            _field = field;
            _queue = new List<Action>();
            _size = _queue.Count;
            _gameCollection = gameCollection;
        }

        public void addAction(Action action, int numb = Int16.MaxValue)
        {
            if (numb == Int16.MaxValue)
                _queue.Add(action);
            else
                _queue.Insert(numb, action);
            _size = _queue.Count;
        }

        public void deleteAllFor(Entity entity)
        {
            List<Action> copy = new List<Action>();
            copy.AddRange(_queue);
            for (int i = 1; i < _queue.Count; i++)
                if ((copy[i] is Bang) || (copy[i] is MineDamage))
                    _queue.Remove(copy[i]);
            _size = _queue.Count;
        }

        public void nextAction(GameTime gameTime)
        {
            //Stopwatch timer = new Stopwatch();
            //timer.Start();
            if (_size != 0)
            {
                if (_queue[0].execute(gameTime))
                {
                    double endTime = gameTime.Total.TotalSeconds;
                    _queue.RemoveAt(0);
                    _size--;
                    if (_queue.Count != 0)
                    {
                        _queue[0].StartTime = endTime;
                    }
                }
            }
            //timer.Stop();
            //if (timer.Elapsed.Ticks > 100) 
            //    Console.WriteLine("Next action " + timer.Elapsed.Ticks);
        }

        public void clear()
        {
            _queue.Clear();
            _size = 0;
        }

        public void delay(double delay)
        {
            if (_queue.Count != 0)
            {
                _queue[0].StartTime += delay;
            }
        }
    }
}

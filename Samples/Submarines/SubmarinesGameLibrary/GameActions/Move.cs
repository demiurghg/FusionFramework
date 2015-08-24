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
    class Move : Action
    {
        Cell to;
        float _speed;
        
        static int k = 0;

        public Move(Entity sub, Cell to, ActionsQueue queue, float speed, double noise) 
        {
            k++;
            Entity = sub;
            this.to = to;
            ActionsQueue = queue;
            Noise = noise;
            _speed = speed;
        }

        float lerp(float from, float to, float factor)
        {
            return from + (to - from) * factor;
        }

        public override bool execute(GameTime gameTime)
        {
            _speed = Config.SPEED;

            

            //Stopwatch timer = new Stopwatch();
            //timer.Start();
            if (First)
            {
                StartTime = gameTime.Total.TotalSeconds;
                First = false;
            }

            //Console.WriteLine(k + " - " + StartTime + " - " + gameTime.Total.TotalSeconds);

            float factor = (gameTime.Total.TotalSeconds - StartTime) < 1.0f / _speed ? (float)(gameTime.Total.TotalSeconds - StartTime) * _speed / 1.0f : 1.0f;
            Entity.X = lerp(Entity.Cell.X - Config.OffsetX, to.X - Config.OffsetX, factor);
            Entity.Y = lerp(Entity.Cell.Y - Config.OffsetY, to.Y - Config.OffsetY, factor);
            if (factor == 1.0f)
            {
                ActionsQueue.Field.addNewNoise(Entity.Cell, Noise);
                ActionsQueue.Field.addNewNoise(to, Noise);
                Entity.Cell = to;
                //timer.Stop();
                //Console.WriteLine("Move. FrameID: " + gameTime.FrameID + ". Time: " + timer.Elapsed);
                return true;
            }
            //timer.Stop();
            //Console.WriteLine("Move. FrameID: " + gameTime.FrameID + ". Time: " + timer.Elapsed);
            return false;
        }
    }
}

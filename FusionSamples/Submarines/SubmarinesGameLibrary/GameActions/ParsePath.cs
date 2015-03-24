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
    class ParsePath : Action
    {
        List<Cell> path;
        float _speed;

        public ParsePath(Entity entity, List<Cell> path, ActionsQueue queue)
        {
            Entity = entity;
            ActionsQueue = queue;
            this.path = new List<Cell>();
            this.path.AddRange(path);

            if (Entity is Submarine)
            {
                if (this.path.Count > 4)
                    this.path.RemoveRange(4, this.path.Count - 4);
                switch (this.path.Count)
                {
                    case 0:
                        Noise = 0;
                        _speed = 1.0f * Config.SPEED;
                        break;
                    case 1:
                        Noise = 0;
                        _speed = 1.0f * Config.SPEED;
                        break;
                    case 2:
                        Noise = Config.NOISE_1_STEP;
                        _speed = 1.0f * Config.SPEED;
                        break;
                    case 3:
                        Noise = Config.NOISE_2_STEP;
                        _speed = 1.5f * Config.SPEED;
                        break;
                    case 4:
                        Noise = Config.NOISE_3_STEP;
                        _speed = 2.0f * Config.SPEED;
                        break;
                }
            }
            if (Entity is Torpedo)
            {
                if (this.path.Count > 13)
                    this.path.RemoveRange(13, path.Count - 13);
                Noise = Config.NOISE_TORPEDO_STEP;
                _speed = 3.0f * Config.SPEED;
            }
        }

        Boolean checkPath()
        {
            if (path.Count == 0) return true;
            if (path.Count == 1) return false;
            if (path[0] != Entity.Cell)
                return false;
            for (int i = 0; i < path.Count - 1; i++)
                if (!path[i].Neighbours.Contains(path[i + 1]))
                    return false;
            return true;
        }

        public override bool execute(GameTime gameTime)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            if (!checkPath())
                return true;

            List<Action> actionList = new List<Action>();
            for (int i = 0; i < path.Count - 1; i++)
                if (!new Check(Entity, ActionsQueue, Noise, path[i+1], path[i], _speed).CheckState(ref actionList))
                    break;

            for (int i = 0; i < actionList.Count; i++)
                ActionsQueue.addAction(actionList[i], i + 1);
            if (Entity is Torpedo)
                ActionsQueue.addAction(new Bang(Entity, ActionsQueue)); 
                
            timer.Stop();
            //Console.WriteLine("ParsePath. FrameID: " + gameTime.FrameID + ". Time: " + timer.Elapsed);
            return true;
        }
    }
}

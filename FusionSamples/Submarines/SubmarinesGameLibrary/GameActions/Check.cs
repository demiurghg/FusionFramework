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
    class Check
    {
        Entity entity;
        ActionsQueue queue;
        Double noise;
        Cell to;
        Cell from;
        float speed;

        public Check(Entity entity, ActionsQueue queue, Double noise, Cell to, Cell from, float speed)
        {
            this.noise = noise;
            this.queue = queue;
            this.entity = entity;
            this.to = to;
            this.from = from;
            this.speed = speed;
        }

        public bool CheckState(ref List<Action> actionList)
        {
            //Stopwatch timer = new Stopwatch();
            //timer.Start();
            
            List<Submarine> submarines = queue.GameCollection.getSubmarines();
            List<Mine> mines = queue.GameCollection.getMines();
            if (entity is Submarine) {
                Submarine sub = entity as Submarine;
                actionList.Add(new Move(sub, to, queue, speed, noise));
                foreach (BaseC basec in sub.Team.Bases)
                    if (basec.Cell == to)
                        actionList.Add(new Reload(sub));
                foreach (Submarine submarine in submarines)
                {
                    if (sub != submarine)
                    {
                        if (to == submarine.Cell)
                        {
                            actionList.Add(new Accident(sub, submarine, queue));
                            actionList.Add(new Accident(submarine, sub, queue));
                            actionList.Add(new Move(sub, from, queue, speed, noise));
                            foreach (Mine mine in mines)
                            {
                                if (from == mine.Cell)
                                {
                                    actionList.Add(new Bang(mine, queue));
                                    actionList.Add(new MineDamage(sub, queue));
                                }
                            }
                            //timer.Stop();
                            //Console.WriteLine("Check. FrameID: " + gameTime.FrameID + ". Time: " + timer.Elapsed);
                            return false;
                        }
                    }
                }
                foreach (Mine mine in mines)
                {
                    if ((to == mine.Cell) && (mine.activated))
                    {
                        actionList.Add(new Bang(mine, queue));
                        actionList.Add(new MineDamage(sub, queue));
                        //timer.Stop();
                        //Console.WriteLine("Check. FrameID: " + gameTime.FrameID + ". Time: " + timer.Elapsed);
                        return true;
                    }
                }
            }
            if (entity is Torpedo)
            {
                Torpedo torpedo = entity as Torpedo;
                actionList.Add(new Move(torpedo, to, queue, speed, noise));
                foreach (Submarine submarine in submarines)
                {
                    if ((to == submarine.Cell) && (submarine != torpedo.Launcher))
                    {
                        actionList.Add(new Bang(torpedo, queue));
                        actionList.Add(new TorpedoDamage(submarine, queue));
                        //timer.Stop();
                        //Console.WriteLine("Check. FrameID: " + gameTime.FrameID + ". Time: " + timer.Elapsed);
                        return false;
                    }
                }
                foreach (Submarine submarine in submarines)
                {
                    if ((to.Neighbours.Contains(submarine.Cell)) && (submarine != torpedo.Launcher))
                    {
                        actionList.Add(new Move(torpedo, submarine.Cell, queue, speed, noise));
                        actionList.Add(new Bang(torpedo, queue));
                        actionList.Add(new TorpedoDamage(submarine, queue));
                        //timer.Stop();
                        //Console.WriteLine("Check. FrameID: " + gameTime.FrameID + ". Time: " + timer.Elapsed);
                        return false;
                    }
                }
            }

            //timer.Stop();
            //Console.WriteLine("Check. FrameID: " + gameTime.FrameID + ". Time: " + timer.Elapsed);
            return true;
        }
    }
}

using Fusion.Mathematics;
using SubmarinesWars.SubmarinesGameLibrary.ArtificialIntelligence;
using SubmarinesWars.SubmarinesGameLibrary.Field;
using SubmarinesWars.SubmarinesGameLibrary.GameEntity;
using SubmarinesWars.SubmarinesGameLibrary.GameEntity.Markers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary
{
    internal class ReplayManager
    {
        StreamWriter sw;
        StreamReader sr;

        internal ReplayManager(String aiL, String aiR)
        {
            String filepath = @"./Replay/" + aiL + @"/" + aiR;
            DirectoryInfo directoryInfo = new DirectoryInfo(filepath);
            if (!directoryInfo.Exists)
                directoryInfo.Create();
            int count = directoryInfo.GetFiles().Count();
            sw = new StreamWriter(filepath + @"/" + count);
            sw.WriteLine("Start game");
            sw.Flush();
        }

        internal ReplayManager(String path)
        {
            sr = new StreamReader(path);
        }

        internal void InitPos(EntityCollection collection)
        {
            sw.WriteLine("Init position");
            saveCollection(collection);
            sw.WriteLine("End init position");
            sw.Flush();
        }

        internal EntityCollection ReadInitPos(GameField field)
        {
            EntityCollection gameCollection = new EntityCollection();
            string str = sr.ReadLine();
            while (!str.Equals("End init position"))
            {
                if (str.Contains("Team"))
                {
                    Team team = readTeam(Int32.Parse(str.Split(' ').ElementAt(1)), field);
                    team.InitForReplay(gameCollection);
                    gameCollection.addToCollection(team);
                }
                str = sr.ReadLine();
            }
            return gameCollection;
        }

        internal void saveCollection(EntityCollection collection)
        {
            sw.WriteLine("Collection");
            foreach (VisibleObject vo in collection.Collection)
            {
                if (vo is SunkSubmarine)
                    saveSunkSubmarine(vo as SunkSubmarine);
                if (vo is Marker)
                    if (!(vo is CustomMarker))
                        saveMarker(vo);
                if (vo is Team)
                    saveTeam(vo as Team);
            }
            sw.WriteLine("End collection");
            sw.Flush();
        }

        internal EntityCollection ReadCollection(GameField field)
        {
            String str = sr.ReadLine();
            EntityCollection collection = new EntityCollection();
            while (!str.Contains("End collection"))
            {
                if (str.Contains("Sunk submarine"))
                    collection.addToCollection(ReadSunkSubmarine(field));
                if (str.Contains("Marker"))
                {
                    Marker marker = ReadMarker(field);
                    marker.Parent = collection;
                    collection.addToCollection(marker);
                }
                if (str.Contains("Team")) 
                {
                    Team team = readTeam(Int32.Parse(str.Split(' ').ElementAt(1)), field);
                    team.InitForReplay(collection);
                    collection.addToCollection(team);
                }
                str = sr.ReadLine();
            }
            return collection;
        }

        internal void saveMarker(VisibleObject vo)
        {
            sw.WriteLine("Marker");
            if (vo is Aim)
                sw.WriteLine("Aim");
            if (vo is Circle)
                sw.WriteLine("Circle");
            if (vo is Check)
                sw.WriteLine("Check");
            if (vo is Flag)
                sw.WriteLine("Flag");
            if (vo is XMark)
                sw.WriteLine("XMark");
            Marker marker = vo as Marker;
            sw.WriteLine(marker.Cell.I + " " + marker.Cell.J);
            sw.WriteLine("End marker");
            sw.Flush();
        }

        internal Marker ReadMarker(GameField field)
        {
            String str = sr.ReadLine();
            Marker marker = null;
            while (!str.Contains("End marker"))
            {
                String type = str;
                str = sr.ReadLine();
                switch (type)
                {
                    case "Aim":
                        marker = new Aim(field.Field[Int32.Parse(str.Split(' ').ElementAt(0)), Int32.Parse(str.Split(' ').ElementAt(1))]);
                        break;
                    case "Circle":
                        marker = new Circle(field.Field[Int32.Parse(str.Split(' ').ElementAt(0)), Int32.Parse(str.Split(' ').ElementAt(1))]);
                        break;
                    case "Check":
                        marker = new Check(field.Field[Int32.Parse(str.Split(' ').ElementAt(0)), Int32.Parse(str.Split(' ').ElementAt(1))]);
                        break;
                    case "Flag":
                        marker = new Flag(field.Field[Int32.Parse(str.Split(' ').ElementAt(0)), Int32.Parse(str.Split(' ').ElementAt(1))]);
                        break;
                    case "XMark":
                        marker = new XMark(field.Field[Int32.Parse(str.Split(' ').ElementAt(0)), Int32.Parse(str.Split(' ').ElementAt(1))]);
                        break;
                }
                str = sr.ReadLine();
            }
            return marker;
        }

        internal void saveSunkSubmarine(SunkSubmarine ss)
        {
            sw.WriteLine("Sunk submarine");
            sw.WriteLine(ss.Cell.I + " " + ss.Cell.J + " " + ss.color.ToRgba());
            sw.WriteLine("End sunk submarine");
            sw.Flush();
        }

        internal SunkSubmarine ReadSunkSubmarine(GameField field)
        {
            String str = sr.ReadLine();
            SunkSubmarine ss = null;
            while (!str.Contains("End sunk submarine"))
            {
                ss = new SunkSubmarine(field.Field[Int32.Parse(str.Split(' ').ElementAt(0)), Int32.Parse(str.Split(' ').ElementAt(1))], new Color(Int32.Parse(str.Split(' ').ElementAt(2))));
                str = sr.ReadLine();
            }
            return ss;
        }

        internal void saveTeam(Team team)
        {
            sw.WriteLine("Team {0}", team.TeamId);
            sw.WriteLine("Base cells");
            foreach (BaseC cell in team.getBases())
                sw.WriteLine(cell.Cell.I + " " + cell.Cell.J);
            sw.WriteLine("End base cells");
            sw.WriteLine("Submarines");
            foreach (Submarine sub in team.getSubmarines())
                sw.WriteLine(sub.Cell.I + " " + sub.Cell.J + " " + sub.Number + " " + sub.Health + " " + sub.TorpedoCount + " " + sub.MinesCount);
            sw.WriteLine("End submarines");
            sw.WriteLine("Mines");
            foreach (Mine mine in team.getMines())
                sw.WriteLine(mine.Cell.I + " " + mine.Cell.J);
            sw.WriteLine("End mines");
            sw.WriteLine("End team {0}", team.TeamId);
            sw.Flush();
        }

        internal Team readTeam(int teamId, GameField field)
        {
            Team team = new Team(teamId, new AI(), field);
            String str = sr.ReadLine();
            while (!str.Contains("End team"))
            {
                if (str.Equals("Base cells"))
                {
                    str = sr.ReadLine();
                    while (!str.Equals("End base cells"))
                    {
                        team.addToCollection(new BaseC(field.Field[Int32.Parse(str.Split(' ').ElementAt(0)), Int32.Parse(str.Split(' ').ElementAt(1))], team));
                        str = sr.ReadLine();
                    }
                }
                if (str.Equals("Submarines"))
                {
                    str = sr.ReadLine();
                    while (!str.Equals("End submarines"))
                    {
                        team.addToCollection(new Submarine(field.Field[Int32.Parse(str.Split(' ').ElementAt(0)), Int32.Parse(str.Split(' ').ElementAt(1))],
                                                           team, team.TeamId == 0 ? LogicService.submarineL : LogicService.submarineR, Int32.Parse(str.Split(' ').ElementAt(2)),
                                                           Int32.Parse(str.Split(' ').ElementAt(3)), Int32.Parse(str.Split(' ').ElementAt(4)), Int32.Parse(str.Split(' ').ElementAt(5))));
                        str = sr.ReadLine();
                    }
                }
                if (str.Equals("Mines"))
                {
                    str = sr.ReadLine();
                    while (!str.Equals("End mines"))
                    {
                        team.addToCollection(new Mine(field.Field[Int32.Parse(str.Split(' ').ElementAt(0)), Int32.Parse(str.Split(' ').ElementAt(1))], team));
                        str = sr.ReadLine();
                    }
                }
                str = sr.ReadLine();
            }
            return team;
        }

        internal void saveAction(AIAction action, Submarine sub)
        {
            sw.WriteLine("Action");
            sw.WriteLine("Type");
            if (action is PlaceMine)
                sw.WriteLine("PlaceMine");
            if (action is LaunchTorpedo)
                sw.WriteLine("LaunchTorpedo");
            if (action is Move)
                sw.WriteLine("Move");
            sw.WriteLine("End type");
            sw.WriteLine("Path");
            foreach (Cell cell in action.path)
                sw.WriteLine(cell.I + " " + cell.J);
            sw.WriteLine("End path");
            sw.WriteLine("Submarine");
            sw.WriteLine(sub.Number);
            sw.WriteLine("End submarine");
            sw.WriteLine("End action");
            sw.Flush();
        }

        internal ReplayAction ReadAction(GameField field)
        {
            String str = sr.ReadLine();
            String type = "";
            int subNumb = 0;
            List<Cell> path = new List<Cell>();
            while (!str.Contains("End action"))
            {
                if (str.Contains("Type"))
                {
                    str = sr.ReadLine();
                    type = str;
                    str = sr.ReadLine();
                }
                if (str.Contains("Path"))
                {
                    str = sr.ReadLine();
                    while (!str.Contains("End path"))
                    {
                        path.Add(field.Field[Int32.Parse(str.Split(' ').ElementAt(0)), Int32.Parse(str.Split(' ').ElementAt(1))]);
                        str = sr.ReadLine();
                    }
                }
                if (str.Contains("Submarine"))
                {
                    str = sr.ReadLine();
                    subNumb = Int32.Parse(str);
                    str = sr.ReadLine();
                }
                str = sr.ReadLine();
            }
            AIAction action = null;
            switch (type)
            {
                case "Move":
                    action = new Move(path);
                    break;
                case "LaunchTorpedo":
                    action = new LaunchTorpedo(path);
                    break;
                case "PlaceMine":
                    action = new PlaceMine(path);
                    break;
            }
            return new ReplayAction(action, subNumb);
        }

        internal void saveStep(EntityCollection collection, AIAction action, Submarine sub)
        {
            sw.WriteLine("Step {0}", LogicService.stepCount);
            saveCollection(collection);
            saveAction(action, sub);
            sw.WriteLine("End step {0}", LogicService.stepCount);
            sw.Flush();
        }

        internal ReplayStep ReadStep(GameField field, int stepNumb)
        {
            String str = sr.ReadLine();
            EntityCollection collection = null;
            ReplayAction action = null;
            while (!str.Contains("End step"))
            {
                if (str.Contains("Collection"))
                    collection = ReadCollection(field);
                if (str.Contains("Action"))
                    action = ReadAction(field);
                str = sr.ReadLine();
            }
            return new ReplayStep(stepNumb, collection, action);
        }

        internal List<ReplayStep> ReadGame(GameField field)
        {
            List<ReplayStep> replaySteps = new List<ReplayStep>();
            String str = sr.ReadLine();
            int k = 1;
            while (!str.Contains("End game"))
            {
                replaySteps.Add(ReadStep(field, k++));
                str = sr.ReadLine();
            }
            return replaySteps;
        }

        internal void End()
        {
            sw.WriteLine("End game");
            sw.Flush();
            sw.Close();
        }
    }
}

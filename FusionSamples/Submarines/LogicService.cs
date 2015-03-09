using Fusion;
using Fusion.Graphics;
using SubmarinesWars.SubmarinesGameLibrary.Field;
using SubmarinesWars.SubmarinesGameLibrary.GameEntity;
using SubmarinesWars.SubmarinesGameLibrary.GameActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubmarinesWars.SubmarinesGameLibrary.GameEntity.Markers;
using SubmarinesWars.SubmarinesGameLibrary;
using SubmarinesWars.SubmarinesGameLibrary.ArtificialIntelligence;
using System.Reflection;
using System.IO;
using System.Diagnostics;

namespace SubmarinesWars
{
    public class LogicService : GameService
    {
        public class SubmarinesConfig
        {
            string _AIDllTeamR = @"default";
            public string AI_Dll_TeamR
            {
                get { return _AIDllTeamR; }
                set { _AIDllTeamR = value; }
            }

            string _AIClassTeamR = @"default";
            public string AI_Class_TeamR
            {
                get { return _AIClassTeamR; }
                set { _AIClassTeamR = value; }
            }

            string _AIDllTeamL = @"default";
            public string AI_Dll_TeamL
            {
                get { return _AIDllTeamL; }
                set { _AIDllTeamL = value; }
            }

            string _AIClassTeamL = @"default";
            public string AI_Class_TeamL
            {
                get { return _AIClassTeamL; }
                set { _AIClassTeamL = value; }
            }

            public SubmarinesConfig()
            {
            }
        }

        [Config]
        public SubmarinesConfig cfg { get; set; }

        [Command]
        internal AI LoadAI(int teamId)
        {
            String DllPath = "";
            String AIPath = "";
            Assembly Dll = null;
            Type AIType = null;

            switch (teamId)
            {
                case 0 :
                    DllPath = cfg.AI_Dll_TeamR;
                    AIPath = cfg.AI_Class_TeamR;
                    break;
                case 1 :
                    DllPath = cfg.AI_Dll_TeamL;
                    AIPath = cfg.AI_Class_TeamL;
                    break;
            }
            

            if (DllPath != "default")
            {
                try
                {
                    FileInfo f = new FileInfo(DllPath);
                    Dll = Assembly.LoadFile(f.FullName);
                }
                catch (Exception)
                {
                    Log.Message("Cannot download dll for team {0}", teamId);
                    return null;
                }
                try
                {
                    AIType = Dll.GetType(AIPath);
                }
                catch (Exception)
                {
                    Log.Message("Cannot get type for AI class team {0}", teamId);
                    return null;
                }
            }
            else
                AIType = typeof(AI);
            
            Object teamAILoadClass = null;

            try
            {
                teamAILoadClass = Activator.CreateInstance(AIType, new object[] { });
            }
            catch (Exception)
            {
                Log.Message("Cannot create instance for AI class team {0}", teamId);
                return null;
            }

            if (teamAILoadClass is AI)
            {
                Log.Message("AI for team {0} loaded", teamId);
                return (teamAILoadClass as AI);
            }
            else
            {
                Log.Message("Not AI class for team {0}", teamId);
                return null;
            }
        }

        //[Command]
        //public void Start()
        //{
        //    if (Game.Instance.GetService<TestService>().Visible == false)
        //    {
        //        Log.Message("Create field firstly");
        //        return;
        //    }
        //
        //    Game.Instance.GetService<TestService>().Enabled = true;
        //}

        //[Command]
        //public void Stop()
        //{
        //    if (Game.Instance.GetService<SubmarinesService>().Visible == false)
        //    {
        //        Log.Message("Create field firstly");
        //        return;
        //    }
        //    Game.Instance.GetService<SubmarinesService>().Enabled = false;
        //}

        EntityCollection GameCollection;
        ActionsQueue queue;
        bool weHaveAWinner;
        
        public LogicService(Game game)
            : base(game)
        {
            cfg = new SubmarinesConfig();
        }

        public void reset()
        {
            weHaveAWinner = false;
            GameCollection = new EntityCollection();
            queue = new ActionsQueue(GameCollection, Game.GetService<GameFieldService>().GameField);
            Team teamR = new Team(0, LoadAI(0), Game.GetService<GameFieldService>().GameField);
            Team teamL = new Team(1, LoadAI(1), Game.GetService<GameFieldService>().GameField);
            GameCollection.addToCollection(teamR);
            GameCollection.addToCollection(teamL);
            teamR.Initialize(GameCollection, submarineR);
            teamL.Initialize(GameCollection, submarineL);
        }

        internal static Texture2D submarineR;
        internal static Texture2D submarineL;
        internal static Texture2D health;
        internal static Texture2D baseC;
        internal static Texture2D mine;
        internal static Texture2D boom;
        internal static Texture2D sunk;
        internal static Texture2D torpedo;
        internal static Texture2D path;
        internal static Texture2D xMark;
        internal static Texture2D aim;
        internal static Texture2D flag;
        internal static Texture2D circle;
        internal static Texture2D check;
        internal static Texture2D winner;

        void LoadContent()
        {
            submarineR = Game.Content.Load<Texture2D>(@"Textures\submarineR");
            submarineL = Game.Content.Load<Texture2D>(@"Textures\submarineL");
            health = Game.Content.Load<Texture2D>(@"Textures\health");
            baseC = Game.Content.Load<Texture2D>(@"Textures\base");
            mine = Game.Content.Load<Texture2D>(@"Textures\mine");
            boom = Game.Content.Load<Texture2D>(@"Textures\boom");
            sunk = Game.Content.Load<Texture2D>(@"Textures\sunk");
            torpedo = Game.Content.Load<Texture2D>(@"Textures\torpedo");
            path = Game.Content.Load<Texture2D>(@"Textures\path");
            xMark = Game.Content.Load<Texture2D>(@"Textures\markerCross");
            aim = Game.Content.Load<Texture2D>(@"Textures\markerTarget");
            flag = Game.Content.Load<Texture2D>(@"Textures\markerFlag");
            circle = Game.Content.Load<Texture2D>(@"Textures\markerDot");
            check = Game.Content.Load<Texture2D>(@"Textures\markerCheck");
            winner = Game.Content.Load<Texture2D>(@"Textures\winner");
        }

        public override void Initialize()
        {
            LoadContent();
            base.Initialize();
            reset();
            Game.Reloading += (s, e) => LoadContent();
        }

        internal static int stepCount = 0;

        void printResults()
        {
            StreamWriter sw;
            if (!File.Exists("Results.txt"))
            {
                sw = new StreamWriter("Results.txt", true);
                sw.WriteLine("#Time\tTurns\tTeam\tTorpedoes\tMines\tAlive\tLives\tTeam\tTorpedoes\tMines\tAlive\tLives");
            }
            else
                sw = new StreamWriter("Results.txt", true);

            String info = "";
            info += DateTime.Now + "\t";
            info += stepCount + "\t";
            foreach (Team team in GameCollection.getTeams())
            {
                int lives = 0;
                foreach (Submarine sub in team.Submarines)
                    lives += sub.Health;
                info += team.AI.GetType().ToString().Split('.')[team.AI.GetType().ToString().Split('.').Length - 1] + "\t";
                info += team.launchedTorpedo + "\t";
                info += team.placedMines + "\t";
                info += team.Submarines.Count + "\t";
                info += lives + "\t";
            }
            sw.WriteLine(info);
            sw.Close();
        }

        bool f = false;

        public override void Update(GameTime gameTime)
        {
            if (stepCount < Config.MaxStepCount)
            {
                GameCollection.Update(gameTime);
                if ((GameCollection.Winner != null) && (!weHaveAWinner))
                {
                    weHaveAWinner = true;
                    printResults();
                }
                if (!weHaveAWinner)
                    if (queue.Size == 0)
                    {
                        stepCount++;
                        Random rnd = new Random();
                        Game.GetService<GameFieldService>().GameField.coolNoise();
                        Game.GetService<GameFieldService>().GameField.calculateNoise();
                        GameCollection.GlobalUpdate();
                        List<Submarine> list = GameCollection.getSubmarines();
                        Submarine sub = list[rnd.Next(list.Count)];
                        ((Team)sub.Parent).AI.NextAction(sub, Game.GetService<GameFieldService>().GameField).execute(sub, queue);
                        foreach (Marker marker in sub.Team.AI.Markers)
                        {
                            marker.Parent = GameCollection;
                            GameCollection.addToCollection(marker);
                        }
                    }
                    else
                        queue.nextAction(gameTime);
                base.Update(gameTime);
            }
            else
            {
                if (!f)
                {
                    printResults();
                    f = true;
                }
            }
        }

        public override void Draw(GameTime gameTime, StereoEye stereoEye)
        {
            SpriteBatch sb = Game.GetService<SpriteBatch>();
            sb.Begin();
            GameCollection.Draw(sb, Game.GetService<DebugStrings>(), stereoEye);
            if (weHaveAWinner)
            {
                float offsetScale = 0;
                if (stereoEye == StereoEye.Left)
                    offsetScale = -Config.offsetScale*1.5f;
                if (stereoEye == StereoEye.Right)
                    offsetScale = Config.offsetScale*1.5f;

                int y = (int)Game.GraphicsDevice.DisplayBounds.Height / 2;
                int x = (int)Game.GraphicsDevice.DisplayBounds.Width / 2;
                sb.Draw(winner, x - 450 - offsetScale, y - 114, 900, 228, GameCollection.Winner.Color);
            }
            sb.End();
            base.Draw(gameTime, stereoEye);
        }
    }
}

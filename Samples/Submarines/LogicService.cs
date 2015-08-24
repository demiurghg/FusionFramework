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
using Fusion.Input;

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

        [UICommand]
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

        EntityCollection GameCollection;
        ActionsQueue queue;
        ReplayManager rm;
        bool weHaveAWinner;
        
        public LogicService(Game game)
            : base(game)
        {
            cfg = new SubmarinesConfig();
        }

        public void rmEnd()
        {
            rm.End();
        }

        public void rmSet()
        {
            rm = new ReplayManager(ail.GetType().ToString(), air.GetType().ToString());
            rm.InitPos(GameCollection);
        }

        AI air;
        AI ail;

        public void reset()
        {
            weHaveAWinner = false;
            CustomMarker.GraphicsDevice = Game.GraphicsDevice;
            GameCollection = new EntityCollection();
            queue = new ActionsQueue(GameCollection, Game.GetService<GameFieldService>().GameField);
            air = LoadAI(0);
            ail = LoadAI(1);
            Team teamL = new Team(0, air, Game.GetService<GameFieldService>().GameField);
            Team teamR = new Team(1, ail, Game.GetService<GameFieldService>().GameField);
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

        double gameTime;
        double pausedTime;

        bool _isPaused = false;
        internal bool IsPaused { get { return _isPaused; } }

        bool _isStepByStep = false;
        bool _nextStep = false;
        internal bool IsStepByStep { get { return _isStepByStep; } }

        void InputDevice_KeyDown(object sender, Fusion.Input.InputDevice.KeyEventArgs e)
        {
            if (e.Key == Keys.P)
            {
                if (!_isPaused)
                    pausedTime = gameTime;
                else
                    queue.delay(gameTime - pausedTime);
                _isPaused = !_isPaused;
            }
            if (e.Key == Keys.Q)
            {
                _isStepByStep = !_isStepByStep;
            }
            if (e.Key == Keys.Space)
            {
                _nextStep = true;
            }
        }

        public override void Initialize()
        {
            LoadContent();
            base.Initialize();
            reset();
            Game.Reloading += (s, e) => LoadContent();
            Game.InputDevice.KeyDown += InputDevice_KeyDown;
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

        bool resultsPrinted = false;
        bool _waitNextStep = true;
        Random rnd = new Random();

        public override void Update(GameTime gameTime)
        {
            this.gameTime = gameTime.Total.TotalSeconds;
            if (stepCount < Config.MaxStepCount)
            {
                if (!_isPaused)
                {
                    GameCollection.Update(gameTime);
                    if ((GameCollection.Winner != null) && (!weHaveAWinner))
                    {
                        weHaveAWinner = true;
                        printResults();
                    }
                    if (!weHaveAWinner)
                    {
                        if (queue.Size == 0)
                        {
                            if (_isStepByStep)
                            {
                                if (_waitNextStep)
                                    _nextStep = false;
                            }
                            else
                                _nextStep = true;

                            _waitNextStep = false;

                            if (_nextStep)
                            {
                                GameCollection.GlobalUpdate();
                                _waitNextStep = true;
                                stepCount++;
                                List<Submarine> list = GameCollection.getSubmarines();
                                Submarine sub = list[rnd.Next(list.Count)];
                                AIAction aiAction = sub.Team.AI.NextAction(sub, Game.GetService<GameFieldService>().GameField);
                                foreach (Marker marker in sub.Team.AI.Markers)
                                    if (marker != null)
                                    {
                                        marker.Parent = GameCollection;
                                        GameCollection.addToCollection(marker);
                                    }
                                if (aiAction != null)
                                {
                                    rm.saveStep(GameCollection, aiAction, sub);
                                    aiAction.execute(sub, queue);
                                }
                            }
                        }
                        else
                        {
                            queue.nextAction(gameTime);
                            if (queue.Size == 0)
                            {
                                Game.GetService<GameFieldService>().GameField.coolNoise();
                                Game.GetService<GameFieldService>().GameField.calculateNoise();
                            }
                        }
                    }
                }
                base.Update(gameTime);
            }
            else
            {
                if (!resultsPrinted)
                {
                    printResults();
                    resultsPrinted = true;
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

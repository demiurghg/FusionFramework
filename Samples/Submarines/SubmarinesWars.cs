using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Mathematics;
using Fusion.Audio;
using Fusion.Content;
using Fusion.Graphics;
using Fusion.Input;
using System.ComponentModel;
using Fusion.Development;
using System.IO;
using SubmarinesWars.SubmarinesGameLibrary;

namespace SubmarinesWars
{
    class SubmarinesWars : Game
    {


        /// <summary>
        /// SubmarinesWars constructor
        /// </summary>
        public SubmarinesWars()
            : base()
        {

            //	enable object tracking :
            Parameters.TrackObjects = true;

            //	enable debug graphics device in Debug :
#if DEBUG
//				Parameters.UseDebugDevice	=	true;
#endif

                Parameters.Width = Config.FIELD_WIDTH * Config.HEX_SIZE + Config.HEX_SIZE / 2;
                Parameters.Height = (int)(Config.FIELD_HEIGHT * Config.HEX_SIZE * (float)Math.Cos(Math.PI / 6)) + (int)(Config.HEX_SIZE * (1 - (float)Math.Cos(Math.PI / 6))) + Config.HEX_SIZE;

                Config.viewportHeight = Parameters.Height;
                Config.viewportWidht = Parameters.Width;

            //	add services :
            AddService(new SpriteBatch(this), false, false, 0, 0);
            AddService(new DebugStrings(this), true, true, 9999, 9999);
            AddService(new DebugRender(this), true, true, 9998, 9998);

            //	add here additional services :
            AddService(new ConfigService(this), true, true, 1, 1);
            AddService(new GameFieldService(this), true, true, 2, 2);
            AddService(new LogicService(this), true, true, 3, 3);
            AddService(new ReplayService(this), false, false, 4, 4);

            //	load configuration for each service :
            LoadConfiguration();

            //	make configuration saved on exit :
            Exiting += FusionGame_Exiting;
        }

        bool replay = false;
        /// <summary>
        /// Add services :
        /// </summary>
        protected override void Initialize()
        {
            if (Environment.GetCommandLineArgs().Length != 1)
            {
                int count = Environment.GetCommandLineArgs().Length;
                //StreamReader sr = new StreamReader(Environment.GetCommandLineArgs()[1]);
                //String s = sr.ReadLine();
                for (int i = 1; i < count; i++) {
                    String s = Environment.GetCommandLineArgs()[i];
                    string[] command = s.Split('=');
                    command[0] = command[0].ToLower().Replace(" ", "");
                    switch (command[0])
                    {
                        case "hexsize":
                            try
                            {
                                GetService<ConfigService>().cfg.HEX_SIZE = Int32.Parse(command[1]);
                            }
                            catch (Exception e)
                            {
                                Log.Message("\"" + s + "\" - ERROR: " + e.Message);
                                Log.Message("Game will be started with defualt value: " + GetService<ConfigService>().cfg.HEX_SIZE);
                            }
                            break;
                        case "submarinesinteam":
                            try
                            {
                                GetService<ConfigService>().cfg.SUBMARINES_IN_TEAM = Int32.Parse(command[1]);
                            }
                            catch (Exception e)
                            {
                                Log.Message("\"" + s + "\" - ERROR: " + e.Message);
                                Log.Message("Game will be started with defualt value: " + GetService<ConfigService>().cfg.SUBMARINES_IN_TEAM);
                            }
                            break;
                        case "torpedocount":
                            try
                            {
                                GetService<ConfigService>().cfg.TORPEDO_COUNT = Int32.Parse(command[1]);
                            }
                            catch (Exception e)
                            {
                                Log.Message("\"" + s + "\" - ERROR: " + e.Message);
                                Log.Message("Game will be started with defualt value: " + GetService<ConfigService>().cfg.TORPEDO_COUNT);
                            }
                            break;
                        case "minescount":
                            try
                            {
                                GetService<ConfigService>().cfg.MINES_COUNT = Int32.Parse(command[1]);
                            }
                            catch (Exception e)
                            {
                                Log.Message("\"" + s + "\" - ERROR: " + e.Message);
                                Log.Message("Game will be started with defualt value: " + GetService<ConfigService>().cfg.MINES_COUNT);
                            }
                            break;
                        case "maxnoise":
                            try
                            {
                                GetService<ConfigService>().cfg.MAX_NOISE = Int32.Parse(command[1]);
                            }
                            catch (Exception e)
                            {
                                Log.Message("\"" + s + "\" - ERROR: " + e.Message);
                                Log.Message("Game will be started with defualt value: " + GetService<ConfigService>().cfg.MAX_NOISE);
                            }
                            break;
                        case "speed":
                            try
                            {
                                GetService<ConfigService>().cfg.SPEED = Int32.Parse(command[1]);
                            }
                            catch (Exception e)
                            {
                                Log.Message("\"" + s + "\" - ERROR: " + e.Message);
                                Log.Message("Game will be started with defualt value: " + GetService<ConfigService>().cfg.SPEED);
                            }
                            break;
                        case "seed":
                            try
                            {
                                GetService<ConfigService>().cfg.SEED = Int32.Parse(command[1]);
                            }
                            catch (Exception e)
                            {
                                Log.Message("\"" + s + "\" - ERROR: " + e.Message);
                                Log.Message("Game will be started with defualt value: " + GetService<ConfigService>().cfg.SEED);
                            }
                            break;
                        case "height":
                            try
                            {
                                GetService<ConfigService>().cfg.FIELD_HEIGHT = Int32.Parse(command[1]);
                            }
                            catch (Exception e)
                            {
                                Log.Message("\"" + s + "\" - ERROR: " + e.Message);
                                Log.Message("Game will be started with defualt value: " + GetService<ConfigService>().cfg.FIELD_HEIGHT);
                            }
                            break;
                        case "width":
                            try
                            {
                                GetService<ConfigService>().cfg.FIELD_WIDTH = Int32.Parse(command[1]);
                            }
                            catch (Exception e)
                            {
                                Log.Message("\"" + s + "\" - ERROR: " + e.Message);
                                Log.Message("Game will be started with defualt value: " + GetService<ConfigService>().cfg.FIELD_WIDTH);
                            }
                            break;
                        case "teamrdll":
                            GetService<LogicService>().cfg.AI_Dll_TeamR = command[1].Replace(" ", "");
                            break;
                        case "teamrai":
                            GetService<LogicService>().cfg.AI_Class_TeamR = command[1].Replace(" ", "");
                            break;
                        case "teamldll":
                            GetService<LogicService>().cfg.AI_Dll_TeamL = command[1].Replace(" ", "");
                            break;
                        case "teamlai":
                            GetService<LogicService>().cfg.AI_Class_TeamL = command[1].Replace(" ", "");
                            break;
                        case "replay":
                            GetService<LogicService>().Enabled = false;
                            GetService<LogicService>().Visible = false;
                            GetService<ReplayService>().Enabled = true;
                            GetService<ReplayService>().Visible = true;
                            GetService<ReplayService>().initReplayManager(command[1]);
                            replay = true;
                            break;
                        case "startfrom":
                            ReplayService.k = Int32.Parse(command[1]);
                            break;
                        default:
                            Log.Message("\"" + s + "\" - ERROR");
                            Exit();
                            break;
                    }
                    //s = sr.ReadLine();
                }
            }

            //this.GetService<ReplayService>().initReplayManager(@"C:\Users\Artem\Documents\GitHub\FusionFramework\FusionSamples\Submarines\bin\x64\Release\Replay\SubmarinesWars.SubmarinesGameLibrary.ArtificialIntelligence.AI\SubmarinesWars.SubmarinesGameLibrary.ArtificialIntelligence.AI\28");
            
            GraphicsDevice_ViewportChanged(null, null);

            //	initialize services :
            base.Initialize();

            //	add keyboard handler :
            InputDevice.KeyDown += InputDevice_KeyDown;

            GraphicsDevice.DisplayBoundsChanged += GraphicsDevice_ViewportChanged;

            if (!replay)
                GetService<LogicService>().rmSet();
        }
        

        private void GraphicsDevice_ViewportChanged(object sender, EventArgs e)
        {
            int width = GraphicsDevice.DisplayBounds.Width;
            int height = GraphicsDevice.DisplayBounds.Height;

            Config.viewportHeight = height;
            Config.viewportWidht = width;

            int fieldSizeWidth = Config.FIELD_WIDTH * Config.HEX_SIZE + Config.HEX_SIZE / 2;
            int fieldSizeHeight = (int)(Config.FIELD_HEIGHT * Config.HEX_SIZE * (float)Math.Cos(Math.PI / 6)) + (int)(Config.HEX_SIZE * (1 - (float)Math.Cos(Math.PI / 6))) + Config.HEX_SIZE;

            int offsetX = width - fieldSizeWidth > 0 ? (width - fieldSizeWidth)/2 : 0;
            int offsetY = height - fieldSizeHeight > 0 ? (height - fieldSizeHeight)/2 : 0;

            Config.OffsetX = offsetX;
            Config.OffsetY = offsetY;
        }



        /// <summary>
        /// Handle keys for each demo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void InputDevice_KeyDown(object sender, Fusion.Input.InputDevice.KeyEventArgs e)
        {
            if (e.Key == Keys.F1)
            {
                //DevCon.Show(this);
            }

            if (e.Key == Keys.F5)
            {
                Reload();
            }

            if (e.Key == Keys.F12)
            {
                GraphicsDevice.Screenshot();
            }

            if (e.Key == Keys.Escape)
            {
                Exit();
            }
        }



        /// <summary>
        /// Save configuration on exit.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void FusionGame_Exiting(object sender, EventArgs e)
        {
            if (!replay)
                GetService<LogicService>().rmEnd();
            SubmarinesGameLibrary.GameEntity.Markers.CustomMarker.Dispose();
            SaveConfiguration();
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Update(GameTime gameTime)
        {
            var ds = GetService<DebugStrings>();

            ds.Add(Color.Orange, "FPS {0}", gameTime.Fps);
            ds.Add("F12  - make screenshot");
            ds.Add("Speed " + Config.SPEED);
            if (replay)
            {
                ds.Add("Step number " + (ReplayService.k - 1));
                ds.Add("Pause " + (GetService<ReplayService>().pause == true ? "on" : "off"));
            }
            else
            {
                ds.Add("Step number " + LogicService.stepCount);
                ds.Add("Pause " + (GetService<LogicService>().IsPaused == true ? "on" : "off"));
                ds.Add("Step by step mode " + (GetService<LogicService>().IsStepByStep == true ? "on" : "off"));
            }
            ds.Add("ESC  - exit");

            base.Update(gameTime);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="stereoEye"></param>
        protected override void Draw(GameTime gameTime, StereoEye stereoEye)
        {
            base.Draw(gameTime, stereoEye);
        }
    }
}

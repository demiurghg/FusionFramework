using Fusion;
using Fusion.Input;
using Fusion.Mathematics;
using SubmarinesWars.SubmarinesGameLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars
{
    public class ConfigService : GameService
    {
        public class GameConfig
        {
            [Description("Min 4\nMax 60")]
            public int HEX_SIZE { get { return Config.HEX_SIZE; } set { if (Game.Instance.GetService<ConfigService>().acceptably) Config.HEX_SIZE = value; } }

            [Description("Min 16\nMax 128")]
            public int FIELD_HEIGHT { get { return Config.FIELD_HEIGHT; } set { if (Game.Instance.GetService<ConfigService>().acceptably) Config.FIELD_HEIGHT = value; } }

            [Description("Min 16\nMax 128")]
            public int FIELD_WIDTH { get { return Config.FIELD_WIDTH; } set { if (Game.Instance.GetService<ConfigService>().acceptably) Config.FIELD_WIDTH = value; } }

            [Description("Min 1")]
            public int SUBMARINES_IN_TEAM { get { return Config.SUBMARINES_IN_TEAM; } set { if (Game.Instance.GetService<ConfigService>().acceptably) Config.SUBMARINES_IN_TEAM = value; } }

            [Description("Min 1")]
            public int TORPEDO_COUNT { get { return Config.TORPEDO_COUNT; } set { if (Game.Instance.GetService<ConfigService>().acceptably) Config.TORPEDO_COUNT = value; } }

            [Description("Min 1")]
            public int MINES_COUNT { get { return Config.MINES_COUNT; } set { if (Game.Instance.GetService<ConfigService>().acceptably) Config.MINES_COUNT = value; } }

            [Description("Min 1")]
            public double MAX_NOISE { get { return Config.MAX_NOISE; } set { if (Game.Instance.GetService<ConfigService>().acceptably) Config.MAX_NOISE = value; } }

            [Description("Min 0.1")]
            public float SPEED { get { return Config.SPEED; } set { if (Game.Instance.GetService<ConfigService>().acceptably) Config.SPEED = value; } }

            [Description("0 - random field")]
            public int SEED { get { return Config.SEED; } set { if (Game.Instance.GetService<ConfigService>().acceptably) Config.SEED = value; } }

            [Description("Min 1")]
            public int MAX_STEP_COUNT { get { return Config.MaxStepCount; } set { if (Game.Instance.GetService<ConfigService>().acceptably) Config.MaxStepCount = value; } }

            public GameConfig()
            {
            }
        }

        [Config]
        public GameConfig cfg { get; set; }

        internal Boolean acceptably = true;

        public ConfigService(Game game)
            : base(game)
        {
            cfg = new GameConfig();
        }

        void InputDevice_KeyDown(object sender, Fusion.Input.InputDevice.KeyEventArgs e)
        {
            if (e.Key == Keys.Up)
            {
                if (Config.SPEED >= 10)
                    Config.SPEED = Config.SPEED + 5;
                else
                    if (Config.SPEED >= 2)
                        Config.SPEED = Config.SPEED + 1;
                    else
                        Config.SPEED = (float)(Math.Round((double)Config.SPEED + 0.1f, 1));
            }
            if (e.Key == Keys.Down)
            {
                if (Config.SPEED > 10)
                    Config.SPEED = Config.SPEED - 5;
                else
                    if (Config.SPEED > 2)
                        Config.SPEED = Config.SPEED - 1;
                    else
                        Config.SPEED = (float)(Math.Round((double)Config.SPEED - 0.1f, 1));
            }
        }

        public override void Initialize()
        {
            acceptably = false;
            base.Initialize();
            Game.InputDevice.KeyDown += InputDevice_KeyDown;
        }
    }
}

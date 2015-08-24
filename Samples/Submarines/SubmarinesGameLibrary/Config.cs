using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary
{
    public class Config
    {
        static int height = 32;
        internal static int FIELD_HEIGHT { get { return height; } set { if ((value >= 16) && (value <= 128)) height = value; } }
        static int width = 64;
        internal static int FIELD_WIDTH { get { return width; } set { if ((value >= 16) && (value <= 128)) width = value; } }

        internal static int OffsetX = 0;
        internal static int OffsetY = 0;

        static int hexSize = 24;
        internal static int HEX_SIZE
        {
            get { return hexSize; }
            set
            {
                if ((value >= 4) && (value <= 60))
                {
                    hexSize = (value / 4) * 4;

                    int width = Config.viewportWidht;
                    int height = Config.viewportHeight;

                    int fieldSizeWidth = Config.FIELD_WIDTH * Config.HEX_SIZE + Config.HEX_SIZE / 2;
                    int fieldSizeHeight = (int)(Config.FIELD_HEIGHT * Config.HEX_SIZE * (float)Math.Cos(Math.PI / 6)) + (int)(Config.HEX_SIZE * (1 - (float)Math.Cos(Math.PI / 6))) + Config.HEX_SIZE;

                    int offsetX = width - fieldSizeWidth > 0 ? (width - fieldSizeWidth) / 2 : 0;
                    int offsetY = height - fieldSizeHeight > 0 ? (height - fieldSizeHeight) / 2 : 0;

                    Config.OffsetX = offsetX;
                    Config.OffsetY = offsetY;
                }
            }
        }

        static int submarinesInTeam = 5;
        internal static int SUBMARINES_IN_TEAM { get { return submarinesInTeam; } set { if ((value >= 1) && (value <= 5)) submarinesInTeam = value; } }

        static int torpedoCount = 15;
        internal static int TORPEDO_COUNT { get { return torpedoCount; } set { if (value >= 1) torpedoCount = value; } }
        static int minesCount = 15;
        internal static int MINES_COUNT { get { return minesCount; } set { if (value >= 1) minesCount = value; } }

        static double maxNoise = 1600;
        public static double MAX_NOISE { get { return maxNoise; } internal set { if (value >= 1) maxNoise = value; } }
        internal static double COEFF_NOISE_DOWN = 0.8;
        internal static double NOISE_3_STEP = MAX_NOISE;
        internal static double NOISE_2_STEP = NOISE_3_STEP * COEFF_NOISE_DOWN;
        internal static double NOISE_1_STEP = NOISE_2_STEP * COEFF_NOISE_DOWN;
        internal static double NOISE_TORPEDO_STEP = MAX_NOISE / 2;
        internal static double NOISE_BOOM_MINE = MAX_NOISE;
        internal static double NOISE_BOOM_TORPEDO = MAX_NOISE;
        internal static double NOISE_ACCIDENT = MAX_NOISE;

        internal static float NOISE_NORM = (float)MAX_NOISE / 240;

        static float speed = 100;
        internal static float SPEED { get { return speed; } set { if ((value >= 0.1) && (value <= 100)) speed = value; } }
        static int maxStepCount = 500;
        internal static int MaxStepCount { get { return maxStepCount; } set { if (value > 0) maxStepCount = value; } }

        static int seed = 50;
        public static int SEED { get { return seed; } internal set { if ((value >= 0) && (value <= Int32.MaxValue)) seed = value; } }



        internal static int viewportHeight = 0;
        internal static int viewportWidht = 0;

        internal static float offsetScale = 2;


        internal static string[] Names = new string[] { "Alfa", "Bravo", "Charlie", "Delta", "Echo", "Foxtrot", "Golf", "Hotel", "India", "Juliett" };
    }
}

using Fusion;
using Fusion.Mathematics;
using Fusion.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Input;
using System.Diagnostics;
using SubmarinesWars.SubmarinesGameLibrary.Field;
using SubmarinesWars.SubmarinesGameLibrary;

namespace SubmarinesWars
{
    class GameFieldService : GameService
    {
        GameField gameField;
        public GameField GameField { get { return gameField; } set { } }

        public GameFieldService(Game game)
            : base(game)
        {
        }

        public void reset()
        {
            gameField = new GameField();
        }

		Texture2D	background;
		Texture2D	cellLand;
		Texture2D	cellDeep;
		Texture2D	cellShallow;
		Texture2D	cellNoise;

		void LoadContent ()
		{
			cellLand	=	Game.Content.Load<Texture2D>(@"Textures\cellLand");
			cellDeep	=	Game.Content.Load<Texture2D>(@"Textures\cellDeep");
			cellShallow	=	Game.Content.Load<Texture2D>(@"Textures\cellShallow");
			cellNoise	=	Game.Content.Load<Texture2D>(@"Textures\cellNoise");
			background	=	Game.Content.Load<Texture2D>(@"Textures\background");
		}

        public override void Initialize()
        {
            reset();
            base.Initialize();
	
			LoadContent();

			Game.Reloading += (s,e) => LoadContent();

            Game.InputDevice.KeyDown += InputDevice_KeyDown;
        }

        bool isNoiseMap = false;

        void InputDevice_KeyDown(object sender, InputDevice.KeyEventArgs e)
        {
            if (e.Key == Keys.S)
                isNoiseMap = !isNoiseMap;
        }

        public override void Update(GameTime gameTime)
        {
            //GameField.coolNoise();
            //GameField.calculateNoise();
            base.Update(gameTime);
        }


		Color ColorLand		=	Color.SandyBrown;
		Color ColorShallow	=	new Color(40,60,100,255);
		Color ColorDeep		=	new Color(13,20, 33,255);

        public override void Draw(GameTime gameTime, StereoEye stereoEye)
        {
			var vp	=	Game.GraphicsDevice.DisplayBounds;

            float offsetScale = 0;
            if (stereoEye == StereoEye.Left)
                offsetScale = -Config.offsetScale;
            if (stereoEye == StereoEye.Right)
                offsetScale = Config.offsetScale;

            var sb = Game.GetService<SpriteBatch>();
            sb.Begin();

            

            sb.Draw(background, new Rectangle(0, 0, vp.Width, vp.Height), Color.White);

            //sb.Restart( BlendState.Additive );

            float dd = 1.4f;
            if (!isNoiseMap)
            {
                for (int i = 0; i < Config.FIELD_HEIGHT; i++)
                {
                    for (int j = 0; j < Config.FIELD_WIDTH; j++)
                    {
                        if (gameField.Field[i, j].Type == CellType.LAND)
                            sb.Draw(cellLand, gameField.Field[i, j].X - dd - offsetScale, gameField.Field[i, j].Y - dd, Config.HEX_SIZE + dd * 2, Config.HEX_SIZE + dd * 2, ColorLand);
                        if (gameField.Field[i, j].Type == CellType.SHALLOW)
                            sb.Draw(cellShallow, gameField.Field[i, j].X - dd, gameField.Field[i, j].Y - dd, Config.HEX_SIZE + dd * 2, Config.HEX_SIZE + dd * 2, ColorShallow);
                        if (gameField.Field[i, j].Type == CellType.DEEP)
                        {
                            //if (stereoEye != StereoEye.Mono)
                            //    offsetScale -= gameField.Field[i, j].Depth * 0.75f;
                            sb.Draw(cellDeep, gameField.Field[i, j].X - dd + offsetScale, gameField.Field[i, j].Y - dd, Config.HEX_SIZE + dd * 2, Config.HEX_SIZE + dd * 2, ColorDeep);
                        }
                    }
                }
            }
            else
            {

                //double noiseMax = -9999990;
                //double noiseMin = 999999;

                //sb.Restart( BlendState.Screen );
                //if (Game.InputDevice.IsKeyDown(Keys.S))
                //{

                    for (int i = 0; i < Config.FIELD_HEIGHT; i++)
                    {
                        for (int j = 0; j < Config.FIELD_WIDTH; j++)
                        {
                            if (gameField.Field[i, j].Type != CellType.LAND)
                            {
                                float depth = 1;
                                if (gameField.Field[i, j].Type == CellType.DEEP)
                                {
                                    depth = 0.9f;
                                }
                                float noise = MathUtil.Clamp((float)gameField.Field[i, j].Noise, 0, (float)Config.MAX_NOISE) / (float)Config.MAX_NOISE;
                                //noise = (float)(Math.Pow(noise, 0.5));
                                //noise = (float)(Math.Round(noise * 16)/16.0f);
                                //var color	=	Hsv2Rgb.HsvToRgb( 240 - noise * 240, 1, 0.5f + noise/2, ((noise < 0.01) ? (byte)0 : (byte)255) );
                                var color = Hsv2Rgb.HsvToRgb(240 - noise * 240, 1, depth * (0.5f + noise / 2), 255);
                                sb.Draw(cellNoise, gameField.Field[i, j].X - dd + offsetScale, gameField.Field[i, j].Y - dd, Config.HEX_SIZE + dd * 2, Config.HEX_SIZE + dd * 2, color);
                            }
                            else
                                sb.Draw(cellNoise, gameField.Field[i, j].X - dd - offsetScale, gameField.Field[i, j].Y - dd, Config.HEX_SIZE + dd * 2, Config.HEX_SIZE + dd * 2, Color.Black);
                        }
                    }
                //}
            }
			/*
            else
            {
                for (int i = 0; i < Config.FIELD_HEIGHT; i++)
                    for (int j = 0; j < Config.FIELD_WIDTH; j++)
                        if (gameField.Field[i, j].Type != CellType.LAND)
                        {
                            float k = Math.Abs((float)gameField.Field[i, j].Noise - (float)Config.MAX_NOISE);
                            k = k / Config.NOISE_NORM;
                            sb.Draw(hex, gameField.Field[i, j].X, gameField.Field[i, j].Y, Config.HEX_SIZE, Config.HEX_SIZE,
                                    Hsv2Rgb.HsvToRgb(k, (float)1, (float)1, (byte)255));
                        }
                        else sb.Draw(hex, gameField.Field[i, j].X, gameField.Field[i, j].Y, Config.HEX_SIZE, Config.HEX_SIZE,
                                    Color.Black);
            } */
            sb.End();
            base.Draw(gameTime, stereoEye);
        }
    }
}

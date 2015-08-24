using Fusion;
using Fusion.Graphics;
using Fusion.Input;
using SubmarinesWars.SubmarinesGameLibrary;
using SubmarinesWars.SubmarinesGameLibrary.GameActions;
using SubmarinesWars.SubmarinesGameLibrary.GameEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars
{
    class ReplayService : GameService
    {
        ReplayManager rm;

        internal void initReplayManager(string pathToReplay)
        {
            rm = new ReplayManager(pathToReplay);
        }

        public ReplayService(Game game)
            : base(game)
        {
        }

        List<ReplayStep> stepList = new List<ReplayStep>();
        ActionsQueue queue;

		void LoadContent ()
		{
		}
        
        public override void Initialize()
        {
            base.Initialize();

			LoadContent();

			Game.Reloading += (s,e) => LoadContent();

            Game.InputDevice.KeyDown += InputDevice_KeyDown;
        }

        bool thirstStep = true;

        void InputDevice_KeyDown(object sender, InputDevice.KeyEventArgs e)
        {
            if (e.Key == Keys.Left)
            {
                if (k > 1) k--;
                thirstStep = false;
                gameCollection = (EntityCollection)stepList.Find(x => x.stepNumb == k).collection.Copy(null);
                queue.clear();
                queue.GameCollection = gameCollection;
                Game.GetService<GameFieldService>().GameField.coolNoise();
                pause = true;
            }
            if (e.Key == Keys.Right)
            {
                if (k < stepList.Count)
                {
                    if (!thirstStep) k++;
                    thirstStep = false;
                    gameCollection = (EntityCollection)stepList.Find(x => x.stepNumb == k).collection.Copy(null);
                    queue.clear();
                    queue.GameCollection = gameCollection;
                    Game.GetService<GameFieldService>().GameField.coolNoise();
                    pause = true;
                }
            }
            if (e.Key == Keys.Space)
            {
                pause = false;
                thirstStep = true;
            }
        }

        bool IsFirst = true;
        EntityCollection gameCollection;
        internal static int k = 1;
        internal bool pause = false;

        public override void Update(GameTime gameTime)
        {
            if (!pause)
            {
                if (IsFirst)
                {
                    gameCollection = (EntityCollection)rm.ReadInitPos(Game.GetService<GameFieldService>().GameField).Copy(null);
                    stepList = rm.ReadGame(Game.GetService<GameFieldService>().GameField);
                    gameCollection = (EntityCollection)stepList.Find(x => x.stepNumb == k).collection.Copy(null);
                    queue = new ActionsQueue(gameCollection, Game.GetService<GameFieldService>().GameField);
                    
                    IsFirst = false;
                }
                else
                {
                    gameCollection.Update(gameTime);
                    if (queue.Size == 0)
                    {
                        if (k <= stepList.Count)
                        {
                            gameCollection.GlobalUpdate();
                            ReplayStep step = stepList.Find(x => x.stepNumb == k);
                            k++;
                            if (step.action != null)
                                step.action.action.execute(gameCollection.getSubmarines().Find(x => x.Number == step.action.subNumb), queue);
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

        public override void Draw(GameTime gameTime, StereoEye stereoEye)
        {
            SpriteBatch sb = Game.GetService<SpriteBatch>();
            sb.Begin();
            gameCollection.Draw(sb, Game.GetService<DebugStrings>(), stereoEye);
            sb.End();
            base.Draw(gameTime, stereoEye);
        }
    }
}

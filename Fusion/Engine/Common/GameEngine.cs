using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Reflection;
using System.Threading.Tasks;
using Fusion.Drivers.Audio;
using System.Globalization;
using System.Threading;
using Fusion.Drivers.Input;
using System.IO;
using System.Diagnostics;
using Fusion.Drivers.Graphics;
using SharpDX.Windows;
using Fusion.Core;
using Fusion.Core.Development;
using Fusion.Core.Content;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Core.Shell;
using Fusion.Core.IniParser;
using Fusion.Engine.Graphics;
using Fusion.Engine.Input;
using Fusion.Engine.Network;
using Lidgren.Network;


namespace Fusion.Engine.Common {

	/// <summary>
	/// Provides basic graphics device initialization, game logic, and rendering code. 
	/// </summary>
	public class GameEngine : DisposableBase {

		/// <summary>
		/// GameEngine instance.
		/// </summary>
		public static GameEngine Instance = null;

		/// <summary>
		/// Gets the current audio device
		/// </summary>
		internal	AudioDevice	AudioDevice { get { return audioDevice; } }

		/// <summary>
		/// Gets the current input device
		/// </summary>
		internal	InputDevice	InputDevice { get { return inputDevice; } }

		/// <summary>
		/// Gets the current graphics device
		/// </summary>
		internal	GraphicsDevice GraphicsDevice { get { return graphicsDevice; } }

		/// <summary>
		/// Gets the current graphics engine
		/// </summary>
		[GameModule("Graphics", "ge")]
		public	GraphicsEngine GraphicsEngine { get { return graphicsEngine; } }

		[GameModule("Network", "net")]
		public NetworkEngine Network { get { return network; } }

		/// <summary>
		/// Gets current content manager
		/// </summary>
		public	ContentManager Content { get { return content; } }

		/// <summary>
		/// Gets keyboard.
		/// </summary>
		[GameModule("Keyboard", "kb")]
		public Keyboard Keyboard { get { return keyboard; } }

		/// <summary>
		/// Gets mouse.
		/// </summary>
		[GameModule("Mouse", "mouse")]
		public Mouse Mouse { get { return mouse; } }

		/// <summary>
		/// Gets gamepads
		/// </summary>
		public GamepadCollection Gamepads { get { return gamepads; } }

		/// <summary>
		/// Gets current content manager
		/// </summary>
		public	Invoker Invoker { get { return invoker; } }

		/// <summary>
		/// Indicates whether the game is initialized.
		/// </summary>
		public	bool IsInitialized { get { return initialized; } }

		/// <summary>
		/// Indicates whether GameEngine.Update and GameEngine.Draw should be called on each frame.
		/// </summary>
		public	bool Enabled { get; set; }

		/// <summary>
		/// Raised when the game exiting before disposing
		/// </summary>
		public event	EventHandler Exiting;

		/// <summary>
		/// Raised after GameEngine.Reload() called.
		/// This event used primarily for developement puprpose.
		/// </summary>
		public event	EventHandler Reloading;


		/// <summary>
		/// Raised when the game gains focus.
		/// </summary>
		public event	EventHandler Activated;

		/// <summary>
		/// Raised when the game loses focus.
		/// </summary>
		public event	EventHandler Deactivated;


		bool	initialized		=	false;
		bool	requestExit		=	false;
		bool	requestReload	=	false;

		AudioDevice			audioDevice		;
		InputDevice			inputDevice		;
		GraphicsDevice		graphicsDevice	;
		//AudioEngine			audioEngine		;
		//InputEngine			inputEngine		;
		GraphicsEngine		graphicsEngine	;
		NetworkEngine		network			;
		ContentManager		content			;
		Invoker				invoker			;
		Keyboard			keyboard		;
		Mouse				mouse			;
		GamepadCollection	gamepads		;

		GameTime	gameTimeInternal;

		GameServer	sv;
		GameClient cl;
		GameInterface gi;


		/// <summary>
		/// Current game server.
		/// </summary>
		[GameModule("Server", "sv")]
		public GameServer GameServer { get { return sv; } set { sv = value; } }
		
		/// <summary>
		/// Current game client.
		/// </summary>
		[GameModule("Client", "cl")]
		public GameClient GameClient { get { return cl; } set { cl = value; } }

		/// <summary>
		/// Current game interface.
		/// </summary>
		[GameModule("Interface", "ui")]
		public GameInterface GameInterface { get { return gi; } set { gi = value; } }



		/// <summary>
		/// 
		/// </summary>
		/// <param name="p"></param>
		/// <param name="sv"></param>
		/// <param name="cl"></param>
		/// <param name="gi"></param>
		public void Run ()
		{
			InitInternal();
			RenderLoop.Run( GraphicsDevice.Display.Window, UpdateInternal );
		}



		public string GetReleaseInfo ()
		{
			return string.Format("{0} {1} {2}", 
				Assembly.GetExecutingAssembly().GetName().Name, 
				Assembly.GetExecutingAssembly().GetName().Version,
				#if DEBUG
					"debug"
				#else
					"release"
				#endif
			);
		}



		/// <summary>
		/// Initializes a new instance of this class, which provides 
		/// basic graphics device initialization, game logic, rendering code, and a game loop.
		/// </summary>
		public GameEngine ()
		{
			Enabled	=	true;

			AppDomain currentDomain = AppDomain.CurrentDomain;
			currentDomain.UnhandledException += currentDomain_UnhandledException;

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault( false );
			CultureInfo.DefaultThreadCurrentCulture	=	CultureInfo.InvariantCulture;
			Thread.CurrentThread.CurrentCulture		=	CultureInfo.InvariantCulture;
			Thread.CurrentThread.CurrentUICulture	=	CultureInfo.InvariantCulture;

			Debug.Assert( Instance == null );

			Instance	=	this;

			Log.Message(GetReleaseInfo());
			Log.Message("Startup directory : {0}", AppDomain.CurrentDomain.BaseDirectory );
			Log.Message("Current directory : {0}", Directory.GetCurrentDirectory() );

			//	For animation rendering applications :
			//	http://msdn.microsoft.com/en-us/library/bb384202.aspx
			GCSettings.LatencyMode	=	GCLatencyMode.SustainedLowLatency;

			audioDevice			=	new AudioDevice( this );
			inputDevice			=	new InputDevice( this );
			graphicsDevice		=	new GraphicsDevice( this );
			graphicsEngine		=	new GraphicsEngine( this );
			network				=	new NetworkEngine( this );
			content				=	new ContentManager( this );
			gameTimeInternal	=	new GameTime();
			invoker				=	new Invoker(this);

			keyboard			=	new Keyboard(this);
			mouse				=	new Mouse(this);
			gamepads			=	new GamepadCollection(this);

		}



		void currentDomain_UnhandledException ( object sender, UnhandledExceptionEventArgs e )
		{
			ExceptionDialog.Show( (Exception) e.ExceptionObject );
		}



		
		/// <summary>
		/// Manage game to raise Reloading event.
		/// </summary>
		public void Reload()
		{
			if (!IsInitialized) {
				throw new InvalidOperationException("GameEngine is not initialized");
			}
			requestReload = true;
		}



		/// <summary>
		/// Request game to exit.
		/// GameEngine will quit when update & draw loop will be completed.
		/// </summary>
		public void Exit ()
		{
			if (!IsInitialized) {
				Log.Warning("GameEngine is not initialized");
				return;
			}
			requestExit	=	true;
		}



		/// <summary>
		/// InitInternal
		/// </summary>
		internal bool InitInternal ()
		{
			Log.Message("");
			Log.Message("---------- GameEngine Initializing ----------");

			var p = new GameParameters();
			p.Width	=	800;
			p.Height =	600;

			GraphicsDevice.Initialize( p );
			InputDevice.Initialize();
			AudioDevice.Initialize();

			GraphicsDevice.FullScreen = false;

			//	init game :
			Log.Message("");

			GameModule.InitializeAll( this );

			initialized	=	true;

			Log.Message("---------------------------------------");
			Log.Message("");

			return true;
		}





		/// <summary>
		/// Overloaded. Immediately releases the unmanaged resources used by this object. 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (!initialized) {
				return;
			}

			Log.Message("");
			Log.Message("---------- GameEngine Shutting Down ----------");

			sv.Wait();

			if (Exiting!=null) {
				Exiting(this, EventArgs.Empty);
			}

			if (disposing) {

				GameModule.DisposeAll( this );

				content.Dispose();

				Log.Message("Disposing : Input Device");
				SafeDispose( ref inputDevice );

				Log.Message("Disposing : Audio Device");
				SafeDispose( ref audioDevice );

				Log.Message("Disposing : Graphics Device");
				SafeDispose( ref graphicsDevice );
			}

			base.Dispose(disposing);

			Log.Message("----------------------------------------");
			Log.Message("");

			ReportActiveComObjects();
		}



		/// <summary>
		/// Print warning message if leaked objectes detected.
		/// Works only if GameParameters.TrackObjects set.
		/// </summary>
		public void ReportActiveComObjects ()
		{
			if (SharpDX.Configuration.EnableObjectTracking) {
				if (SharpDX.Diagnostics.ObjectTracker.FindActiveObjects().Any()) {
					Log.Warning("{0}", SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects() );
				} else {
					Log.Message("Leaked COM objects are not detected.");
				}
				SharpDX.Configuration.EnableObjectTracking = false;
			} else {
				Log.Message("Object tracking disabled.");
			}
		}



		/// <summary>
		/// Returns true if game is active and receive user input
		/// </summary>
		public bool IsActive {
			get {
				return GraphicsDevice.Display.Window.Focused;
			}
		}




		bool isActiveLastFrame = true;


		/// <summary>
		/// 
		/// </summary>
		internal void UpdateInternal ()
		{
			if (IsDisposed) {
				throw new ObjectDisposedException("GameEngine");
			}

			if (!IsInitialized) {
				throw new InvalidOperationException("GameEngine is not initialized");
			}

			bool isActive = IsActive;  // to reduce access to winforms.
			if (isActive!=isActiveLastFrame) {
				isActiveLastFrame = isActive;
				if (isActive) {
					if (Activated!=null) { Activated(this, EventArgs.Empty); } 
				} else {
					if (Deactivated!=null) { Deactivated(this, EventArgs.Empty); } 
				}
			}

			if (Enabled) {

				if (requestReload) {
					if (Reloading!=null) {
						Reloading(this, EventArgs.Empty);
					}
					requestReload = false;
				}

				graphicsDevice.Display.Prepare();

				//	pre update :
				gameTimeInternal.Update();

				InputDevice.UpdateInput();

				UpdateClientServerGame( gameTimeInternal );


				//
				//	Render :
				//
				var eyeList	= graphicsDevice.Display.StereoEyeList;

				foreach ( var eye in eyeList ) {

					GraphicsDevice.ResetStates();

					GraphicsDevice.Display.TargetEye = eye;

					GraphicsDevice.RestoreBackbuffer();

					GraphicsDevice.ClearBackbuffer(Color.Zero);

					this.Draw( gameTimeInternal, eye );

					gameTimeInternal.AddSubframe();
				}

				GraphicsDevice.Present();

				InputDevice.EndUpdateInput();
			}

			try {
				invoker.ExecuteQueue( gameTimeInternal );
			} catch ( Exception e ) {
				Log.Error( e.Message );
			}

			CheckExitInternal();
		}



		/// <summary>
		/// Called when the game determines it is time to draw a frame.
		/// In stereo mode this method will be called twice to render left and right parts of stereo image.
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		protected virtual void Draw ( GameTime gameTime, StereoEye stereoEye )
		{
			GraphicsEngine.Draw( gameTime, stereoEye );

			GraphicsDevice.ResetStates();
			GraphicsDevice.RestoreBackbuffer();
		}
		

		
		/// <summary>
		/// Performs check and does exit
		/// </summary>
		private void CheckExitInternal () 
		{
			if (requestExit) {
				GraphicsDevice.Display.Window.Close();
			}
		}


		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Configuration stuff :
		 * 
		-----------------------------------------------------------------------------------------*/


		/// <summary>
		/// Loads configuration for each subsystem
		/// </summary>
		/// <param name="path"></param>
		public void LoadConfiguration ( string filename )
		{
			Log.Message("Loading configuration...");

			Invoker.FeedConfigs( ConfigSerializer.GetConfigVariables( GameModule.Enumerate(this) ) );

			ConfigSerializer.LoadFromFile( GameModule.Enumerate(this), ConfigSerializer.GetConfigPath(filename) );
		}


		/// <summary>
		/// Saves configuration to XML file	for each subsystem
		/// </summary>
		/// <param name="path"></param>
		public void SaveConfiguration ( string filename )
		{	
			Log.Message("Saving configuration...");

			ConfigSerializer.SaveToFile( GameModule.Enumerate(this), ConfigSerializer.GetConfigPath(filename) );
		}




		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Client-server stuff :
		 * 
		-----------------------------------------------------------------------------------------*/

		[Command("map")]
		public class MapCommand : NoRollbackCommand {

			[CommandLineParser.Required()]
			[CommandLineParser.Name("mapname")]
			public string MapName { get; set; }
			
			public MapCommand ( Invoker invoker ) : base(invoker) 
			{
			}

			public override void Execute ()
			{
				Invoker.GameEngine.StartServer( MapName );
			}
		}


		[Command("killServer")]
		public class KillServerCommand : NoRollbackCommand {
			
			public KillServerCommand ( Invoker invoker ) : base(invoker) 
			{
			}

			public override void Execute ()
			{
				Invoker.GameEngine.KillServer();
			}
		}


		[Command("connect")]
		public class ConnectCommand : NoRollbackCommand {

			[CommandLineParser.Required()]
			[CommandLineParser.Name("host")]
			public string Host { get; set; }

			[CommandLineParser.Required()]
			[CommandLineParser.Name("port")]
			public int Port { get; set; }
			
			public ConnectCommand ( Invoker invoker ) : base(invoker) 
			{
			}

			public override void Execute ()
			{
				Invoker.GameEngine.Connect( Host, Port );
			}
		}


		[Command("disconnect")]
		public class DisconnectCommand : NoRollbackCommand {

			public DisconnectCommand ( Invoker invoker ) : base(invoker) 
			{
			}

			public override void Execute ()
			{
				Invoker.GameEngine.Disconnect();
			}
		}



		
		/// <summary>
		/// Updates game logic and client-server interaction.
		/// </summary>
		/// <param name="gameTime"></param>
		void UpdateClientServerGame ( GameTime gameTime )
		{
			gi.Update( gameTime );
		}



		void StartServer ( string map )
		{
			GameServer.StartInternal( map, null );
		}


		void KillServer ()
		{
			GameServer.KillInternal();
		}



		void Connect ( string host, int port )
		{
			GameClient.ConnectInternal(host, port);
		}


		void Disconnect ()
		{
			GameClient.DisconnectInternal(false);
		}
	}
}

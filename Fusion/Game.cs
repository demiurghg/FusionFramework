using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Reflection;
using Fusion.Audio;
using System.Globalization;
using System.Threading;
using Fusion.Input;
using System.IO;
using System.Diagnostics;
using Fusion.Graphics;
using SharpDX.Windows;
using Fusion.Development;
using Fusion.Content;
using Fusion.Mathematics;
using Fusion.Shell;


namespace Fusion {

	/// <summary>
	/// Provides basic graphics device initialization, game logic, and rendering code. 
	/// </summary>
	public abstract partial class Game : DisposableBase {


		/// <summary>
		/// Game instance.
		/// </summary>
		public static Game Instance = null;

		/// <summary>
		/// Gets the current audio device
		/// </summary>
		public	AudioDevice	AudioDevice { get { return audioDevice; } }

		/// <summary>
		/// Gets the current input device
		/// </summary>
		public	InputDevice	InputDevice { get { return inputDevice; } }

		/// <summary>
		/// Gets the current graphics device
		/// </summary>
		public	GraphicsDevice GraphicsDevice { get { return graphicsDevice; } }

		/// <summary>
		/// Gets current content manager
		/// </summary>
		public	ContentManager Content { get { return content; } }

		/// <summary>
		/// Gets current content manager
		/// </summary>
		public	Invoker Invoker { get { return invoker; } }

		/// <summary>
		/// Indicates whether the game is initialized.
		/// </summary>
		public	bool IsInitialized { get { return initialized; } }

		/// <summary>
		/// Indicates whether Game.Update and Game.Draw should be called on each frame.
		/// </summary>
		public	bool Enabled { get; set; }

		/// <summary>
		/// Gets game parameters
		/// </summary>
		public	GameParameters Parameters { get { return gameParams; } }

		/// <summary>
		/// Raised when the game exiting before disposing
		/// </summary>
		public event	EventHandler Exiting;

		/// <summary>
		/// Raised after Game.Reload() called.
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

		AudioDevice			audioDevice			;
		InputDevice			inputDevice			;
		GraphicsDevice		graphicsDevice		;
		ContentManager		content				;
		Invoker				invoker				;

		List<GameService>	serviceList	=	new List<GameService>();
		GameTime	gameTimeInternal;

		GameParameters	gameParams = new GameParameters();



		/// <summary>
		/// Runs game instace
		/// </summary>
		/// <param name="args"></param>
		public void Run ( string[] args )
		{
			InitInternal();
			RenderLoop.Run( GraphicsDevice.Display.Window, UpdateInternal );
		}



		/// <summary>
		/// Initializes a new instance of this class, which provides 
		/// basic graphics device initialization, game logic, rendering code, and a game loop.
		/// </summary>
		public Game ()
		{
			Enabled	=	true;

			AppDomain currentDomain = AppDomain.CurrentDomain;
			currentDomain.UnhandledException += currentDomain_UnhandledException;

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault( false );
			CultureInfo.DefaultThreadCurrentCulture	=	CultureInfo.InvariantCulture;
			Thread.CurrentThread.CurrentCulture		= CultureInfo.InvariantCulture;
			Thread.CurrentThread.CurrentUICulture	= CultureInfo.InvariantCulture;

			Debug.Assert( Instance == null );

			Instance	=	this;

			Log.Message("{0} {1}", 
				Assembly.GetExecutingAssembly().GetName().Name, 
				Assembly.GetExecutingAssembly().GetName().Version
				);
			Log.Message("Startup directory : {0}", AppDomain.CurrentDomain.BaseDirectory );
			Log.Message("Current directory : {0}", Directory.GetCurrentDirectory() );

			//	For animation rendering applications :
			//	http://msdn.microsoft.com/en-us/library/bb384202.aspx
			GCSettings.LatencyMode	=	GCLatencyMode.SustainedLowLatency;

			audioDevice			=	new AudioDevice( this );
			inputDevice			=	new InputDevice( this );
			graphicsDevice		=	new GraphicsDevice( this );
			content				=	new ContentManager( this );
			gameTimeInternal	=	new GameTime();
			invoker				=	new Invoker(this);
		}







		void currentDomain_UnhandledException ( object sender, UnhandledExceptionEventArgs e )
		{
			ExceptionDialog.Show( (Exception) e.ExceptionObject );
		}



		
		/// <summary>
		/// Manage game to raise Reloading event.
		/// </summary>
		[UICommand("Reload Content", 999998)]
		public void Reload()
		{
			if (!IsInitialized) {
				throw new InvalidOperationException("Game is not initialized");
			}
			requestReload = true;
		}



		/// <summary>
		/// Request game to exit.
		/// Game will quit when update & draw loop will be completed.
		/// </summary>
		[UICommand("Exit", 999999)]
		public void Exit ()
		{
			if (!IsInitialized) {
				Log.Warning("Game is not initialized");
				return;
			}
			requestExit	=	true;
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
			Log.Message("---------- Game Shutting Down ----------");

			if (Exiting!=null) {
				Exiting(this, EventArgs.Empty);
			}

			if (disposing) {
				
				lock ( serviceList ) {
					//	shutdown registered services in reverse order:
					serviceList.Reverse();

					foreach ( var svc in serviceList ) {
						Log.Message("Disposing : {0}", svc.GetType().Name );
						svc.Dispose();
					}
					serviceList.Clear();
				}

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



		/// <summary>
		/// InitInternal
		/// </summary>
		internal bool InitInternal ()
		{
			Log.Message("");
			Log.Message("---------- Game Initializing ----------");

			GraphicsDevice.Initialize( Parameters );
			InputDevice.Initialize();
			AudioDevice.Initialize();

			GraphicsDevice.FullScreen = Parameters.FullScreen;

			//	init game :
			Log.Message("");


			lock ( serviceList ) {
				Initialize();
				initialized = true;
			}


			Log.Message("---------------------------------------");
			Log.Message("");

			return true;
		}




		bool isActiveLastFrame = true;


		/// <summary>
		/// 
		/// </summary>
		internal void UpdateInternal ()
		{
			if (IsDisposed) {
				throw new ObjectDisposedException("Game");
			}

			if (!IsInitialized) {
				throw new InvalidOperationException("Game is not initialized");
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

				//
				//	Update :
				//
				this.Update( gameTimeInternal );

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
		/// Called after the Game and GraphicsDevice are created.
		/// Initializes all registerd services
		/// </summary>
		protected virtual void Initialize ()
		{
			//	init registered services :
			foreach ( var svc in serviceList ) {
				Log.Message("Initializing : {0}", svc.GetType().Name );
				svc.Initialize();
			}
		}



		/// <summary>
		/// Called when the game has determined that game logic needs to be processed.
		/// </summary>
		/// <param name="gameTime"></param>
		protected virtual void Update ( GameTime gameTime )
		{
			GameService[] svcList;

			lock (serviceList) {
				svcList = serviceList.OrderBy( a => a.UpdateOrder ).ToArray();
			}

			foreach ( var svc in svcList ) {
					
				if ( svc.Enabled ) {
					svc.Update( gameTime );
				}
			}
		}



		/// <summary>
		/// Called when the game determines it is time to draw a frame.
		/// In stereo mode this method will be called twice to render left and right parts of stereo image.
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		protected virtual void Draw ( GameTime gameTime, StereoEye stereoEye )
		{
			GameService[] svcList;

			lock (serviceList) {
				svcList = serviceList.OrderBy( a => a.DrawOrder ).ToArray();
			}


			foreach ( var svc in svcList ) {

				if ( svc.Visible ) {
					GraphicsDevice.ResetStates();
					GraphicsDevice.RestoreBackbuffer();
					svc.Draw( gameTime, stereoEye );
				}
			}

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
		 *	Service stuff :
		 * 
		-----------------------------------------------------------------------------------------*/

		/// <summary>
		/// Returns service list
		/// </summary>
		/// <returns></returns>
		public List<GameService> GetServiceList ()
		{
			return serviceList.ToList();
		}


	
		/// <summary>
		/// Adds service. Note, services are initialized and updated in order of addition,
		/// and shutted down in reverse order. Services can'not be removed.
		/// </summary>
		/// <param name="service"></param>
		public void AddService ( GameService service )
		{
			lock (serviceList) {
				if (IsInitialized) {
					service.Initialize();
				}

				serviceList.Add( service );	
			}
		}



		/// <summary>
		/// 
		/// </summary>
		public void RemoveService ( GameService service )
		{
			lock (serviceList) {

				service.Dispose();

				serviceList.Remove( service );	
			}
		}



		/// <summary>
		/// Adds service. Forces IsUpdateable and IsDrawable properties.
		/// </summary>
		/// <param name="service"></param>
		/// <param name="updateable"></param>
		/// <param name="drawable"></param>
		public void AddService ( GameService service, bool enabled, bool visible, int updateOrder, int drawOrder )
		{
			service.Enabled		=	enabled;
			service.Visible		=	visible;
			service.DrawOrder	=	drawOrder;
			service.UpdateOrder	=	updateOrder;

			AddService ( service );
		}



		/// <summary>
		/// Get service of specified type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T GetService<T> () where T : GameService
		{
			lock (serviceList) {
				
				foreach ( var svc in serviceList ) {
					if (svc is T) {
						return (T)svc;
					}
				}

				throw new InvalidOperationException(string.Format("Game service of type \"{0}\" is not added", typeof(T).ToString()));
			}
		}



		/// <summary>
		/// Gets service by name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		internal GameService GetServiceByName( string name )
		{
			lock (serviceList) {

				var obj = serviceList.FirstOrDefault( svc => svc.GetType().Name.ToLower() == name.ToLower() );

				if (obj==null) {
					throw new InvalidOperationException(string.Format("Service '{0}' not found", name) );
				}

				return (GameService)obj;
			}
		}



		/// <summary>
		/// Gets service by name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		internal object GetConfigObjectByServiceName( string name )
		{
			var svc = GetServiceByName( name );
			return GameService.GetConfigObject( svc );
		}



		/// <summary>
		/// Checks wether service of given type exist?
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public bool IsServiceExist<T>() where T : GameService 
		{
			lock (serviceList) {
				
				foreach ( var svc in serviceList ) {
					if (svc is T) {
						return true;
					}
				}

				return false;
			}
		}

		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Configuration stuff :
		 * 
		-----------------------------------------------------------------------------------------*/

		static void SaveToXml ( object obj, Type type, string fileName ) 
		{
			XmlSerializer serializer = new XmlSerializer( type );
			TextWriter textWriter = new StreamWriter( fileName );
			serializer.Serialize( textWriter, obj );
			textWriter.Close();
		}


		static object LoadFromXml( Type type, string fileName )
		{
			XmlSerializer serializer = new XmlSerializer( type );
			TextReader textReader = new StreamReader( fileName );
			object obj = serializer.Deserialize( textReader );
			textReader.Close();
			return obj;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		internal KeyValuePair<string, object>[] GetConfiguration ()
		{
			return serviceList
				.Select( svc => new KeyValuePair<string,object>( svc.GetType().Name, GameService.GetConfigObject( svc ) ) )
				.ToArray();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		string GetConfigPath ( string fileName )
		{
			string myDocs	=	Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			string appName	=	Path.GetFileNameWithoutExtension( AppDomain.CurrentDomain.FriendlyName.Replace(".vshost","") );
			return Path.Combine( myDocs, appName, fileName );
		}



		/// <summary>
		/// Saves configuration to XML file	for each subsystem
		/// </summary>
		/// <param name="path"></param>
		[UICommand("Save Configuration", 1)]
		public void SaveConfiguration ()
		{	
			Log.Message("Saving configuration...");

			foreach ( var svc in serviceList ) {

				string name = GetConfigPath( string.Format("Config.{0}.xml", svc.GetType().Name ) );
				var cfgProp = GameService.GetConfigProperty( svc.GetType() );

				if (cfgProp==null) {
					continue;
				}

				Log.Message("Saving : {0}", name );

				Directory.CreateDirectory( Path.GetDirectoryName(name) );

				try {
					SaveToXml( cfgProp.GetValue(svc), cfgProp.PropertyType, name );
				} catch ( Exception ex ) {
					Log.Error("Failed to save configuration:");
					Log.Error( ex.Message );
				}
			}
		}




		/// <summary>
		/// Loads configuration for each subsystem
		/// </summary>
		/// <param name="path"></param>
		[UICommand("Load Configuration", 1)]
		public void LoadConfiguration ()
		{
			Log.Message("Loading configuration...");

			foreach ( var svc in serviceList ) {

				string name = GetConfigPath( string.Format("Config.{0}.xml", svc.GetType().Name ) );
				var cfgProp = GameService.GetConfigProperty( svc.GetType() );

				if (cfgProp==null) {
					continue;
				}

				Log.Message("Loading : {0}", name );

				Directory.CreateDirectory( Path.GetDirectoryName(name) );

				try {
						
					var cfg = LoadFromXml( cfgProp.PropertyType, name );
					cfgProp.SetValue( svc, cfg );

				} catch ( Exception ex ) {
					Log.Warning("Failed to load configuration:");
					Log.Warning( ex.Message );
				}
			}
		}
	}
}

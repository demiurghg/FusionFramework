using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using System.Threading;

namespace Fusion.Engine.Common {

	public abstract partial class GameServer : GameModule {

		Task serverTask;
		CancellationTokenSource killToken;


		object lockObj = new object();


		/// <summary>
		/// Gets whether server is still alive.
		/// </summary>
		internal bool IsAlive {
			get {
				return serverTask != null; 
			}
		}



		/// <summary>
		/// Initiate server thread.
		/// </summary>
		/// <param name="map"></param>
		/// <param name="postCommand"></param>
		internal void StartInternal ( string map, string postCommand )
		{
			lock (lockObj) {
				if (IsAlive) {
					Log.Warning("Can not start server, it is already running");
					return;
				}

				killToken	=	new CancellationTokenSource();
				serverTask	=	new Task( () => ServerTaskFunc(map, postCommand), killToken.Token );
				serverTask.Start();
			}
		}


		
		/// <summary>
		/// Kills server thread.
		/// </summary>
		/// <param name="wait"></param>
		internal void KillInternal ()
		{
			lock (lockObj) {
				if (!IsAlive) {
					Log.Warning("Server is not running");
				}

				if (killToken!=null) {
					killToken.Cancel();
				}
			}
		}



		/// <summary>
		/// Waits for server thread.
		/// </summary>
		internal void Wait ()
		{
			lock (lockObj) {
				if (killToken!=null) {
					killToken.Cancel();
				}

				if (serverTask!=null) {
					Log.Message("Waiting for server task...");
					serverTask.Wait();
				}
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="map"></param>
		void ServerTaskFunc ( string map, string postCommand )
		{
			NetServer server = null;
			//
			//	configure & start server :
			//
			try {

				var	peerCfg = new NetPeerConfiguration("Server");

				peerCfg.Port				=	GameEngine.Network.Config.Port;
				peerCfg.MaximumConnections	=	GameEngine.Network.Config.MaxClients;

				server	=	new NetServer( peerCfg );
				server.Start();

				//
				//	start game specific stuff :
				//
				Start( map );

				Log.Message("Server started : port = {0}", peerCfg.Port );


				//
				//	invoke post-start command :
				//
				if (postCommand!=null) {
					GameEngine.Invoker.Push( postCommand );
				}


				//
				//	start server loop :
				//
				var svTime = new GameTime();

				//
				//	do stuff :
				//	
				while ( !killToken.IsCancellationRequested ) {

					svTime.Update();

					DispatchIM(server);

					Update( svTime );

				}

			} catch ( Exception e ) {
				Log.Error("Server error: {0}", e.ToString());
				
			} finally {

				//
				//	kill game specific stuff :
				//
				Kill();

				//
				//	shutdown connection :
				//
				if (server!=null) {
					server.Shutdown("shutdown");
				}

				killToken	=	null;
				serverTask	=	null;
			}
		}



		/// <summary>
		/// Dispatches input messages from all the clients.
		/// </summary>
		/// <param name="server"></param>
		void DispatchIM (NetServer server)
		{
			NetIncomingMessage im;
			while ((im = server.ReadMessage()) != null)
			{
				Log.Message("IM!");
				// handle incoming message
				switch (im.MessageType)
				{
					case NetIncomingMessageType.DebugMessage:
					case NetIncomingMessageType.ErrorMessage:
					case NetIncomingMessageType.WarningMessage:
					case NetIncomingMessageType.VerboseDebugMessage:
						string text = im.ReadString();
						Log.Message(text);
						break;

					case NetIncomingMessageType.StatusChanged:
						NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();

						string reason = im.ReadString();
						Log.Message(NetUtility.ToHexString(im.SenderConnection.RemoteUniqueIdentifier) + " " + status + ": " + reason);

						if (status == NetConnectionStatus.Connected)
							Log.Message("Remote hail: " + im.SenderConnection.RemoteHailMessage.ReadString());

						//UpdateConnectionsList();
						break;
					case NetIncomingMessageType.Data:
						// incoming chat message from a client
						string chat = im.ReadString();

						Log.Message("Broadcasting '" + chat + "'");

						// broadcast this to all connections, except sender
						/*List<NetConnection> all = server.Connections; // get copy
						all.Remove(im.SenderConnection);

						if (all.Count > 0)
						{
							NetOutgoingMessage om = s_server.CreateMessage();
							om.Write(NetUtility.ToHexString(im.SenderConnection.RemoteUniqueIdentifier) + " said: " + chat);
							s_server.SendMessage(om, all, NetDeliveryMethod.ReliableOrdered, 0);
						} */
						break;
					default:
						Log.Message("Unhandled type: " + im.MessageType + " " + im.LengthBytes + " bytes " + im.DeliveryMethod + "|" + im.SequenceChannel);
						break;
				}
				server.Recycle(im);
			}
		}
	}
}

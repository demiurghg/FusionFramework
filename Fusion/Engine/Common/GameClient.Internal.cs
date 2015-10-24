using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using System.Threading;


namespace Fusion.Engine.Common {
	public abstract partial class GameClient : GameModule {

		NetClient	client;
		GameTime	clTime;

		Task	clientTask;
		CancellationTokenSource	disconnectToken;


		/*internal bool IsRunning {
			get {
				if (clientTask==null) {
					return false;
				}
				if (clientTask.
			}
		} */

		
		/// <summary>
		/// Connects client.
		/// </summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		internal void ConnectInternal ( string host, int port )
		{		
			clientTask	=	new Task( () => ClientTaskFunc( host, port ) );
			clientTask.Start();
		}



		/// <summary>
		/// Internal disconnect
		/// </summary>
		internal void DisconnectInternal (bool wait)
		{
			if (disconnectToken!=null) {
				disconnectToken.Cancel();
			}

			if (wait) {
				if (clientTask!=null) {
					clientTask.Wait();
				}
			}
		}
		



		/// <summary>
		/// 
		/// </summary>
		void ClientTaskFunc ( string host, int port )
		{
			var	peerCfg = new NetPeerConfiguration("Server");
			peerCfg.AutoFlushSendQueue	=	false;

			client	=	new NetClient(peerCfg);
			client.Start();
			client.Connect( host, port, client.CreateMessage("Hail!") );

			client.RegisterReceivedCallback(new SendOrPostCallback(GotMessage)); 


			Connect( host, port );


			clTime	=	new GameTime();


			while (!disconnectToken.IsCancellationRequested) {

				clTime.Update();

				Update( clTime );

			}

			disconnectToken.Dispose();
			disconnectToken = null;

		}




		public void GotMessage(object peer)
		{
			NetIncomingMessage im;
			while ((im = client.ReadMessage()) != null)
			{
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

						/*if (status == NetConnectionStatus.Connected)
							s_form.EnableInput();
						else
							s_form.DisableInput();

						if (status == NetConnectionStatus.Disconnected)
							s_form.button2.Text = "Connect";*/

						string reason = im.ReadString();
						Log.Message(status.ToString() + ": " + reason);

						break;
					case NetIncomingMessageType.Data:
						string chat = im.ReadString();
						Log.Message(chat);
						break;
					default:
						Log.Message("Unhandled type: " + im.MessageType + " " + im.LengthBytes + " bytes");
						break;
				}
				client.Recycle(im);
			}
		}



	}
}

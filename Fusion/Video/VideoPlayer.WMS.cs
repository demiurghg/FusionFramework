using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Win32;
using SharpDX.MediaFoundation;
using Fusion;
using Fusion.Graphics;

namespace Fusion.Video
{
	public sealed partial class VideoPlayer : IDisposable
	{
		private static MediaSession _session;
		private static AudioStreamVolume _volumeController;
		private static PresentationClock _clock;

		// HACK: Need SharpDX to fix this.
		private static Guid AudioStreamVolumeGuid;

		private static Callback _callback;

		internal bool SetNewVideo = false;

		private class Callback : IAsyncCallback
		{
			private VideoPlayer _player;

			public Callback(VideoPlayer player)
			{
				_player = player;
			}

			public void Dispose()
			{
			}

			public IDisposable Shadow { get; set; }
			public void Invoke(AsyncResult asyncResultRef)
			{
				var ev = _session.EndGetEvent(asyncResultRef);

				// Trigger an "on Video Ended" event here if needed
				if (ev.TypeInfo == MediaEventTypes.SessionTopologyStatus && ev.Get(EventAttributeKeys.TopologyStatus) == TopologyStatus.Ended)
				{
					Console.WriteLine("Video ended");
					if (!_player.SetNewVideo) _player.PlatformPlay();
					else {
						_player.SetNewVideo = false;
					}
				}

				if (ev.TypeInfo == MediaEventTypes.SessionTopologyStatus && ev.Get(EventAttributeKeys.TopologyStatus) == TopologyStatus.Ready)
					_player.OnTopologyReady();

				_session.BeginGetEvent(this, null);
			}

			public AsyncCallbackFlags Flags { get; private set; }
			public WorkQueueId WorkQueueId { get; private set; }
		}

		private void PlatformInitialize()
		{
			// The GUID is specified in a GuidAttribute attached to the class
			AudioStreamVolumeGuid = Guid.Parse(((GuidAttribute)typeof(AudioStreamVolume).GetCustomAttributes(typeof(GuidAttribute), false)[0]).Value);

			MediaAttributes attr = new MediaAttributes(0);
			//MediaManagerState.CheckStartup();
			
			MediaManager.Startup();

			MediaFactory.CreateMediaSession(attr, out _session);
		}

		private Texture2D PlatformGetTexture()
		{
			var sampleGrabber = _currentVideo.SampleGrabber;

			var texData = sampleGrabber.TextureData;

			if (texData == null)
				return null;

			// TODO: This could likely be optimized if we held on to the SharpDX Surface/Texture data,
			// and set it on an XNA one rather than constructing a new one every time this is called.
			//var retTex = new Texture2D(Game.Instance.GraphicsDevice, _currentVideo.Width, _currentVideo.Height, ColorFormat.Bgra8, false);

			_currentVideo.VideoFrame.SetData(texData);

			return _currentVideo.VideoFrame;
		}

		private void PlatformGetState(ref MediaState result)
		{
			if (_clock != null)
			{
				ClockState state;
				_clock.GetState(0, out state);

				switch (state)
				{
					case ClockState.Running:
						result = MediaState.Playing;
						return;

					case ClockState.Paused:
						result = MediaState.Paused;
						return;
				}
			}

			result = MediaState.Stopped;
		}

		private void PlatformPause()
		{
			_session.Pause();
		}

		private void PlatformPlay()
		{
			// Cleanup the last song first.
			if (State != MediaState.Stopped)
			{
				_session.Stop();
				_session.ClearTopologies();
				_session.Close();
				if (_volumeController != null)
				{
					_volumeController.Dispose();
					_volumeController = null;
				}
				_clock.Dispose();
			}

			//create the callback if it hasn't been created yet
			if (_callback == null)
			{
				_callback = new Callback(this);
				_session.BeginGetEvent(_callback, null);
			}

			// Set the new song.
			_session.SetTopology(SessionSetTopologyFlags.Immediate, _currentVideo.Topology);

			// Get the clock.
			_clock = _session.Clock.QueryInterface<PresentationClock>();

			// Start playing.
			var varStart = new Variant();
			_session.Start(null, varStart);
		}

		private void PlatformResume()
		{
			_session.Start(null, null);
		}

		private void PlatformStop()
		{
			_session.ClearTopologies();
			_session.Stop();
			_session.Close();
			if (_volumeController != null)
			{
				_volumeController.Dispose();
				_volumeController = null;
			}
			_clock.Dispose();
			_clock = null;
		}

		private void SetChannelVolumes()
		{
			if (_volumeController != null && !_volumeController.IsDisposed)
			{
				float volume = _volume;
				if (IsMuted)
					volume = 0.0f;

				for (int i = 0; i < _volumeController.ChannelCount; i++)
				{
					_volumeController.SetChannelVolume(i, volume);
				}
			}
		}

		private void PlatformSetVolume()
		{
			if (_volumeController == null)
				return;

			SetChannelVolumes();
		}

		private void PlatformSetIsLooped()
		{
			
			throw new NotImplementedException();
		}

		private void PlatformSetIsMuted()
		{
			if (_volumeController == null)
				return;

			SetChannelVolumes();
		}

		private TimeSpan PlatformGetPlayPosition()
		{
			return TimeSpan.FromTicks(_clock.Time);
		}

		private void PlatformDispose(bool disposing)
		{
		}

		private void OnTopologyReady()
		{
			if (_session.IsDisposed)
				return;

			try {
				// Get the volume interface.
				IntPtr volumeObjectPtr;
				MediaFactory.GetService(_session, MediaServiceKeys.StreamVolume, AudioStreamVolumeGuid, out volumeObjectPtr);
				_volumeController = CppObject.FromPointer<AudioStreamVolume>(volumeObjectPtr);

				SetChannelVolumes();
			}
			catch (Exception e) {
				_volumeController = null;
			}
		}
	}
}

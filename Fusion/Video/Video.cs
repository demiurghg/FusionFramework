using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Video
{
	public enum VideoSoundtrackType
    {
        /// <summary>
        /// This video contains only music.
        /// </summary>
        Music,

        /// <summary>
        /// This video contains only dialog.
        /// </summary>
        Dialog,

        /// <summary>
        /// This video contains music and dialog.
        /// </summary>
        MusicAndDialog,
    }


	/// <summary>
    /// Represents a video.
    /// </summary>
    public sealed partial class Video : IDisposable
	{
		private bool _disposed;

		#region Public API

		/// <summary>
		/// I actually think this is a file PATH...
		/// </summary>
		public string FileName { get; private set; }

		/// <summary>
		/// Gets the duration of the Video.
        /// </summary>
        public TimeSpan Duration { get; internal set; }

        /// <summary>
        /// Gets the frame rate of this video.
        /// </summary>
        public float FramesPerSecond { get; internal set; }

        /// <summary>
        /// Gets the height of this video, in pixels.
        /// </summary>
        public int Height { get; internal set; }

        /// <summary>
        /// Gets the VideoSoundtrackType for this video.
        /// </summary>
        public VideoSoundtrackType VideoSoundtrackType { get; internal set; }

        /// <summary>
        /// Gets the width of this video, in pixels.
        /// </summary>
        public int Width { get; internal set; }

        #endregion

        #region Internal API

        //public Video(string fileName, float durationMs) : this(fileName)
        //{
        //    Duration = TimeSpan.FromMilliseconds(durationMs);
        //}

        public Video(string fileName)
        {
            FileName = fileName;

	        //Width	= width;
	        //Height	= height;

            PlatformInitialize();
        }

        ~Video()
        {
            Dispose(false);
        }

        #endregion

        #region IDisposable Implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                //PlatformDispose(disposing);
                _disposed = true;
            }
        }

        #endregion
    }
}

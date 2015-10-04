using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using SharpDX;
using SharpDX.XAudio2;
using SharpDX.X3DAudio;
using SharpDX.Multimedia;
using Fusion.Core;
using Fusion.Engine.Common;


namespace Fusion.Drivers.Audio {
	public class AudioDevice : DisposableBase {

		readonly Game game;
        internal XAudio2 Device { get; private set; }
        internal MasteringVoice MasterVoice { get; private set; }
        

		/// <summary>
		/// 
		/// </summary>
		internal AudioDevice ( Game game )
		{
			this.game	=	game;
		}



		/// <summary>
		/// 
		/// </summary>
        internal void Initialize()
        {
            try
            {
                if (Device == null) {
                    Device = new XAudio2(XAudio2Flags.None, ProcessorSpecifier.DefaultProcessor);
                    Device.StartEngine();
                }

				var DeviceFormat = Device.GetDeviceDetails(0).OutputFormat;

                // Just use the default device.
                const int deviceId = 0;

                if (MasterVoice == null) {
                    // Let windows autodetect number of channels and sample rate.
                    MasterVoice = new MasteringVoice(Device, XAudio2.DefaultChannels, XAudio2.DefaultSampleRate, deviceId);
                    MasterVoice.SetVolume(_masterVolume, 0);
                }

                // The autodetected value of MasterVoice.ChannelMask corresponds to the speaker layout.
                var deviceDetails = Device.GetDeviceDetails(deviceId);
                Speakers = deviceDetails.OutputFormat.ChannelMask;

				var dev3d = Device3D;

				Log.Debug("Audio devices :");
				for ( int devId = 0; devId < Device.DeviceCount; devId++ ) {
					var device = Device.GetDeviceDetails( devId );

					Log.Debug( "[{1}] {0}", device.DisplayName, devId );
					Log.Debug( "    role : {0}", device.Role );
					Log.Debug( "    id   : {0}", device.DeviceID );
				}
            }
            catch
            {
                // Release the device and null it as
                // we have no audio support.
                if (Device != null)
                {
                    Device.Dispose();
                    Device = null;
                }

                MasterVoice = null;
            }
        }



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
			if (disposing) {
				if (MasterVoice != null) {
					MasterVoice.DestroyVoice();
					MasterVoice.Dispose();
					MasterVoice = null;
				}

				if (Device != null) {
					Device.StopEngine();
					Device.Dispose();
					Device = null;
				}

				_device3DDirty = true;
				_speakers = Speakers.Stereo;
			}
        }



		/*-----------------------------------------------------------------------------------------
		 * 
		 *	3D sound stuff :
		 * 
		-----------------------------------------------------------------------------------------*/

        private float		speedOfSound	= 343.5f;
		private float		_distanceScale	= 1.0f;
		private X3DAudio	_device3D		= null;
        private bool		_device3DDirty	= true;
        private Speakers	_speakers		= Speakers.Stereo;
		private float		_masterVolume	= 1.0f;
        private float		_dopplerScale	= 1.0f;


		/// <summary>
		/// ???
		/// </summary>
        internal Speakers Speakers {
            get {
                return _speakers;
            }

            set	 {
                if (_speakers != value) {
                    _speakers = value;
                    _device3DDirty = true;
                }
            }
        }




		/// <summary>
		/// Device 3D
		/// </summary>
        internal X3DAudio Device3D
        {
            get {
                if (_device3DDirty) {
                    _device3DDirty = false;
                    _device3D = new X3DAudio(_speakers, speedOfSound);
                }//				   */

                return _device3D;
            }
        }




		/// <summary>
		/// Mastering voice value.
		/// </summary>
        public float MasterVolume 
        { 
            get {
                return _masterVolume;
            }
            set {
                if (_masterVolume != value) {
                    _masterVolume = value;
                }
                MasterVoice.SetVolume(_masterVolume, 0);
            }
        }



		/// <summary>
		/// Overall distance scale. Default = 1.
		/// </summary>
        public float DistanceScale {
            get {
                return _distanceScale;
            }
            set	{
                if (value <= 0f){
					throw new ArgumentOutOfRangeException ("value of DistanceScale");
                }
                _distanceScale = value;
            }
        }



		/// <summary>
		/// Overall doppler scale. Default = 1;
		/// </summary>
        public float DopplerScale {
            get {
                return _dopplerScale;
            }
            set	 {
                // As per documenation it does not look like the value can be less than 0
                //   although the documentation does not say it throws an error we will anyway
                //   just so it is like the DistanceScale
                if (value < 0f) {
                    throw new ArgumentOutOfRangeException ("value of DopplerScale");
                }
                _dopplerScale = value;
            }
        }



		/// <summary>
		/// Global speed of sound. Default = 343.5f;
		/// </summary>
        public float SpeedOfSound {
            get {
                return speedOfSound;
            }
            set {
                speedOfSound = value;
		        _device3DDirty = true;
            }
        }
	}

}

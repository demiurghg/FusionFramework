#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License
﻿
using System;
using System.Collections.Generic;
using System.IO;
using SharpDX;
using SharpDX.XAudio2;
using SharpDX.Multimedia;
using SharpDX.X3DAudio;
using Fusion.Core.Mathematics;


namespace Fusion.Drivers.Audio
{
    public sealed class SoundEffect : IDisposable
    {
        private bool isDisposed = false;
		readonly AudioDevice	device;


        internal DataStream		_dataStream;
        internal AudioBuffer	_buffer;
        internal AudioBuffer	_loopedBuffer;
        internal WaveFormat		_format;
        
        // These three fields are used for keeping track of instances created
        // internally when Play is called directly on SoundEffect.
        private List<SoundEffectInstance> _playingInstances;
        private List<SoundEffectInstance> _availableInstances;
        private List<SoundEffectInstance> _toBeRecycledInstances;



		/// <summary>
		/// SoundEffect constructor
		/// </summary>
		/// <param name="buffer">sound data buffer</param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <param name="sampleRate"></param>
		/// <param name="channels"></param>
		/// <param name="loopStart"></param>
		/// <param name="loopLength"></param>
		public SoundEffect( AudioDevice device, byte[] buffer, int offset, int bytesCount, int sampleRate, AudioChannels channels, int loopStart, int loopLength )
        {
			this.device	=	device;
            Initialize(new WaveFormat(sampleRate, (int)channels), buffer, offset, bytesCount, loopStart, loopLength);
        }



		/// <summary>
		/// 
		/// </summary>
		/// <param name="format"></param>
		/// <param name="buffer"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <param name="loopStart"></param>
		/// <param name="loopLength"></param>
        private void Initialize(WaveFormat format, byte[] buffer, int offset, int count, int loopStart, int loopLength)
        {
            _format = format;

            _dataStream = DataStream.Create<byte>(buffer, true, false);

            // Use the loopStart and loopLength also as the range
            // when playing this SoundEffect a single time / unlooped.
            _buffer = new AudioBuffer()
            {
                Stream = _dataStream,
                AudioBytes = count,
                Flags = BufferFlags.EndOfStream,
                PlayBegin = loopStart,
                PlayLength = loopLength,
                Context = new IntPtr(42),
            };

            _loopedBuffer = new AudioBuffer()
            {
                Stream = _dataStream,
                AudioBytes = count,
                Flags = BufferFlags.EndOfStream,
                LoopBegin = loopStart,
                LoopLength = loopLength,
                LoopCount = AudioBuffer.LoopInfinite,
                Context = new IntPtr(42),
            };            
        }



		/// <summary>
		/// 
		/// </summary>
        public bool IsDisposed
        {
            get
            {
                return isDisposed;
            }
        }



		/// <summary>
		/// 
		/// </summary>
        public void Dispose()
        {
            _dataStream.Dispose();
            isDisposed = true;
        }



		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
        public SoundEffectInstance CreateInstance()
        {
            SourceVoice voice = null;
            if (device.Device != null) {
                voice = new SourceVoice(device.Device, _format, VoiceFlags.UseFilter, XAudio2.MaximumFrequencyRatio);
			}

            var instance = new SoundEffectInstance(device, this, voice);
            return instance;
        }



		/// <summary>
		/// 
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
        public static SoundEffect FromStream(Stream stream)
        {            
			throw new NotImplementedException();
        }


	
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
        public bool Play()
        {
            return Play(1.0f, 0.0f, 0.0f);
        }



		/// <summary>
		/// 
		/// </summary>
		/// <param name="volume"></param>
		/// <param name="pitch"></param>
		/// <param name="pan"></param>
		/// <returns></returns>
        public bool Play(float volume, float pitch, float pan)
        {
            if (device.MasterVolume > 0.0f) {
                if (_playingInstances == null) {
                    // Allocate lists first time we need them.
                    _playingInstances = new List<SoundEffectInstance>();
                    _availableInstances = new List<SoundEffectInstance>();
                    _toBeRecycledInstances = new List<SoundEffectInstance>();
                }
                else {
                    // Cleanup instances which have finished playing.                    
                    foreach (var inst in _playingInstances)
                    {
                        if (inst.State == SoundState.Stopped)
                        {
                            _toBeRecycledInstances.Add(inst);
                        }
                    }                    
                }

                // Locate a SoundEffectInstance either one already
                // allocated and not in use or allocate a new one.
                SoundEffectInstance instance = null;
                if (_toBeRecycledInstances.Count > 0) {
                    foreach (var inst in _toBeRecycledInstances)
                    {
                        _availableInstances.Add(inst);
                        _playingInstances.Remove(inst);
                    }
                    _toBeRecycledInstances.Clear();
                }
                if (_availableInstances.Count > 0) {
                    instance = _availableInstances[0];
                    _playingInstances.Add(instance);
                    _availableInstances.Remove(instance);
                }
                else {
                    instance = CreateInstance();
                    _playingInstances.Add(instance);
                }

                instance.Volume = volume;
                instance.Pitch = pitch;
                instance.Pan = pan;
                instance.Play();
            }

            // XNA documentation says this method returns false if the sound limit
            // has been reached. However, there is no limit on PC.
            return true;
        }


		/// <summary>
		/// 
		/// </summary>
        public TimeSpan Duration {
            get {
                var sampleCount = _buffer.PlayLength;
                var avgBPS = _format.AverageBytesPerSecond;
                
                return TimeSpan.FromSeconds((float)sampleCount / (float)avgBPS);
            }
        }



    }
}


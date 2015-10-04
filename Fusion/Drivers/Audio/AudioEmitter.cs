// #region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
// #endregion License
// 
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Fusion;
using Fusion.Core;
using Fusion.Core.Mathematics;

namespace Fusion.Drivers.Audio
{
	public class AudioEmitter
	{
		/// <summary>
		/// 
		/// </summary>
		public AudioEmitter ()
		{
            _dopplerScale = 1.0f;
			Forward = Vector3.ForwardRH;
			Position = Vector3.Zero;
			Up = Vector3.Up;
			Velocity = Vector3.Zero;
			DistanceScale  = 1;
		}

        private float _dopplerScale;
		
		/// <summary>
		/// Doppler scale
		/// </summary>
		public float DopplerScale {
            get	{
                return _dopplerScale;
            }

            set {
                if (value < 0.0f) {
                    throw new ArgumentOutOfRangeException("AudioEmitter.DopplerScale must be greater than or equal to 0.0f");
				}

                _dopplerScale = value;
            }
		}

		/// <summary>
		/// Emitter's forward direction.
		/// </summary>
		public Vector3 Forward {
			get;
			set;
		}

		/// <summary>
		/// Emitter's position.
		/// </summary>
		public Vector3 Position {
			get;
			set;
		}

		/// <summary>
		/// Emitter's up.
		/// </summary>
		public Vector3 Up {
			get;
			set;
		}

		/// <summary>
		/// Absolute emitter velocity.
		/// </summary>
		public Vector3 Velocity {
			get;
			set;
		}

		/// <summary>
		/// Local emitter distance scale
		/// </summary>
		public float DistanceScale {
			get; set;
		}



		private SharpDX.X3DAudio.CurvePoint[] volumeCurve = null;

		/// <summary>
		/// Volume falloff curve. Null value means inverse square law.
		/// </summary>
		public CurvePoint[] VolumeCurve {
			set {
				volumeCurve	=	SharpDXHelper.Convert( value );
			} 
			get {
				return SharpDXHelper.Convert( volumeCurve );
			}
		}



		/// <summary>
		/// Converts to X3DAudio emitter.
		/// </summary>
		/// <returns></returns>
        internal SharpDX.X3DAudio.Emitter ToEmitter()
        {           
            // Pulling out Vector properties for efficiency.
            var pos = this.Position;
            var vel = this.Velocity;
            var fwd = this.Forward;
            var up = this.Up;

            // From MSDN:
            //  X3DAudio uses a left-handed Cartesian coordinate system, 
            //  with values on the x-axis increasing from left to right, on the y-axis from bottom to top, 
            //  and on the z-axis from near to far. 
            //  Azimuths are measured clockwise from a given reference direction. 
            //
            // From MSDN:
            //  The XNA Framework uses a right-handed coordinate system, 
            //  with the positive z-axis pointing toward the observer when the positive x-axis is pointing to the right, 
            //  and the positive y-axis is pointing up. 
            //
            // Programmer Notes:         
            //  According to this description the z-axis (forward vector) is inverted between these two coordinate systems.
            //  Therefore, we need to negate the z component of any position/velocity values, and negate any forward vectors.

            fwd *= -1.0f;
            pos.Z *= -1.0f;
            vel.Z *= -1.0f;

			var emitter =	 new SharpDX.X3DAudio.Emitter();

			emitter.ChannelCount		=	1;
			emitter.Position			=	SharpDXHelper.Convert( pos );
			emitter.Velocity			=	SharpDXHelper.Convert( vel );
			emitter.OrientFront			=	SharpDXHelper.Convert( fwd );
			emitter.OrientTop			=	SharpDXHelper.Convert( up );
			emitter.DopplerScaler		=	DopplerScale;
			emitter.CurveDistanceScaler	=	DistanceScale;
			emitter.VolumeCurve			=	volumeCurve;

            return emitter;
        }


	}
}

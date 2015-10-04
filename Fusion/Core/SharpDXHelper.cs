using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Mathematics.Interop;
using Fusion.Core.Mathematics;

namespace Fusion.Core {
	internal static class SharpDXHelper {
		
		static public RawVector3 Convert( Vector3 v ) 
		{ 
			return new RawVector3{ X = v.X, Y = v.Y, Z = v.Z }; 
		}
		

		static public RawColorBGRA Convert( Color v ) 
		{ 
			return new RawColorBGRA{ R = v.R, G = v.G, B = v.B, A = v.A }; 
		}


		static public RawColor4 Convert( Color4 v ) 
		{ 
			return new RawColor4{ R = v.Red, G = v.Green, B = v.Blue, A = v.Alpha }; 
		}


		static public RawInt4 Convert( Int4 v ) 
		{ 
			return new RawInt4{ X = v.X, Y = v.Y, Z = v.Z, W = v.W }; 
		}


		static public RawViewport Convert( Viewport v ) 
		{ 
			return new RawViewport{ X = v.X, Y = v.Y, Width = v.Width, Height = v.Height, MinDepth = v.MinDepth, MaxDepth = v.MaxDepth }; 
		}


		static public RawViewportF	Convert( ViewportF v ) 
		{ 
			return new RawViewportF { X = v.X, Y = v.Y, Width = v.Width, Height = v.Height, MinDepth = v.MinDepth, MaxDepth = v.MaxDepth }; 
		}


        static public SharpDX.X3DAudio.Listener ToListener(this Fusion.Drivers.Audio.AudioListener listener)
        {
            // Pulling out Vector properties for efficiency.
            var pos = listener.Position;
            var vel = listener.Velocity;
            var forward = listener.Forward;
            var up = listener.Up;

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

            forward *= -1.0f;
            pos.Z *= -1.0f;
            vel.Z *= -1.0f;

            return new SharpDX.X3DAudio.Listener()
            {
                Position	=	Convert( pos ),
                Velocity	=	Convert( vel ),		
                OrientFront =	Convert( forward ),	
                OrientTop	=	Convert( up ),
            };
        }



		/// <summary>
		/// 
		/// </summary>
		/// <param name="curve"></param>
		/// <returns></returns>
		public static SharpDX.X3DAudio.CurvePoint[] Convert ( Drivers.Audio.CurvePoint[] curve )
		{
			if (curve==null) {
				return null;
			}

			return curve
				.Select( c => new SharpDX.X3DAudio.CurvePoint{ Distance = c.Distance, DspSetting = c.DspSetting } )
				.ToArray();
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="curve"></param>
		/// <returns></returns>
		public static Drivers.Audio.CurvePoint[] Convert ( SharpDX.X3DAudio.CurvePoint[] curve )
		{
			if (curve==null) {
				return null;
			}

			return curve
				.Select( c => new Drivers.Audio.CurvePoint{ Distance = c.Distance, DspSetting = c.DspSetting } )
				.ToArray();
		}
	}
}

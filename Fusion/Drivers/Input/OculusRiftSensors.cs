using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;

namespace Fusion.Drivers.Input
{
	internal static class OculusRiftSensors
	{
		public class Eye
		{
			public Vector3		Position	{internal set; get; }
			public Quaternion	Rotation	{internal set; get; }
			public Matrix		Projection	{internal set; get; }
		}

		public static Eye LeftEye	{ internal set; get; }
		public static Eye RightEye	{ internal set; get; }
		
		public static Vector3		HeadPosition { internal set; get; }
		public static Quaternion	HeadRotation { internal set; get; }


		/// <summary>
		/// Convert an ovrMatrix4f to a SharpDX Matrix.
		/// </summary>
		/// <param name="ovrMatrix4f">ovrMatrix4f to convert to a SharpDX Matrix.</param>
		/// <returns>SharpDX Matrix, based on the ovrMatrix4f.</returns>
		#if false
		internal static Matrix ToMatrix(this OculusWrap.OVR.Matrix4f ovrMatrix4f)
		{
			return new Matrix(ovrMatrix4f.M11, ovrMatrix4f.M12, ovrMatrix4f.M13, ovrMatrix4f.M14, ovrMatrix4f.M21, ovrMatrix4f.M22, ovrMatrix4f.M23, ovrMatrix4f.M24, ovrMatrix4f.M31, ovrMatrix4f.M32, ovrMatrix4f.M33, ovrMatrix4f.M34, ovrMatrix4f.M41, ovrMatrix4f.M42, ovrMatrix4f.M43, ovrMatrix4f.M44);
		}
		#endif
	}
}

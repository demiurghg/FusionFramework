using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace Fusion.Drivers.Graphics {
	static class HardwareProfileChecker {

		/// <summary>
		/// Gets the level of feature ?
		/// </summary>
		/// <param name="profile"></param>
		/// <returns></returns>
		public static FeatureLevel GetFeatureLevel ( GraphicsProfile profile )
		{
			if (profile==GraphicsProfile.HiDef) {
				return FeatureLevel.Level_11_0;
			}
			if (profile==GraphicsProfile.Reach) {
				return FeatureLevel.Level_10_0;
			}
			if (profile==GraphicsProfile.Mobile) {
				return FeatureLevel.Level_9_3;
			}

			throw new ArgumentException("profile");
		}


		/// <summary>
		/// Gets the version of shader
		/// </summary>
		/// <param name="profile"></param>
		/// <returns></returns>
		public static string GetShaderVersion( GraphicsProfile profile )
		{
			if (profile==GraphicsProfile.HiDef) {
				return "5_0";
			}
			if (profile==GraphicsProfile.Reach) {
				return "4_0";
			}
			if (profile==GraphicsProfile.Mobile) {
				return "2_0";
			}

			throw new ArgumentException("profile");
		}

	}
}

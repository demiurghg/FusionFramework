using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace Fusion.Graphics {
	static class HardwareProfileChecker {

		/// <summary>
		/// 
		/// </summary>
		/// <param name="profile"></param>
		/// <returns></returns>
		public static FeatureLevel GetFeatureLevel ( HardwareProfile profile )
		{
			if (profile==HardwareProfile.HiDef) {
				return FeatureLevel.Level_11_0;
			}
			if (profile==HardwareProfile.Reach) {
				return FeatureLevel.Level_10_0;
			}

			throw new ArgumentException("profile");
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="profile"></param>
		/// <returns></returns>
		public static string GetShaderVersion( HardwareProfile profile )
		{
			if (profile==HardwareProfile.HiDef) {
				return "5_0";
			}
			if (profile==HardwareProfile.Reach) {
				return "4_0";
			}

			throw new ArgumentException("profile");
		}

	}
}

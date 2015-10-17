using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using SharpDX;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Core.Content;
using Fusion.Engine.Common;


namespace Fusion.Drivers.Graphics {

	/// <summary>
	/// Scene loader
	/// </summary>
	[ContentLoader(typeof(Scene))]
	public class Loader : ContentLoader {


		/// <summary>
		/// Loads scene
		/// </summary>
		/// <param name="game"></param>
		/// <param name="stream"></param>
		/// <param name="requestedType"></param>
		/// <returns></returns>
		public override object Load ( GameEngine game, Stream stream, Type requestedType, string assetPath )
		{																			
			var scene = Scene.Load( stream );

			return scene;
		}
	}
}

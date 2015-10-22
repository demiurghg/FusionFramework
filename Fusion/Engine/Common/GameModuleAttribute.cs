using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Drivers.Graphics;
using Fusion.Core;


namespace Fusion.Engine.Common {

	[AttributeUsage(AttributeTargets.Property)]
	public class GameModuleAttribute : Attribute {

		/// <summary>
		/// Gets nice name of declared service.
		/// </summary>
		public string NiceName { get; private set; }


		/// <summary>
		/// Gets short name of declared service.
		/// </summary>
		public string ShortName { get; private set; }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="niceName"></param>
		/// <param name="shortName"></param>
		public GameModuleAttribute( string niceName, string shortName )
		{
			NiceName	=	niceName;
			ShortName	=	shortName;
		}
	}
}

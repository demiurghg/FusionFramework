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

	public abstract class GameModule : DisposableBase {

		public GameEngine GameEngine { get; protected set; }

		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="game"></param>
		public GameModule ( GameEngine gameEngine )
		{
			GameEngine = gameEngine;
		}


		/// <summary>
		/// Intializes module.
		/// </summary>
		public abstract void Initialize ();



		internal class ModuleBinding {
			public readonly string NiceName;
			public readonly string ShortName;
			public readonly object Object;
			
			public ModuleBinding ( object obj, string niceName, string shortName )
			{
				Object		=	obj;
				NiceName	=	niceName;
				ShortName	=	shortName;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="rootObj"></param>
		/// <returns></returns>
		static void GetAllR ( object rootObj, ref List<ModuleBinding> bindings )
		{
			if (bindings==null) {
				bindings = new List<ModuleBinding>();
			}

			var binds = rootObj.GetType()
						.GetProperties()
						.Where( prop => prop.GetCustomAttribute<GameModuleAttribute>() != null )
						.Select( prop1 => new ModuleBinding(prop1.GetValue( rootObj ), 
							prop1.GetCustomAttribute<GameModuleAttribute>().NiceName, 
							prop1.GetCustomAttribute<GameModuleAttribute>().ShortName ) )
						.ToList();

			bindings.AddRange( binds );

			foreach ( var bind in binds ) {
				GetAllR( bind.Object, ref bindings );
			}
		}


		internal static IEnumerable<ModuleBinding> Enumerate ( object rootObj )
		{
			List<ModuleBinding> bindings = null;
			GetAllR( rootObj, ref bindings );
			return bindings;
		}



		/// <summary>
		/// Calls 'Initialize' method on all services starting from top one tree.
		/// </summary>
		/// <param name="obj"></param>
		static internal void InitializeAll ( object rootObj )
		{
			foreach ( var bind in Enumerate(rootObj) ) {
				Log.Message( "---- Init : {0} ----", bind.NiceName );

				var initialize = bind.Object.GetType().GetMethod("Initialize");

				if (initialize!=null) {
					initialize.Invoke( bind.Object, null );
				}
			}
		}



		static internal void DisposeAll ( object rootObj )
		{
			foreach ( var bind in Enumerate(rootObj).Reverse() ) {
				Log.Message( "---- Dispose : {0} ----", bind.NiceName );

				var initialize = bind.Object.GetType().GetMethod("Dispose");

				if (initialize!=null) {
					initialize.Invoke( bind.Object, null );
				}
			}
		}
	}
}

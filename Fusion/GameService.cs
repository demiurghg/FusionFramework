using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using Fusion;
using Fusion.Mathematics;
using Fusion.Graphics;


namespace Fusion {

	public enum StereoType {
		Horizontal, Vertical
	}


	public class GameService : DisposableBase {

		
		public	TimeSpan	UpdateTime;
		public	TimeSpan	DrawTime;


		public Game Game { get; protected set; }

		/// <summary>
		/// Wether service should be updates
		/// </summary>
		public bool Enabled { get; set; }

		/// <summary>
		/// Wether service should be drawn
		/// </summary>
		public bool Visible { get; set; }
		
		/// <summary>
		/// Update order from minimal value to maximum value.
		/// Negative values means that service will be updated before Game
		/// Positive or zero values means that service will be updates after Game
		/// </summary>
		public int	UpdateOrder { get; set; }

		/// <summary>
		/// Draw order from minimal value to maximum value.
		/// Negative values means that service will be drawn before Game
		/// Positive or zero values means that service will be drawn after Game
		/// </summary>
		public int	DrawOrder { get; set; }
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="game"></param>
		public GameService ( Game game )
		{
			Enabled			=	false;
			Visible			=	false;
			UpdateOrder		=	0;
			DrawOrder		=	0;
			Game = game;
		}


		/// <summary>
		/// 
		/// </summary>
		public virtual void	Initialize () 
		{
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		public virtual void	Update ( GameTime gameTime ) 
		{
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		public virtual void Draw ( GameTime gameTime, StereoEye stereoEye ) 
		{
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
			}

			base.Dispose(disposing);
		}



		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public void RequireService<T>() where T : GameService 
		{
			if ( !Game.IsServiceExist<T>() ) {
				throw new InvalidOperationException( string.Format("Service '{0}' requires service '{1}' to run.", GetType().ToString(), typeof(T).ToString() ) );
			}
		}



		/// <summary>
		/// Gets all configs from 
		/// </summary>
		/// <returns></returns>
		static public KeyValuePair<string, PropertyInfo>[] GetConfigProperties ( Type type )
		{
			string typeName =	type.Name;

			var cfgProps	=	type.GetProperties()
					.Where( prop => prop.GetCustomAttributes(false).Any( propAttr => propAttr is ConfigAttribute ) )
					.Select( prop2 => {
								
						var attr = prop2.GetCustomAttributes(false).FirstOrDefault( a => a is ConfigAttribute ) as ConfigAttribute;
						var name = typeName + ( attr.Unnamed ? "" : "." + attr.Name );
								
						return new KeyValuePair<string, PropertyInfo>( name, prop2 );

					}).ToArray();

			foreach ( var cfgProp in cfgProps ) {
				//Log.LogMessage( "{0} = {1}", cfgProp.Key, cfgProp.Value.Name );
			}

			foreach ( var prop in cfgProps ) {
				if ( cfgProps.Count( p => p.Key == prop.Key ) > 1 ) {
					throw new InvalidOperationException(string.Format("{0} has duplicate config name '{1}'", typeName, prop.Key));
				}
			}

			return cfgProps;
		}

	}
}

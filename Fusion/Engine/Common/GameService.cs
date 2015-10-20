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

	public enum StereoType {
		Horizontal, Vertical
	}


	public class GameService : DisposableBase {

		
		public	TimeSpan	UpdateTime;
		public	TimeSpan	DrawTime;


		public GameEngine GameEngine { get; protected set; }

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
		/// Negative values means that service will be updated before GameEngine
		/// Positive or zero values means that service will be updates after GameEngine
		/// </summary>
		public int	UpdateOrder { get; set; }

		/// <summary>
		/// Draw order from minimal value to maximum value.
		/// Negative values means that service will be drawn before GameEngine
		/// Positive or zero values means that service will be drawn after GameEngine
		/// </summary>
		public int	DrawOrder { get; set; }


		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="game"></param>
		public GameService ( GameEngine game )
		{
			Enabled			=	false;
			Visible			=	false;
			UpdateOrder		=	0;
			DrawOrder		=	0;
			GameEngine = game;
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
			if ( !GameEngine.IsServiceExist<T>() ) {
				throw new InvalidOperationException( string.Format("Service '{0}' requires service '{1}' to run.", GetType().ToString(), typeof(T).ToString() ) );
			}
		}



		/// <summary>
		/// Gets config object/
		/// </summary>
		/// <param name="service"></param>
		/// <returns>Null if no config object.</returns>
		static internal object GetConfigObject ( object service )
		{
			var prop = GetConfigProperty( service.GetType() );

			if (prop==null) {
				return null;
			}

			return prop.GetValue( service );
		}



		/// <summary>
		/// Gets config property info from service of given type.
		/// </summary>
		/// <returns>Null if no config property defined for this type.</returns>
		static internal PropertyInfo GetConfigProperty ( Type type )
		{
			var configProps = type.GetProperties()
				.Where( pi => pi.CustomAttributes.Any( ca => ca.AttributeType == typeof(ConfigAttribute) ) )
				.ToList();

			if (configProps.Count!=1) {
				return null;
			}

			return configProps[0];
		}

	}
}

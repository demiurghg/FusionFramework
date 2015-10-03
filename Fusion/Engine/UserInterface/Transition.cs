using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using SharpDX;
using Fusion;
using Fusion.Core.Mathematics;


namespace Fusion.UserInterface {

	public interface ITransition {
		void	Update	( GameTime gameTime );
		void	Terminate ();
		bool	IsDone { get; }
		bool	IsActive { get; }
		string	TagName { get; set; }
		float	Timer { get; }
	}


	public interface IInterpolator<T> {
		T Lerp( T init, T term, float factor );
	}


	public class ColorInterpolator : IInterpolator<Color> {
		public Color Lerp ( Color init, Color term, float factor )
		{
			return Color.Lerp( init, term, factor );
		}
	}

	
	public class IntInterpolator : IInterpolator<int> {
		public int Lerp ( int init, int term, float factor )
		{
			int val = MathUtil.Lerp( init, term, factor );
			if (init<term) {
				if (val<=init) val = init;
				if (val>=term) val = term;
			} else {
				if (val>=init) val = init;
				if (val<=term) val = term;
			}
			return val;
		}
	}



	public class Transition<T,I> : ITransition where I: IInterpolator<T>, new() {

		readonly object			targetObject;
		readonly PropertyInfo	targetProperty;
		readonly T				targetValue;
		readonly float			period;
		readonly I				interpolator;

		T		originValue;
		float	timer;

		IEnumerable<ITransition>	toCancel;


		public string TagName {
			get; set;
		}


		public bool IsDone {
			get {
				return timer >= period;
			}
		}


		public bool IsActive {
			get {
				return timer >= 0;
			}
		}



		public float Timer {
			get {
				return timer;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="targetObject"></param>
		/// <param name="targetProperty"></param>
		/// <param name="initValue"></param>
		/// <param name="termValue"></param>
		public Transition ( object targetObject, PropertyInfo targetProperty, T targetValue, float delay, float period, IEnumerable<ITransition> toCancel )
		{
			this.targetObject	=	targetObject;
			this.targetProperty	=	targetProperty;
			this.targetValue	=	targetValue;
			this.period			=	period;
			this.interpolator	=	new I();
			this.toCancel		=	toCancel;

			timer	=	-delay;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		public void Update ( GameTime gameTime )
		{
			var delta	=	(float)gameTime.Elapsed.TotalMilliseconds;
			timer		+=	delta;

			//Log.Debug("Timer = {0}; Value = {1}; IsDone = {2}; IsActive = {3}", timer, targetProperty.GetValue( targetObject ), IsDone, IsActive );

			float factor	=	0;

			//	do not affect properties
			//	until time not come
			if ( timer < 0 ) {
				return;
			} 

			//	timer just jumped over :
			if ( timer - delta <= 0 ) {

				originValue	=	(T)targetProperty.GetValue( targetObject );

				if (toCancel!=null && toCancel.Any()) {
					foreach ( var t in toCancel ) {
						if (t.Timer>0 && t!=this) {
							t.Terminate();
						}
					}
				}
			}
			
			//	calc factor :
			if ( timer >= period ) {
				factor	=	1;
			} else {
				factor	=	timer / period;
			}


			factor	=	MathUtil.SmoothStep( factor );


			T newValue	=	interpolator.Lerp( originValue, targetValue, factor );


			targetProperty.SetValue( targetObject, newValue );

			//if (targetProperty.Name=="X") {
			//	Log.Debug("X = {0}; factor = {1}, [{2} - {3}]", newValue, factor, originValue, targetValue );
			//}
		}



		/// <summary>
		/// 
		/// </summary>
		public void Terminate ()
		{
			timer	=	int.MaxValue;
			targetProperty.SetValue( targetObject, targetValue );
		}

		
	}
}

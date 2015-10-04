using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Drivers.Input;
using SharpDX;
using Fusion;
using Fusion.Core.Mathematics;

namespace Fusion.UserInterface {


	public enum AutoImageSize {
		Stretch,
		FitWidth,
	}



	[Flags]
	public enum FrameBahavior {
		AllowDragEvents		=	0x0001,
		PassScrollEvenetsUp	=	0x0002,
	}



	public class DragEventArgs : EventArgs 
	{
		public DragEventArgs ( Point startPoint, Point dragPoint, Keys button, int dx, int dy )
		{
			StartPoint	=	startPoint;
			DragPoint	=	dragPoint;
			Button		=	button;
			Dx			=	dx;
			Dy			=	dy;
		}

		public Point	StartPoint	{ get; protected set; }
		public Point	DragPoint	{ get; protected set; }
		public Keys			Button		{ get; protected set; }
		public int			Dx;
		public int			Dy;
	}
	

	public class TickEventArgs : EventArgs 
	{
		public GameTime	GameTime;
	}


	public class DropEventArgs : EventArgs
	{
		public DropEventArgs ( Point startPoint, Point dragPoint, Keys button )
		{
			StartPoint	=	startPoint;
			DropPoint	=	dragPoint;
			Button		=	button;
		}

		public Point	StartPoint	{ get; protected set; }
		public Point	DropPoint	{ get; protected set; }
		public Keys			Button		{ get; protected set; }
	}


	public class ScrollEventArgs : EventArgs 
	{
		public int SrollAmount { get; set; }
	}


	/// <summary>
	/// Some useful stuff
	/// </summary>
	public static class Utils {

		/// <summary>
		/// Computes size of the auto-sized image
		/// </summary>
		/// <param name="imageSize"></param>
		/// <param name="imageRect"></param>
		/// <param name="windowRect"></param>
		/// <param name="srcRect"></param>
		/// <returns></returns>
		public static Rectangle ComputeImageSize ( AutoImageSize imgSize, Rectangle imgRect, Rectangle wndRect )
		{
			float imgAspect	= ((float)imgRect.Height) / (float)imgRect.Width;
			float wndAspect	= ((float)wndRect.Height) / (float)wndRect.Width;

			if( ( imgSize == AutoImageSize.Stretch ) && ( imgAspect > wndAspect ) ) {
				int h  = wndRect.Height;
				int y  = wndRect.Y;
				int w  = (int)( wndRect.Height / imgAspect );
				int x  = wndRect.X + ( wndRect.Width - w ) / 2;

				return new Rectangle( x, y, w + x, y + h );
			}

			if( ( imgSize == AutoImageSize.FitWidth ) || ( ( imgSize == AutoImageSize.Stretch ) && ( imgAspect <= wndAspect ) ) ) {
				int x = wndRect.X;
				int w = wndRect.Width;
				int h = (int)( imgAspect * wndRect.Width );
				int y = wndRect.Y + ( wndRect.Height - h ) / 2;

				return new Rectangle( x, y, w + x, y + h );
			}
			return wndRect;
		}


		
		public static void ClipRectangle ( Rectangle bounds, ref Rectangle src, ref Rectangle dst )
		{
			
		}
	}


}

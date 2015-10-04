using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using Fusion;
using Fusion.Core.Mathematics;

using Fusion.Drivers.Input;


namespace Fusion.UserInterface {

	class MouseProcessor {

		public readonly Game Game;
		public UserInterface ui;

		Point	oldMousePoint;

		Frame	hoveredFrame;
		Frame	heldFrame		=	null;
		bool	heldFrameLBM	=	false;
		bool	heldFrameRBM	=	false;
		Point	heldPoint;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		public MouseProcessor ( Game game, UserInterface ui )
		{
			this.Game	=	game;
			this.ui		=	ui;
		}



		/// <summary>
		/// 
		/// </summary>
		public void Initialize ()
		{
			Game.InputDevice.KeyDown += InputDevice_KeyDown;
			Game.InputDevice.KeyUp += InputDevice_KeyUp;
			Game.InputDevice.MouseScroll += InputDevice_MouseScroll;

			oldMousePoint	=	Game.InputDevice.MousePosition;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="root"></param>
		public void Update ( Frame root )
		{
			var mousePoint	=	Game.InputDevice.MousePosition;

			var hovered		=	GetHoveredFrame();
			
			//
			//	update mouse move :
			//	
			if ( mousePoint!=oldMousePoint ) {
				
				int dx =	mousePoint.X - oldMousePoint.X;
				int dy =	mousePoint.Y - oldMousePoint.Y;

				if (heldFrame!=null) {
					heldFrame.OnMouseMove( dx, dy );
				} else if ( hovered!=null ) {
					hovered.OnMouseMove( dx, dy );
				}

				oldMousePoint = mousePoint;
			}

			//
			//	Mouse down/up events :
			//
			if (heldFrame==null) {
				var oldHoveredFrame	=	hoveredFrame;
				var newHoveredFrame	=	GetHoveredFrame();

				hoveredFrame		=	newHoveredFrame;

				if (oldHoveredFrame!=newHoveredFrame) {

					CallMouseOut		( oldHoveredFrame );
					CallStatusChanged	( oldHoveredFrame, FrameStatus.None );

					CallMouseIn			( newHoveredFrame );
					CallStatusChanged	( newHoveredFrame, FrameStatus.Hovered );
				}
			}
		}



		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Stuff :
		 * 
		-----------------------------------------------------------------------------------------*/



		/// <summary>
		/// Holds frame
		/// </summary>
		/// <param name="frame"></param>
		/// <param name="key"></param>
		void PushFrame ( Frame currentHovered, Keys key )
		{
			//	frame pushed:
			if (currentHovered!=null) {

				if (heldFrame!=currentHovered) {
					heldPoint = Game.InputDevice.MousePosition;
				}

				//	record pushed frame :
				if (heldFrame==null) {
					heldFrame		=	currentHovered;
				}

				if (key==Keys.LeftButton) {
					heldFrameLBM	=	true;
				}

				if (key==Keys.RightButton) {
					heldFrameRBM	=	true;
				}

				CallMouseDown		( heldFrame, key );
				CallStatusChanged	( heldFrame, FrameStatus.Pushed );
			}
		}


		/// <summary>
		/// Releases frame
		/// </summary>
		/// <param name="frame"></param>
		/// <param name="key"></param>
		void ReleaseFrame ( Frame currentHovered, Keys key )
		{
			//	no frame is held, ignore :
			if (heldFrame==null) {
				return;
			}

			if (key==Keys.LeftButton) {
				heldFrameLBM	=	false;
			}

			if (key==Keys.RightButton) {
				heldFrameRBM	=	false;
			}

			//	call MouseUp :
			CallMouseUp( heldFrame, key );

			//	button are still pressed, no extra action :
			if ( heldFrameLBM || heldFrameRBM ) {
				return;
			}

			//	do stuff :
			hoveredFrame	=	currentHovered;

			if ( currentHovered!=heldFrame ) {
				
				CallMouseOut		( heldFrame );
				CallStatusChanged	( heldFrame, FrameStatus.None );
				CallMouseIn			( currentHovered );
				CallStatusChanged	( currentHovered, FrameStatus.Hovered );

			} else {

				CallStatusChanged	( heldFrame, FrameStatus.Hovered );
				CallClick			( heldFrame );
			}

			heldFrame	=	null;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void InputDevice_KeyDown ( object sender, Drivers.Input.InputDevice.KeyEventArgs e )
		{
			if (e.Key==Keys.LeftButton || e.Key==Keys.RightButton) {
				PushFrame( GetHoveredFrame(), e.Key );
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void InputDevice_KeyUp ( object sender, Drivers.Input.InputDevice.KeyEventArgs e )
		{
			if (e.Key==Keys.LeftButton || e.Key==Keys.RightButton) {
				ReleaseFrame( GetHoveredFrame(), e.Key );
			}
		}



		void InputDevice_MouseScroll ( object sender, InputDevice.MouseScrollEventArgs e )
		{
			var hovered = GetHoveredFrame();
			if ( hovered!=null ) {
				hovered.OnMouseWheel( e.WheelDelta );
			}
		}

		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Callers :
		 * 
		-----------------------------------------------------------------------------------------*/

		void CallClick ( Frame frame )
		{
			if (frame!=null && frame.CanAcceptControl) {
				frame.OnClick();
			}
		}

		void CallMouseDown ( Frame frame, Keys key ) 
		{
			if (frame!=null && frame.CanAcceptControl) {
				frame.OnMouseDown( key );
			}
		}

		void CallMouseUp ( Frame frame, Keys key ) 
		{
			if (frame!=null) {
				frame.OnMouseUp( key );
			}
		}

		void CallMouseIn ( Frame frame ) 
		{
			if (frame!=null && frame.CanAcceptControl) {
				frame.OnMouseIn();
			}
		}

		void CallMouseOut ( Frame frame ) 
		{
			if (frame!=null) {
				frame.OnMouseOut();
			}
		}

		void CallStatusChanged ( Frame frame, FrameStatus status )
		{
			if (frame!=null) {
				frame.ForEachAncestor( f => f.OnStatusChanged( status ) );
			}
		}


		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Stuff :
		 * 
		-----------------------------------------------------------------------------------------*/

		/// <summary>
		/// 
		/// </summary>
		/// <param name="root"></param>
		/// <returns></returns>
		Frame GetHoveredFrame ()
		{
			Frame mouseHoverFrame = null;

			UpdateHoverRecursive( ui.RootFrame, Game.InputDevice.MousePosition, ref mouseHoverFrame );

			return mouseHoverFrame;
		}



		/// <summary>
		/// Updates current hovered frame
		/// </summary>
		/// <param name="frame"></param>
		/// <param name="viewCtxt"></param>
		void UpdateHoverRecursive ( Frame frame, Point p, ref Frame mouseHoverFrame )
		{
			if (frame==null) {
				return;
			}

			var absLeft		=	frame.GlobalRectangle.Left;
			var absTop		=	frame.GlobalRectangle.Top;
			var absRight	=	frame.GlobalRectangle.Right;
			var absBottom	=	frame.GlobalRectangle.Bottom;

			if (!frame.CanAcceptControl) {
				return;
			}
			
			bool hovered	=	p.X >= absLeft 
							&&	p.X <  absRight 
							&&	p.Y >= absTop
							&&	p.Y <  absBottom;

			if (hovered) {
				mouseHoverFrame = frame;
				foreach (var child in frame.Children) {
					UpdateHoverRecursive( child, p, ref mouseHoverFrame );
				}
			}

		}
	}
}

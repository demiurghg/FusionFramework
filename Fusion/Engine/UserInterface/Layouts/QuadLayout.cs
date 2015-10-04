using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Input;
using Fusion.Drivers.Graphics;



namespace Fusion.UserInterface.Layouts {
	public class QuadLayout : LayoutEngine {

		public	QuadLayoutStyle	LayoutStyle			{ get; set; }
		public	bool			UseTransitions		{ get; set; }

		public	int				TransitionDelay		{ get; set; }
		public	int				TransitionPeriod	{ get; set; }
		
		int gapH	=	0;		
		int gapW	=	0;		

		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="style"></param>
		/// <param name="verticalGap"></param>
		/// <param name="horizontalGap"></param>
		public QuadLayout ( QuadLayoutStyle style, int verticalInterval, int horizontalInterval )
		{
			LayoutStyle			=	style;

			this.gapH	=	verticalInterval;
			this.gapW	=	horizontalInterval;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="targetFrame"></param>
		public override void RunLayout ( Frame targetFrame, bool forceTransitions = false )
		{
			var gp	=	targetFrame.GetPaddedRectangle();
			int w1	=	( gp.Width	- gapW ) / 2;
			int h1	=	( gp.Height	- gapH ) / 2;
			int w2	=	w1 + ( MathUtil.IsOdd( gp.Width - gapW ) ? 1 : 0 );
			int h2	=	h1 + ( MathUtil.IsOdd( gp.Height - gapH )  ? 1 : 0 );

			bool	useTransitions	=	!forceTransitions & UseTransitions;

			//targetFrame.ForEachChildren( f => f.Visible = false );
			
			if ( LayoutStyle==QuadLayoutStyle.SinglePanel ) {
				MoveAndResizeLogical( targetFrame, useTransitions, 0,  0, 0,  2, 2 );
				HideFrame( targetFrame, useTransitions, 1 );
				HideFrame( targetFrame, useTransitions, 2 );
				HideFrame( targetFrame, useTransitions, 3 );
				return;
			}
			
			if ( LayoutStyle==QuadLayoutStyle.TwoPanelsSideBySide ) {
				MoveAndResizeLogical( targetFrame, useTransitions, 0,  0, 0,  1, 2 );
				MoveAndResizeLogical( targetFrame, useTransitions, 1,  1, 0,  1, 2 );
				HideFrame( targetFrame, useTransitions, 2 );
				HideFrame( targetFrame, useTransitions, 3 );
				return;
			}
			
			if ( LayoutStyle==QuadLayoutStyle.TwoPanelsStacked ) {
				MoveAndResizeLogical( targetFrame, useTransitions, 0,  0, 0,  2, 1 );
				MoveAndResizeLogical( targetFrame, useTransitions, 1,  0, 1,  2, 1 );
				HideFrame( targetFrame, useTransitions, 2 );
				HideFrame( targetFrame, useTransitions, 3 );
				return;
			}
			
			if ( LayoutStyle==QuadLayoutStyle.ThreePanelsSplitLeft ) {
				MoveAndResizeLogical( targetFrame, useTransitions, 0,  0, 0,  1, 1 );
				MoveAndResizeLogical( targetFrame, useTransitions, 1,  0, 1,  1, 1 );
				MoveAndResizeLogical( targetFrame, useTransitions, 2,  1, 0,  1, 2 );
				HideFrame( targetFrame, useTransitions, 3 );
				return;
			}
			
			if ( LayoutStyle==QuadLayoutStyle.ThreePanelsSplitRight ) {
				MoveAndResizeLogical( targetFrame, useTransitions, 0,  0, 0,  1, 2 );
				MoveAndResizeLogical( targetFrame, useTransitions, 1,  1, 0,  1, 1 );
				MoveAndResizeLogical( targetFrame, useTransitions, 2,  1, 1,  1, 1 );
				HideFrame( targetFrame, useTransitions, 3 );
				return;
			}
			
			if ( LayoutStyle==QuadLayoutStyle.ThreePanelsSplitTop ) {
				MoveAndResizeLogical( targetFrame, useTransitions, 0,  0, 0,  1, 1 );
				MoveAndResizeLogical( targetFrame, useTransitions, 1,  1, 0,  1, 1 );
				MoveAndResizeLogical( targetFrame, useTransitions, 2,  0, 1,  2, 1 );
				HideFrame( targetFrame, useTransitions, 3 );
				return;
			}
			
			if ( LayoutStyle==QuadLayoutStyle.ThreePanelsSplitBottom ) {
				MoveAndResizeLogical( targetFrame, useTransitions, 0,  0, 0,  2, 1 );
				MoveAndResizeLogical( targetFrame, useTransitions, 1,  0, 1,  1, 1 );
				MoveAndResizeLogical( targetFrame, useTransitions, 2,  1, 1,  1, 1 );
				HideFrame( targetFrame, useTransitions, 3 );
				return;
			}
			
			if ( LayoutStyle==QuadLayoutStyle.FourPanels ) {
				MoveAndResizeLogical( targetFrame, useTransitions, 0,  0, 0,  1, 1 );
				MoveAndResizeLogical( targetFrame, useTransitions, 1,  0, 1,  1, 1 );
				MoveAndResizeLogical( targetFrame, useTransitions, 2,  1, 0,  1, 1 );
				MoveAndResizeLogical( targetFrame, useTransitions, 3,  1, 1,  1, 1 );
				return;
			}

		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="targetFrame"></param>
		/// <param name="childIndex"></param>
		/// <param name="logicalX"></param>
		/// <param name="logicalY"></param>
		/// <param name="logicalWidth"></param>
		/// <param name="logicalHeight"></param>
		void MoveAndResizeLogical ( Frame targetFrame, bool useTransitions, int childIndex, int logicalX, int logicalY, int logicalWidth, int logicalHeight )
		{
			int offsetX = 0;
			int offsetY = 0;

			if ( LayoutStyle==QuadLayoutStyle.ThreePanelsSplitRight) {
				offsetX = (int)((targetFrame.Width - gapW)/2 * 0.2f);
				offsetY = (int)((targetFrame.Width - gapW)/2 * 0.1f);
			}
			if (logicalWidth==2) offsetX = 0;
			if (logicalHeight==2) offsetY = 0;

			var gp	=	targetFrame.GetPaddedRectangle(false);
			int w1	=	( gp.Width	- gapW ) / 2;
			int h1	=	( gp.Height	- gapH ) / 2;

			int x	=	gp.X + logicalX * ( w1 + gapW ) + ( offsetX * ((logicalX==0) ? 0 : 1) );
			int y	=	gp.Y + logicalY * ( h1 + gapH ) + ( offsetY * ((logicalY==0) ? 0 : 1) );
			int w	=	w1 * logicalWidth  + gapW * (logicalWidth  - 1) + ( offsetX * ((logicalX==0) ? 1 : -1) );
			int h	=	h1 * logicalHeight + gapH * (logicalHeight - 1) + ( offsetY * ((logicalY==0) ? 1 : -1) );

			MoveAndResize( targetFrame, useTransitions, childIndex, x, y, w, h );
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="targetFrame"></param>
		/// <param name="id"></param>
		void MoveAndResize ( Frame targetFrame, bool useTransitions, int childIndex, int x, int y, int w, int h )
		{
			var child = targetFrame.Children.ElementAtOrDefault( childIndex );

			if (child==null) {
				return;
			}

			child.Visible	=	true;

			if (useTransitions) {
				child.RunTransition("OverallColor", Color.White, 	(childIndex+1) * TransitionDelay, TransitionPeriod );
				child.RunTransition("X",			x,				(childIndex+1) * TransitionDelay, TransitionPeriod );
				child.RunTransition("Y",			y,				(childIndex+1) * TransitionDelay, TransitionPeriod );
				child.RunTransition("Width",		w,				(childIndex+1) * TransitionDelay, TransitionPeriod );
				child.RunTransition("Height",		h,				(childIndex+1) * TransitionDelay, TransitionPeriod );
			} else {
				child.OverallColor	=	Color.White;
				child.X			=	x;
				child.Y			=	y;
				child.Width		=	w;
				child.Height	=	h;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="targetFrame"></param>
		/// <param name="useTransitions"></param>
		/// <param name="childIndex"></param>
		void HideFrame ( Frame targetFrame, bool useTransitions, int childIndex )
		{
			var child = targetFrame.Children.ElementAtOrDefault( childIndex );

			if (child==null) {
				return;
			}

			var color =	new Color(255,255,255,0);

			var gp	=	targetFrame.GetPaddedRectangle(false);
			int w	=	( gp.Width	- gapW ) / 2;
			int h	=	( gp.Height	- gapH ) / 2;
			int x	=	w / 2;
			int y	=	h / 2;

			if (useTransitions) {
				child.RunTransition("OverallColor", color,		0*(childIndex+1) * TransitionDelay, TransitionPeriod );
				//child.RunTransition("X",			x,			0*(childIndex+1) * TransitionDelay, TransitionPeriod );
				//child.RunTransition("Y",			y,			0*(childIndex+1) * TransitionDelay, TransitionPeriod );
				//child.RunTransition("Width",		w,			0*(childIndex+1) * TransitionDelay, TransitionPeriod );
				//child.RunTransition("Height",		h,			0*(childIndex+1) * TransitionDelay, TransitionPeriod );
			} else {
				child.OverallColor	=	color;
				//child.X			=	x;
				//child.Y			=	y;
				//child.Width		=	w;
				//child.Height	=	h;
			}
		}

	}
}

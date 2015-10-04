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
using Forms = System.Windows.Forms;


namespace Fusion.UserInterface {

	public partial class Frame {

		public readonly	Game	Game;
		readonly UserInterface	ui;

		/// <summary>
		/// 
		/// </summary>
		public	string		Name				{ get; set; }

		/// <summary>
		/// Is frame visible. Default true.
		/// </summary>
		public	bool		Visible				{ get; set; }

		/// <summary>
		/// 
		/// </summary>
		internal bool		CanAcceptControl	{ get { return Visible && OverallColor.A != 0 && !Ghost; } }

		/// <summary>
		/// 
		/// </summary>
		internal bool		IsDrawable			{ get { return Visible && OverallColor.A != 0; } }

		/// <summary>
		/// Frame visible but does not receive input 
		/// </summary>
		public	bool		Ghost				{ get; set; }

		/// <summary>
		/// Is frame receive input. Default true.
		/// </summary>
		public	bool		Enabled				{ get; set; }

		/// <summary>
		/// Should frame fit its size to content. Default false.
		/// </summary>
		public	bool		AutoSize			{ get; set; }

		/// <summary>
		/// Text font
		/// </summary>
		public	SpriteFont	Font				{ get; set; }

		/// <summary>
		/// Tag object
		/// </summary>
		public	object		Tag;

		/// <summary>
		/// 
		/// </summary>
		public	ClippingMode	 ClippingMode	{ get; set; }

		/// <summary>
		/// Overall color that used as multiplier 
		/// for all children elements
		/// </summary>
		public	Color		OverallColor		{ get; set; }

		/// <summary>
		/// Background color
		/// </summary>
		public	Color		BackColor			{ get; set; }

		/// <summary>
		/// Background color
		/// </summary>
		public	Color		BorderColor			{ get; set; }

		/// <summary>
		/// Foreground (e.g. text) color
		/// </summary>
		public	Color		ForeColor			{ get; set; }

		/// <summary>
		/// Text shadow color
		/// </summary>
		public	Color		ShadowColor			{ get; set; }

		/// <summary>
		/// Shadow offset
		/// </summary>
		public	Vector2		ShadowOffset		{ get; set; }


		/// <summary>
		/// Local X position of the frame
		/// </summary>
		public	int			X					{ get; set; }

		/// <summary>
		/// Local Y position of the frame
		/// </summary>
		public	int			Y					{ get; set; }

		/// <summary>
		///	Width of the frame
		/// </summary>
		public	int			Width				{ get; set; }

		/// <summary>
		///	Height of the frame
		/// </summary>
		public	int			Height				{ get; set; }

		/// <summary>
		/// Left gap between frame and its content
		/// </summary>
		public	int			PaddingLeft			{ get; set; }

		/// <summary>
		/// Right gap between frame and its content
		/// </summary>
		public	int			PaddingRight		{ get; set; }

		/// <summary>
		/// Top gap  between frame and its content
		/// </summary>
		public	int			PaddingTop			{ get; set; }

		/// <summary>
		/// Bottom gap  between frame and its content
		/// </summary>
		public	int			PaddingBottom		{ get; set; }

		/// <summary>
		/// Top and bottom padding
		/// </summary>
		public	int			VPadding			{ set { PaddingBottom = PaddingTop = value; } }

		/// <summary>
		///	Left and right padding
		/// </summary>
		public	int			HPadding			{ set { PaddingLeft = PaddingRight = value; } }

		/// <summary>
		/// Top, bottom, left and right padding
		/// </summary>
		public	int			Padding				{ set { VPadding = HPadding = value; } }

		/// <summary>
		/// 
		/// </summary>
		public	int			BorderTop			{ get; set; }

		/// <summary>
		/// 
		/// </summary>
		public	int			BorderBottom		{ get; set; }

		/// <summary>
		/// 
		/// </summary>
		public	int			BorderLeft			{ get; set; }

		/// <summary>
		/// 
		/// </summary>
		public	int			BorderRight			{ get; set; }

		/// <summary>
		/// 
		/// </summary>
		public	int			Border				{ set { BorderTop = BorderBottom = BorderLeft = BorderRight = value; } }

		/// <summary>
		/// 
		/// </summary>
		public	string		Text				{ get; set; }

		/// <summary>
		/// 
		/// </summary>
		public	Alignment	TextAlignment		{ get; set; }

		/// <summary>
		/// 
		/// </summary>
		public	int			TextOffsetX			{ get; set; }

		/// <summary>
		/// 
		/// </summary>
		public	int			TextOffsetY			{ get; set; }

		/// <summary>
		/// 
		/// </summary>
		public	TextEffect	TextEffect			{ get; set; }

		/// <summary>
		/// 
		/// </summary>
		public	FrameAnchor	Anchor			{ get; set; }


		public int				ImageOffsetX	{ get; set; }
		public int				ImageOffsetY	{ get; set; }
		public FrameImageMode	ImageMode		{ get; set; }
		public Color			ImageColor		{ get; set; }
		public ShaderResource	Image			{ get; set; }

		/// <summary>
		/// 
		/// </summary>
		public LayoutEngine	Layout	{ 
			get { return layout; }
			set { layout = value; if (LayoutChanged!=null) LayoutChanged(this, EventArgs.Empty); }
		}

		LayoutEngine layout = null;


		#region	Events
		public class KeyEventArgs : EventArgs {
			public Keys	Key;
		}

		public class MouseEventArgs : EventArgs {
			public Keys Key = Keys.None;
			public int X = 0;
			public int Y = 0;
			public int DX = 0;
			public int DY = 0;
			public int Wheel = 0;
		}

		public class StatusEventArgs : EventArgs {
			public FrameStatus	Status;
		}

		public class MoveEventArgs : EventArgs {
			public int	X;
			public int	Y;
		}

		public class ResizeEventArgs : EventArgs {
			public int	Width;
			public int	Height;
		}

		public class DrawEventArgs : EventArgs {
			public GameTime GameTime;
			public StereoEye StereoEye;
			public SpriteBatch SpriteBatch = null;
		}

		public event EventHandler	Tick;
		public event EventHandler	LayoutChanged;
		public event EventHandler<MouseEventArgs>	MouseIn;
		public event EventHandler<MouseEventArgs>	MouseMove;
		public event EventHandler<MouseEventArgs>	MouseOut;
		public event EventHandler<MouseEventArgs>	MouseWheel;
		public event EventHandler<MouseEventArgs>	Click;
		public event EventHandler<MouseEventArgs>	MouseDown;
		public event EventHandler<MouseEventArgs>	MouseUp;
		public event EventHandler<StatusEventArgs>	StatusChanged;
		public event EventHandler<MoveEventArgs>	Move;
		public event EventHandler<ResizeEventArgs>	Resize;
		public event EventHandler<DrawEventArgs>	GameDraw;
		public event EventHandler<DrawEventArgs>	UserDraw;
		#endregion


		/// <summary>
		/// Gets list of frame children
		/// </summary>
		public	IEnumerable<Frame>	Children	{ get { return children; } }


		/// <summary>
		/// Gets frame
		/// </summary>
		public	Frame				Parent		{ get { return parent; } }

		/// <summary>
		/// Global frame rectangle made 
		/// after all layouting and transitioning operation
		/// </summary>
		public	Rectangle	GlobalRectangle		{ get; private set; }



		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="id"></param>
		public Frame ( Game game )
		{
			Game	=	game;
			ui		=	Game.GetService<UserInterface>();
			Init( game );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="w"></param>
		/// <param name="h"></param>
		/// <param name="text"></param>
		/// <param name="backColor"></param>
		/// <returns></returns>
		public static Frame Create ( Game game, int x, int y, int w, int h, string text, Color backColor )
		{
			return new Frame( game ) {
				X = x,
				Y = y,
				Width = w,
				Height = h,
				Text = text,
				BackColor = backColor,
			};
		}



		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="game"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="w"></param>
		/// <param name="h"></param>
		/// <param name="text"></param>
		/// <param name="backColor"></param>
		public Frame ( Game game, int x, int y, int w, int h, string text, Color backColor )
		{
			Game	=	game;
			ui		=	Game.GetService<UserInterface>();
			Init( game );

			X				=	x;
			Y				=	y;
			Width			=	w;
			Height			=	h;
			Text			=	text;
			BackColor		=	backColor;
		}


		
		/// <summary>
		/// Common init 
		/// </summary>
		/// <param name="game"></param>
		void Init ( Game game )
		{
			var ui		=	Game.GetService<UserInterface>();

			Padding			=	0;
			Visible			=	true;
			Enabled			=	true;
			AutoSize		=	false;
			Font			=	ui.DefaultFont;
			ForeColor		=	Color.White;
			Border			=	0;
			BorderColor		=	Color.White;
			ShadowColor		=	Color.Zero;
			OverallColor	=	Color.White;
		
			TextAlignment	=	Alignment.TopLeft;

			Anchor			=	FrameAnchor.Left | FrameAnchor.Top;

			ImageColor		=	Color.White;

			LayoutChanged	+= (s,e) => RunLayout(true);
			Resize			+= (s,e) => RunLayout(true);
		}


		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Hierarchy stuff
		 * 
		-----------------------------------------------------------------------------------------*/

		private	List<Frame>	children	=	new List<Frame>();
		private Frame		parent		=	null;
		

		/// <summary>
		/// Adds frame
		/// </summary>
		/// <param name="frame"></param>
		public void Add ( Frame frame )
		{
			if ( !children.Contains(frame) ) {
				children.Add( frame );
				frame.parent	=	this;
				frame.OnStatusChanged( FrameStatus.None );
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="frame"></param>
		public void Clear ( Frame frame )
		{
			foreach ( var child in children ) {
				child.parent = null;
			}
			children.Clear();
		}


		/// <summary>
		/// Inserts frame at specified index
		/// </summary>
		/// <param name="index"></param>
		/// <param name="frame"></param>
		public void Insert ( int index, Frame frame )
		{
			if ( !children.Contains(frame) ) {
				children.Insert( index, frame );
				frame.parent	=	this;
				frame.OnStatusChanged( FrameStatus.None );
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="frame"></param>
		public void Remove ( Frame frame )
		{
			if ( this.children.Contains(frame) ) {
				this.children.Remove( frame );
				frame.parent	=	this;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public List<Frame>	GetAncestorList ()
		{
			var list = new List<Frame>();

			var frame = this;

			while ( frame != null ) {
				list.Add( frame );
				frame = frame.parent;
			}

			return list;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="action"></param>
		public void ForEachAncestor ( Action<Frame> action ) 
		{
			GetAncestorList().ForEach( f => action(f) );
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="action"></param>
		public void ForEachChildren ( Action<Frame> action ) 
		{
			children.ForEach( f => action(f) );
		}


		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Input stuff :
		 * 
		-----------------------------------------------------------------------------------------*/

		FrameStatus oldStatus = FrameStatus.None;

		internal void OnStatusChanged ( FrameStatus status )
		{
			if (StatusChanged!=null) {
				oldStatus = status;
				StatusChanged( this, new StatusEventArgs(){ Status = status } ); 
			}
		}


		internal void OnClick ()
		{
			int x = Game.InputDevice.MousePosition.X - GlobalRectangle.X;
			int y = Game.InputDevice.MousePosition.Y - GlobalRectangle.Y;

			if (Click!=null) {
				Click( this, new MouseEventArgs(){ Key = Keys.None, X = x, Y = y } );
			}
		}


		internal void OnMouseIn ()
		{
			int x = Game.InputDevice.MousePosition.X - GlobalRectangle.X;
			int y = Game.InputDevice.MousePosition.Y - GlobalRectangle.Y;

			if (MouseIn!=null) {
				MouseIn(this, new MouseEventArgs(){ Key = Keys.None, X = x, Y = y } );
			}
		}


		internal void OnMouseMove (int dx, int dy)
		{
			int x = Game.InputDevice.MousePosition.X - GlobalRectangle.X;
			int y = Game.InputDevice.MousePosition.Y - GlobalRectangle.Y;

			if (MouseMove!=null) {
				MouseMove(this, new MouseEventArgs(){ Key = Keys.None, X = x, Y = y, DX = dx, DY = dy });
			}
		}


		internal void OnMouseOut ()
		{
			int x = Game.InputDevice.MousePosition.X - GlobalRectangle.X;
			int y = Game.InputDevice.MousePosition.Y - GlobalRectangle.Y;

			if (MouseOut!=null) {
				MouseOut(this, new MouseEventArgs(){ Key = Keys.None, X = x, Y = y } );
			}
		}


		internal void OnMouseDown ( Keys key )
		{
			int x = Game.InputDevice.MousePosition.X - GlobalRectangle.X;
			int y = Game.InputDevice.MousePosition.Y - GlobalRectangle.Y;

			if (MouseDown!=null) {
				MouseDown( this, new MouseEventArgs(){ Key = key, X = x, Y = y } );
			}
		}


		internal void OnMouseUp ( Keys key )
		{
			int x = Game.InputDevice.MousePosition.X - GlobalRectangle.X;
			int y = Game.InputDevice.MousePosition.Y - GlobalRectangle.Y;

			if (MouseUp!=null) {
				MouseUp( this, new MouseEventArgs(){ Key = key, X = x, Y = y } );
			}
		}


		internal void OnMouseWheel ( int wheel )
		{
			if (MouseWheel!=null) {
				MouseWheel( this, new MouseEventArgs(){ Wheel = wheel } );
			} else if ( Parent!=null ) {
				Parent.OnMouseWheel( wheel );
			}
		}


		internal void OnTick ()
		{
			if (Tick!=null) {
				Tick(this, EventArgs.Empty);
			}
		}


		internal bool OnGameDraw ( GameTime gameTime, StereoEye stereoEye )
		{
			if (GameDraw!=null) {
				GameDraw( this, new DrawEventArgs(){ GameTime = gameTime, StereoEye = stereoEye } );
				return true;
			}	
			return false;
		}


		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Update and draw stuff :
		 * 
		-----------------------------------------------------------------------------------------*/

		public static List<Frame> BFSList ( Frame v )
		{
			Queue<Frame> Q = new Queue<Frame>();
			List<Frame> list = new List<Frame>();

			Q.Enqueue( v );

			while ( Q.Any() ) {
				
				var t = Q.Dequeue();
				list.Add( t );

				foreach ( var u in t.Children ) {
					Q.Enqueue( u );
				}
			}

			return list;
		}
			

		void UpdateGlobalRect ( int px, int py ) 
		{
			GlobalRectangle = new Rectangle( X + px, Y + py, Width, Height );
			ForEachChildren( ch => ch.UpdateGlobalRect( px + X, py + Y ) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="parentX"></param>
		/// <param name="parentY"></param>
		/// <param name="frame"></param>
		internal void UpdateInternal ( GameTime gameTime, int parentX, int parentY )
		{
		#if true
			var bfsList  = BFSList( this );
			var bfsListR = bfsList.ToList();
			bfsListR.Reverse();
			//bfsList.Reverse();

			UpdateGlobalRect(0,0);

			bfsList .ForEach( f => f.UpdateTransitions(gameTime) );

			UpdateGlobalRect(0,0);

			if (ui.ForceLayout) {
				bfsList.ForEach( f => f.RunLayout(true) );
			}

			bfsList.ForEach( f => f.UpdateMove() );
			bfsList.ForEach( f => f.UpdateResize() );

			UpdateGlobalRect(0,0);

			bfsList .ForEach( f => f.OnTick() );
			bfsList .ForEach( f => f.Update( gameTime ) );

		#else
			UpdateTransitions( gameTime );

			GlobalRectangle		=	new Rectangle( X + parentX, Y + parentY, Width, Height );

			UpdateMoveAndResize(true);

			foreach ( var child in Children ) {
				child.UpdateInternal( gameTime, parentX + X, parentY + Y );
			} //*/

			OnTick();

			Update( gameTime );
		#endif
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="frame"></param>
		internal void DrawInternal ( GameTime gameTime, StereoEye stereoEye, SpriteBatch sb, Color colorMul )
		{
			var vp = Game.GraphicsDevice.DisplayBounds;

			sb.ColorMultiplier	=	colorMul * this.OverallColor;

			if ( IsDrawable && MathUtil.IsRectInsideRect( vp, GlobalRectangle ) ) {

				if (ClippingMode==ClippingMode.None) {

					DrawFrameBorders( sb );

					Draw( gameTime, stereoEye, sb );

					foreach ( var child in Children ) {
						child.DrawInternal( gameTime, stereoEye, sb, sb.ColorMultiplier );
					}

				} else {

					Rectangle clipRect = new Rectangle(0,0,0,0);
					
					if (ClippingMode==ClippingMode.ClipByFrame) {
						clipRect	=	GlobalRectangle;
					}
					if (ClippingMode==ClippingMode.ClipByBorder) {
						clipRect	=	GetBorderedRectangle();
					}
					if (ClippingMode==ClippingMode.ClipByPadding) {
						clipRect	=	GetPaddedRectangle();
					}

					DrawFrameBorders( sb );

					sb.Restart( SpriteBlend.AlphaBlend,null,null,null, clipRect );

						Draw( gameTime, stereoEye, sb );

						foreach ( var child in Children ) {
							child.DrawInternal( gameTime, stereoEye, sb, sb.ColorMultiplier );
						}

					sb.Restart();
				}
			}

			sb.ColorMultiplier	=	colorMul;
		}


		/// <summary>
		/// Updates frame stuff.
		/// </summary>
		/// <param name="gameTime"></param>
		protected virtual void Update ( GameTime gameTime )
		{
		}



		/// <summary>
		/// Draws frame stuff
		/// </summary>
		void DrawFrameBorders ( SpriteBatch sb )
		{
			int gx	=	GlobalRectangle.X;
			int gy	=	GlobalRectangle.Y;
			int w	=	Width;
			int h	=	Height;
			int bt	=	BorderTop;
			int bb	=	BorderBottom;
			int br	=	BorderRight;
			int bl	=	BorderLeft;

			sb.Draw( sb.TextureWhite,	gx,				gy,				w,		bt,				BorderColor ); 
			sb.Draw( sb.TextureWhite,	gx,				gy + h - bb,	w,		bb,				BorderColor ); 
			sb.Draw( sb.TextureWhite,	gx,				gy + bt,		bl,		h - bt - bb,	BorderColor ); 
			sb.Draw( sb.TextureWhite,	gx + w - br,	gy + bt,		br,		h - bt - bb,	BorderColor ); 

			sb.Draw( sb.TextureWhite,	GetBorderedRectangle(), BackColor );
		}



		/// <summary>
		/// Draws frame stuff.
		/// </summary>
		/// <param name="gameTime"></param>
		protected virtual void Draw ( GameTime gameTime, StereoEye stereoEye, SpriteBatch sb )
		{
			DrawFrameImage( sb );
			DrawFrameText( sb );

			if (UserDraw!=null) {
				UserDraw( this, new DrawEventArgs(){ GameTime = gameTime, StereoEye = stereoEye, SpriteBatch = sb } );
			}
		}



		/// <summary>
		/// Adjusts frame size to content, text, image etc.
		/// </summary>
		/// <param name="width"></param>
		/// <param name="?"></param>
		protected virtual void Adjust ()
		{
			throw new NotImplementedException();
		}



		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Utils :
		 * 
		-----------------------------------------------------------------------------------------*/

		int oldX = int.MinValue;
		int oldY = int.MinValue;
		int oldW = int.MinValue;
		int oldH = int.MinValue;
		bool firstResize = true;


		/// <summary>
		/// Checks move and resize and calls appropriate events
		/// </summary>
		protected void UpdateMove ()
		{
			if ( oldX != X || oldY != Y ) {	
				oldX = X;
				oldY = Y;
				if (Move!=null) {
					Move( this, new MoveEventArgs(){ X = X, Y = Y } );
				}
			}
		}



		/// <summary>
		/// 
		/// </summary>
		protected void UpdateResize ()
		{
			if ( oldW != Width || oldH != Height ) {	

				if (Resize!=null) {
					Resize( this, new ResizeEventArgs(){ Width = Width, Height = Height } );
				}

				if (!firstResize) {
					ForEachChildren( f => f.UpdateAnchors( oldW, oldH, Width, Height ) );
				}

				firstResize = false;

				oldW = Width;
				oldH = Height;
			}
		}


		/*protected void UpdateAnchorsAndStoreOldXYWH ()
		{
			if ( oldW != Width || oldH != Height ) {	
				
				if (!firstResize) {
					ForEachChildren( f => f.UpdateAnchors( oldW, oldH, Width, Height ) );
				}
				
				firstResize = false;

				oldW = Width;
				oldH = Height;
			}
		} */



		/// <summary>
		/// 
		/// </summary>
		/// <param name="forceTransitions"></param>
		public void RunLayout (bool forceTransitions)
		{
			if (layout!=null && !ui.SuppressLayout) {
				layout.RunLayout( this, forceTransitions );
			}
		}



		/// <summary>
		/// Get global rectangle bound by borders
		/// </summary>
		/// <returns></returns>
		public Rectangle GetBorderedRectangle ()
		{
			return new Rectangle( 
				GlobalRectangle.X + BorderLeft, 
				GlobalRectangle.Y + BorderTop, 
				Width - BorderLeft - BorderRight,
				Height - BorderTop - BorderBottom );
		}



		/// <summary>
		/// Get global rectangle padded and bound by borders
		/// </summary>
		/// <returns></returns>
		public Rectangle GetPaddedRectangle ( bool global = true )
		{
			int x = global ? GlobalRectangle.X : 0;
			int y = global ? GlobalRectangle.Y : 0;

			return new Rectangle( 
				x + BorderLeft + PaddingLeft, 
				y + BorderTop + PaddingTop, 
				Width  - BorderLeft - BorderRight - PaddingLeft - PaddingRight,
				Height - BorderTop - BorderBottom - PaddingTop - PaddingBottom );
		}



		/// <summary>
		/// 
		/// </summary>
		protected void DrawFrameImage (SpriteBatch sb)
		{
			if (Image==null) {
				return;
			}

			var gp = GetPaddedRectangle();
			var bp = GetBorderedRectangle();

			if (ImageMode==FrameImageMode.Stretched) {
				sb.Draw( Image, gp, ImageColor );
				return;
			}

			if (ImageMode==FrameImageMode.Centered) {
				int x = gp.X + gp.Width/2  - Image.Width/2;
				int y = gp.Y + gp.Height/2 - Image.Height/2;
				sb.Draw( Image, x, y, Image.Width, Image.Height, ImageColor );
				return;
			}

			if (ImageMode==FrameImageMode.Tiled) {
				sb.Draw( Image, bp, new Rectangle(0,0,bp.Width,bp.Height), ImageColor );
				return;
			}

			if (ImageMode == FrameImageMode.DirectMapped) {
				sb.Draw(Image, gp, gp, ImageColor);
				return;
			}


		}



		/// <summary>
		/// Draws string
		/// </summary>
		/// <param name="text"></param>
		protected void DrawFrameText ( SpriteBatch sb )
		{											
			if (string.IsNullOrEmpty(Text)) {
				return;
			}

			var r	=	Font.MeasureStringF( Text );
			int x	=	0;
			int y	=	0;
			var gp	=	GetPaddedRectangle();

			int hAlign	=	0;
			int vAlign	=	0;

			switch (TextAlignment) {
				case Alignment.TopLeft			: hAlign = -1; vAlign = -1; break;
				case Alignment.TopCenter		: hAlign =  0; vAlign = -1; break;
				case Alignment.TopRight			: hAlign =  1; vAlign = -1; break;
				case Alignment.MiddleLeft		: hAlign = -1; vAlign =  0; break;
				case Alignment.MiddleCenter		: hAlign =  0; vAlign =  0; break;
				case Alignment.MiddleRight		: hAlign =  1; vAlign =  0; break;
				case Alignment.BottomLeft		: hAlign = -1; vAlign =  1; break;
				case Alignment.BottomCenter		: hAlign =  0; vAlign =  1; break;
				case Alignment.BottomRight		: hAlign =  1; vAlign =  1; break;

				case Alignment.BaselineLeft		: hAlign = -1; vAlign =  2; break;
				case Alignment.BaselineCenter	: hAlign =  0; vAlign =  2; break;
				case Alignment.BaselineRight	: hAlign =  1; vAlign =  2; break;
			}

			if ( hAlign  < 0 )	x	=	gp.X;
			if ( hAlign == 0 )	x	=	gp.X + (int)( gp.Width/2 - r.Width/2 );
			if ( hAlign  > 0 )	x	=	gp.X + (int)( gp.Width - r.Width );

			if ( vAlign  < 0 )	y	=	gp.Y + (int)( 0 );
			if ( vAlign == 0 )	y	=	gp.Y + (int)( Font.CapHeight/2 - Font.BaseLine + gp.Height/2 );
			if ( vAlign  > 0 )	y	=	gp.Y + (int)( gp.Height - Font.LineHeight );
			if ( vAlign == 2 )	y	=	gp.Y - Font.BaseLine;

			/*if (TextAlignment==Alignment.BaselineLeft) {
				x	=	gp.X;
				y	=	gp.Y - Font.BaseLine;
			}

			if (TextAlignment==Alignment.BaselineLeft) {
				x	=	gp.X;
				y	=	gp.Y - Font.BaseLine;
			}

			if (TextAlignment==Alignment.BaselineLeft) {
				x	=	gp.X;
				y	=	gp.Y - Font.BaseLine;
			} */

			/*if (TextEffect==TextEffect.Shadow) {
				Font.DrawString( sb, Text, x + TextOffsetX+1, y + TextOffsetY+1, ShadowColor, 0, false );
			} */

			if (ShadowColor.A!=0) {
				Font.DrawString( sb, Text, x + TextOffsetX+ShadowOffset.X, y + TextOffsetY+ShadowOffset.Y, ShadowColor, 0, false );
			}

			Font.DrawString( sb, Text, x + TextOffsetX, y + TextOffsetY, ForeColor, 0, false );
		}


		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Animation stuff :
		 * 
		-----------------------------------------------------------------------------------------*/

		List<ITransition>	transitions	=	new List<ITransition>();


		/// <summary>
		/// Pushes new transition to the chain of animation transitions.
		/// Origin value will be retrived when transition starts.
		/// When one of the newest transitions starts, previous transitions on same property will be terminated.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="I"></typeparam>
		/// <param name="property"></param>
		/// <param name="termValue"></param>
		/// <param name="delay"></param>
		/// <param name="period"></param>
		public void RunTransition<T,I> ( string property, T targetValue, int delay, int period ) where I: IInterpolator<T>, new()
		{
			var pi	=	GetType().GetProperty( property );
			
			if ( pi.PropertyType != typeof(T) ) {	
				throw new ArgumentException(string.Format("Bad property and types: {0} is {1}, but values are {2}", property, pi.PropertyType, typeof(T)) );
			}

			//	call ToList() to terminate LINQ evaluation :
			var toCancel = transitions.Where( t => t.TagName == property ).ToList();

			transitions.Add( new Transition<T,I>( this, pi, targetValue, delay, period, toCancel ){ TagName = property } );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="property"></param>
		/// <param name="targetValue"></param>
		/// <param name="delay"></param>
		/// <param name="period"></param>
		public void RunTransition ( string property, Color targetValue, int delay, int period )
		{
			RunTransition<Color, ColorInterpolator>( property, targetValue, delay, period );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="property"></param>
		/// <param name="targetValue"></param>
		/// <param name="delay"></param>
		/// <param name="period"></param>
		public void RunTransition ( string property, int targetValue, int delay, int period )
		{
			RunTransition<int, IntInterpolator>( property, targetValue, delay, period );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		void UpdateTransitions ( GameTime gameTime )
		{
			foreach ( var t in transitions ) {
				t.Update( gameTime );
			}

			transitions.RemoveAll( t => t.IsDone );
		}



		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Anchors :
		 * 
		-----------------------------------------------------------------------------------------*/

		/// <summary>
		/// Incrementally preserving half offset
		/// </summary>
		/// <param name="oldV"></param>
		/// <param name="newV"></param>
		/// <param name="x"></param>
		/// <returns></returns>
		int SafeHalfOffset ( int oldV, int newV, int x )
		{
			int dw = newV - oldV;

			if ( (dw & 1)==1 ) {

				if ( dw > 0 ) {

					if ( (oldV&1)==1 ) {
						dw ++;
					}

				} else {

					if ( (oldV&1)==0 ) {
						dw --;
					}
				}

				return	x + dw/2;

			} else {
				return	x + dw/2;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="oldW"></param>
		/// <param name="oldH"></param>
		/// <param name="newW"></param>
		/// <param name="newH"></param>
		void UpdateAnchors ( int oldW, int oldH, int newW, int newH )
		{
			int dw	=	newW - oldW;
			int dh	=	newH - oldH;

			if ( !Anchor.HasFlag( FrameAnchor.Left ) && !Anchor.HasFlag( FrameAnchor.Right ) ) {
				X	=	SafeHalfOffset( oldW, newW, X );				
			}

			if ( !Anchor.HasFlag( FrameAnchor.Left ) && Anchor.HasFlag( FrameAnchor.Right ) ) {
				X	=	X + dw;
			}

			if ( Anchor.HasFlag( FrameAnchor.Left ) && !Anchor.HasFlag( FrameAnchor.Right ) ) {
			}

			if ( Anchor.HasFlag( FrameAnchor.Left ) && Anchor.HasFlag( FrameAnchor.Right ) ) {
				Width	=	Width + dw;
			}


		
			if ( !Anchor.HasFlag( FrameAnchor.Top ) && !Anchor.HasFlag( FrameAnchor.Bottom ) ) {
				Y	=	SafeHalfOffset( oldH, newH, Y );				
			}

			if ( !Anchor.HasFlag( FrameAnchor.Top ) && Anchor.HasFlag( FrameAnchor.Bottom ) ) {
				Y	=	Y + dh;
			}

			if ( Anchor.HasFlag( FrameAnchor.Top ) && !Anchor.HasFlag( FrameAnchor.Bottom ) ) {
			}

			if ( Anchor.HasFlag( FrameAnchor.Top ) && Anchor.HasFlag( FrameAnchor.Bottom ) ) {
				Height	=	Height + dh;
			}
		}


		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Layouting :
		 * 
		-----------------------------------------------------------------------------------------*/

	}
}


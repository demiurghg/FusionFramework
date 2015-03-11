using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Mathematics;
using Fusion.Graphics;
using Fusion.Input;
using System.Runtime.InteropServices;



namespace QuadDemo2D {
	class QuadDemo : Game {

		[StructLayout(LayoutKind.Explicit)]
		struct ConstData {
			[FieldOffset(0)]	
			public Matrix Transform;
		}

		Ubershader			us;
		VertexBuffer		vb;
		ConstantBuffer		cb;
		VertexInputLayout	layout;
		Texture2D			tex;
		ConstData			cbData;

		enum UberFlags {
			USE_VERTEX_COLOR = 1,
			USE_TEXTURE = 2,
		}


		struct Vertex {
			[Vertex("POSITION")]	public Vector3	Position;
			[Vertex("TEXCOORD", 0)]	public Vector2	TexCoord;
			[Vertex("COLOR")]		public Color	Color;
		}



		/// <summary>
		///	Add services and set options
		/// </summary>
		public QuadDemo ()
		{
		}



		/// <summary>
		/// Load stuff here
		/// </summary>
		protected override void Initialize ()
		{
			var device	=	GraphicsDevice;

			base.Initialize();

			LoadContent();

			vb		=	new VertexBuffer( device, typeof(Vertex), 6 );
			cb		=	new ConstantBuffer(GraphicsDevice, typeof(ConstData));
			layout	=	new VertexInputLayout( device, typeof(Vertex) );


			Reloading += (s,e) => LoadContent();
		}


		void LoadContent ()
		{
			tex		=	Content.Load<Texture2D>("lena.tga" );

			us		=	Content.Load<Ubershader>("test.hlsl");
			us.Map( typeof(UberFlags) );
		}



		/// <summary>
		/// Kill stuff here
		/// </summary>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref us );
				SafeDispose( ref vb );
				SafeDispose( ref cb );
				SafeDispose( ref tex );
			}

			base.Dispose(disposing);
		}



		/// <summary>
		/// Update stuff here
		/// </summary>
		/// <param name="gameTime"></param>
		protected override void Update ( GameTime gameTime )
		{
			if (InputDevice.IsKeyDown(Keys.Escape)) {
				Exit();
			}

			base.Update( gameTime );
		}



		/// <summary>
		/// Draw stuff here
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		protected override void Draw ( GameTime gameTime, StereoEye stereoEye )
		{
			//	Clear back buffer :
			GraphicsDevice.ClearBackbuffer( new Color4(0,0,0,0) );

			//	Update constant buffer and bound it to pipeline:
			cbData.Transform	=	Matrix.OrthoRH( 4, 3, -2, 2 );
			cb.SetData(cbData);

			GraphicsDevice.VertexShaderConstants[0]	= cb;
			GraphicsDevice.PixelShaderConstants[0]	= cb;

			//	Fill vertex buffer :
			var v0	=	new Vertex{ Position = new Vector3( -1.0f, -1.0f, 0 ), Color = Color.Red,   TexCoord = new Vector2(0,1) };
			var v1	=	new Vertex{ Position = new Vector3(  1.0f,  1.0f, 0 ), Color = Color.White, TexCoord = new Vector2(1,0) };
			var v2	=	new Vertex{ Position = new Vector3( -1.0f,  1.0f, 0 ), Color = Color.Blue,  TexCoord = new Vector2(0,0) };
			var v3	=	new Vertex{ Position = new Vector3( -1.0f, -1.0f, 0 ), Color = Color.Red,   TexCoord = new Vector2(0,1) };
			var v4	=	new Vertex{ Position = new Vector3(  1.0f, -1.0f, 0 ), Color = Color.Lime,  TexCoord = new Vector2(1,1) };
			var v5	=	new Vertex{ Position = new Vector3(  1.0f,  1.0f, 0 ), Color = Color.White, TexCoord = new Vector2(1,0) };//*/

			var data = new Vertex[]{ v0, v1, v2, v3, v4, v5 };

			vb.SetData( data, 0, 6 );

			try {

				//	Set required ubershader :
				int flags = 0;
				if ( InputDevice.IsKeyDown(Keys.D1) ) flags |= (int)UberFlags.USE_VERTEX_COLOR;
				if ( InputDevice.IsKeyDown(Keys.D2) ) flags |= (int)UberFlags.USE_TEXTURE;

				us.SetPixelShader( flags );
				us.SetVertexShader( flags );

				//	Set device states :
				GraphicsDevice.BlendState			= BlendState.Opaque ;
				GraphicsDevice.RasterizerState		= RasterizerState.CullNone ;
				GraphicsDevice.DepthStencilState	= DepthStencilState.None ;
				GraphicsDevice.PixelShaderSamplers[0]	= SamplerState.LinearWrap ;

				//	Setup texture :
				GraphicsDevice.PixelShaderResources[0]	= tex ;
								
				//	Setup vertex data and draw :			
				GraphicsDevice.SetupVertexInput( layout, vb, null );
				GraphicsDevice.Draw( Primitive.TriangleList, 6, 0 );

			} catch ( UbershaderException uex )	{
				uex.Report();
			}

			base.Draw( gameTime, stereoEye );
		}
	}
}

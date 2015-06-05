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
using System.Diagnostics;
using System.IO;



namespace QuadDemo2D {
	class QuadDemo : Game {

		[StructLayout(LayoutKind.Explicit)]
		struct ConstData {
			[FieldOffset(0)]	
			public Matrix Transform;
		}

		Ubershader			ubershader;
		VertexBuffer		vertexBuffer;
		ConstantBuffer		constBuffer;
		Texture2D			texture;
		Texture2D			textureSRgb;
		ConstData			cbData;
		StateFactory		factory;

		enum UberFlags {
			NONE = 0,
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
			Parameters.UseDebugDevice =	true;
		}



		/// <summary>
		/// Load stuff here
		/// </summary>
		protected override void Initialize ()
		{
			var device	=	GraphicsDevice;

			base.Initialize();

			LoadContent();

			vertexBuffer	=	new VertexBuffer( device, typeof(Vertex), 6 );
			constBuffer		=	new ConstantBuffer( device, typeof(ConstData) );

			Reloading += (s,e) => LoadContent();

			//Trace.Fail("FUCK");
			//Trace.Assert(true);
			//Trace.Assert(false);
			//Trace.Fail("Some epic fail");
		}


		void LoadContent ()
		{
			SafeDispose( ref factory );

			//texture		=	new Texture2D( GraphicsDevice, File.ReadAllBytes(@"D:\image.png"), false );
			texture		=	Content.Load<Texture2D>("lena.tga" );
			ubershader	=	Content.Load<Ubershader>("test.hlsl");
			factory		=	new StateFactory( ubershader, typeof(UberFlags), Primitive.TriangleList, VertexInputElement.FromStructure(typeof(Vertex) ) );
		}



		/// <summary>
		/// Kill stuff here
		/// </summary>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref vertexBuffer );
				SafeDispose( ref constBuffer );
				SafeDispose( ref texture );
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

			//	Fill vertex buffer :
			var v0	=	new Vertex{ Position = new Vector3( -1.0f, -1.0f, 0 ), Color = Color.Red,   TexCoord = new Vector2(0,1) };
			var v1	=	new Vertex{ Position = new Vector3(  1.0f,  1.0f, 0 ), Color = Color.White, TexCoord = new Vector2(1,0) };
			var v2	=	new Vertex{ Position = new Vector3( -1.0f,  1.0f, 0 ), Color = Color.Blue,  TexCoord = new Vector2(0,0) };
			var v3	=	new Vertex{ Position = new Vector3( -1.0f, -1.0f, 0 ), Color = Color.Red,   TexCoord = new Vector2(0,1) };
			var v4	=	new Vertex{ Position = new Vector3(  1.0f, -1.0f, 0 ), Color = Color.Lime,  TexCoord = new Vector2(1,1) };
			var v5	=	new Vertex{ Position = new Vector3(  1.0f,  1.0f, 0 ), Color = Color.White, TexCoord = new Vector2(1,0) };//*/

			var data = new Vertex[]{ v0, v1, v2, v3, v4, v5 };

			vertexBuffer.SetData( data, 0, 6 );

			var tex = texture;

			UberFlags flags = UberFlags.NONE;
			if (InputDevice.IsKeyDown(Keys.D1) ) flags |= UberFlags.USE_TEXTURE;
			if (InputDevice.IsKeyDown(Keys.D2) ) flags |= UberFlags.USE_VERTEX_COLOR;
			if (InputDevice.IsKeyDown(Keys.D3) ) tex = textureSRgb;

			//	Update constant buffer and bound it to pipeline:
			cbData.Transform	=	Matrix.OrthoRH( 4, 3, -2, 2 );
			constBuffer.SetData(cbData);

			GraphicsDevice.VertexShaderConstants[0]	= constBuffer;
			GraphicsDevice.PixelShaderConstants[0]	= constBuffer;

			//	Setup device state :
			GraphicsDevice.PipelineState			= factory[ (int)flags ];
			GraphicsDevice.PixelShaderSamplers[0]	= SamplerState.LinearWrap ;

			//	Setup texture :
			GraphicsDevice.PixelShaderResources[0]	= tex ;
								
			//	Setup vertex data and draw :			
			GraphicsDevice.SetupVertexInput( vertexBuffer, null );
			GraphicsDevice.Draw( 6, 0 );


			base.Draw( gameTime, stereoEye );
		}
	}
}

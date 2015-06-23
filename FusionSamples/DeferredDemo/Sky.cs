using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion;
using Fusion.Mathematics;
using Fusion.Audio;
using Fusion.Content;
using Fusion.Graphics;
using Fusion.Input;
using Fusion.Development;
using System.Runtime.InteropServices;

namespace DeferredDemo
{
	public class Sky : GameService {
		[Flags]
		enum SkyFlags : int
		{
			PROCEDURAL_SKY		= 1 << 0,
			PANORAMIC_SKY_HALF	= 1 << 1,
			PANORAMIC_SKY_FULL	= 1 << 2,
			CUBEMAP_SKY			= 1 << 3,
			FOG					= 1 << 4,
			SRGB				= 1 << 5,
			CIERGB				= 1 << 6,
			CLOUDS				= 1 << 7,
			A					= 1 << 8,
			B					= 1 << 9,
			C					= 1 << 10,
		}

		//	row_major float4x4 MatrixWVP;      // Offset:    0 Size:    64 [unused]
		//	float3 SunPosition;                // Offset:   64 Size:    12
		//	float4 SunColor;                   // Offset:   80 Size:    16
		//	float Turbidity;                   // Offset:   96 Size:     4 [unused]
		//	float3 Temperature;                // Offset:  100 Size:    12
		//	float SkyIntensity;                // Offset:  112 Size:     4
		[StructLayout(LayoutKind.Explicit, Size=160)]
		struct SkyConsts {
			[FieldOffset(  0)] public Matrix 	MatrixWVP;
			[FieldOffset( 64)] public Vector3	SunPosition;
			[FieldOffset( 80)] public Color4	SunColor;
			[FieldOffset( 96)] public float		Turbidity;
			[FieldOffset(100)] public Vector3	Temperature; 
			[FieldOffset(112)] public float		SkyIntensity; 
			[FieldOffset(116)] public Vector3	Ambient;
			[FieldOffset(128)] public float		Time;
			[FieldOffset(132)] public Vector3	ViewPos;
		}

		GraphicsDevice	rs;
		Scene			skySphere;
		Scene			cloudSphere;
		Ubershader		sky;
		ConstantBuffer	skyConstsCB;
		SkyConsts		skyConstsData;
		StateFactory	factory;

		public Vector3	SkyAmbientLevel { get; protected set; }

		[Config]
		public Config Params { get; set; }

		Vector3[]		randVectors;

		public RenderTargetCube	SkyCube { get { return skyCube; } }
		RenderTargetCube	skyCube;
		Texture2D			clouds;
		Texture2D			cirrus;
		Texture2D			noise;
		Texture2D			arrows;

		#region Sky model
		float dot ( Vector3 a, Vector3 b ) { return Vector3.Dot(a, b); }
		float dot ( Vector4 a, Vector4 b ) { return Vector4.Dot(a, b); }
		float acos(float x) { return (float)Math.Acos(x);}
		float tan (float x) { return (float)Math.Tan(x);}
		float exp (float x) { return (float)Math.Exp(x);}

		Vector3 perezZenith ( float t, float thetaSun )
		{
			float	pi = 3.1415926f;
			Vector4	cx1 = new Vector4 ( 0.00000f,  0.00209f, -0.00375f,  0.00165f );
			Vector4	cx2 = new Vector4 ( 0.00394f, -0.03202f,  0.06377f, -0.02903f );
			Vector4	cx3 = new Vector4 ( 0.25886f,  0.06052f, -0.21196f,  0.11693f );
			Vector4	cy1 = new Vector4 ( 0.00000f,  0.00317f, -0.00610f,  0.00275f );
			Vector4	cy2 = new Vector4 ( 0.00516f, -0.04153f,  0.08970f, -0.04214f );
			Vector4	cy3 = new Vector4 ( 0.26688f,  0.06670f, -0.26756f,  0.15346f );

			float	t2    = t*t;
			float	chi   = (4.0f / 9.0f - t / 120.0f ) * (pi - 2.0f * thetaSun );
			Vector4	theta = new Vector4 ( 1, thetaSun, thetaSun*thetaSun, thetaSun*thetaSun*thetaSun );

			float	Y = (4.0453f * t - 4.9710f) * tan ( chi ) - 0.2155f * t + 2.4192f;
			float	x = t2 * dot ( cx1, theta ) + t * dot ( cx2, theta ) + dot ( cx3, theta );
			float	y = t2 * dot ( cy1, theta ) + t * dot ( cy2, theta ) + dot ( cy3, theta );

			return new Vector3 ( Y, x, y );//*/
		}


		Vector3  perezFunc ( float t, float cosTheta, float cosGamma )
		{
			//return 1;
			float  gamma      = acos ( cosGamma );
			float  cosGammaSq = cosGamma * cosGamma;
			float  aY =  0.17872f * t - 1.46303f;	      float  bY = -0.35540f * t + 0.42749f;
			float  cY = -0.02266f * t + 5.32505f;	      float  dY =  0.12064f * t - 2.57705f;
			float  eY = -0.06696f * t + 0.37027f;	      float  ax = -0.01925f * t - 0.25922f;
			float  bx = -0.06651f * t + 0.00081f;	      float  cx = -0.00041f * t + 0.21247f;
			float  dx = -0.06409f * t - 0.89887f;	      float  ex = -0.00325f * t + 0.04517f;
			float  ay = -0.01669f * t - 0.26078f;	      float  by = -0.09495f * t + 0.00921f;
			float  cy = -0.00792f * t + 0.21023f;	      float  dy = -0.04405f * t - 1.65369f;
			float  ey = -0.01092f * t + 0.05291f;	  
			return new Vector3 ( (1.0f + aY * exp(bY/cosTheta)) * (1.0f + cY * exp(dY * gamma) + eY*cosGammaSq),
							(1.0f + ax * exp(bx/cosTheta)) * (1.0f + cx * exp(dx * gamma) + ex*cosGammaSq),
							(1.0f + ay * exp(by/cosTheta)) * (1.0f + cy * exp(dy * gamma) + ey*cosGammaSq) );
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="t">Turbidity</param>
		/// <param name="cosTheta">cosine of view angle</param>
		/// <param name="cosGamma">cosine of angle between view dir and sun dir</param>
		/// <param name="cosThetaSun">cosine of sun angle</param>
		/// <returns></returns>
		Vector3  perezSky ( float t, float cosTheta, float cosGamma, float cosThetaSun )
		{
			float	thetaSun	=	acos ( cosThetaSun );
			Vector3 zenith		=	perezZenith ( t, thetaSun );
			Vector3 clrYxy		=	zenith * perezFunc ( t, cosTheta, cosGamma );
			Vector3	perez1		=	perezFunc ( t, 1.0f, cosThetaSun );

			clrYxy.X /= perez1.X;
			clrYxy.Y /= perez1.Y;
			clrYxy.Z /= perez1.Z;
			//clrYxy.X = clrYxy.X * smoothstep ( 0.0f, 0.1f, cosThetaSun );			// make sure when thetaSun > PI/2 we have black color
	
			return clrYxy;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="t">Turbidity</param>
		/// <param name="viewDir">Normalized (!) view direction</param>
		/// <param name="sunDir">Normalized (!) sun direction</param>
		/// <returns></returns>
		Vector3  perezSky ( float t, Vector3 viewDir, Vector3 sunDir )
		{
			return perezSky( t, Math.Max( 0, viewDir.Y ), Vector3.Dot( viewDir, sunDir ), Math.Max( 0, sunDir.Y ) );
		}


		Vector3 perezSun ( float t, float cosThetaSun, float boost=100 )
		{
			return perezSky( t, cosThetaSun, 1, cosThetaSun ) * new Vector3(boost,1,1);
		}

		Vector3 YxyToRGB ( Vector3 Yxy )
		{
			//var uc = Game.GetService<Settings>().UserConfig;

			Vector3  clrYxy = Yxy;

			clrYxy.X = MathUtil.Clamp( clrYxy.X, 0, 999999 );

			Vector3  XYZ;
			//float tm = 0.7f * 1/(1 + clrYxy.X); 
			float tm = 1;

			//clrYxy.X = 1.0f - exp ( -clrYxy.X / uc.SkyExposure );

			XYZ.X = clrYxy.X * clrYxy.Y / clrYxy.Z;     
			XYZ.Y = clrYxy.X;              
			XYZ.Z = (1 - clrYxy.Y - clrYxy.Z) * clrYxy.X / clrYxy.Z; 

			Vector3 rCoeffs = new Vector3 ( 2.0413690f, -0.5649464f, -0.3446944f);
			Vector3 gCoeffs = new Vector3 (-0.9692660f,  1.8760108f,  0.0415560f);
			Vector3 bCoeffs = new Vector3 ( 0.0134474f, -0.1183897f,  1.0154096f);

			return new Vector3 ( dot ( rCoeffs, XYZ ), dot ( gCoeffs, XYZ ), dot ( bCoeffs, XYZ ) ) * tm;
		}
		#endregion

		Random	rand = new Random();


		VertexBuffer[]	vertexBuffers;
		IndexBuffer[]	indexBuffers;

		VertexBuffer[]	cloudVertexBuffers;
		IndexBuffer[]	cloudIndexBuffers;


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="rs"></param>
		public Sky ( Game game ) : base( game )
		{
			rs	=	game.GraphicsDevice;
			Params = new Config();
		}



		/// <summary>
		/// 
		/// </summary>
		public override void Initialize() 
		{
			skyCube		=	new RenderTargetCube( rs, ColorFormat.Rgba16F, 128, true );
			skyConstsCB	=	new ConstantBuffer( rs, typeof(SkyConsts) );

			LoadContent();

			Game.Reloading += (s,e) => LoadContent();

			
			randVectors	=	new Vector3[64];

			for (int i=0; i<randVectors.Length; i++) {
				Vector3 randV;
				do {
					randV = rand.NextVector3( -Vector3.One, Vector3.One );
				} while ( randV.Length()>1 && randV.Y < 0 );

				randVectors[i] = randV.Normalized();
			}
		}



		/// <summary>
		/// 
		/// </summary>
		void LoadContent ()
		{
			SafeDispose( ref factory );

			skySphere	=	Game.Content.Load<Scene>("skySphere");
			cloudSphere	=	Game.Content.Load<Scene>("cloudSphere");
			clouds		=	Game.Content.Load<Texture2D>("clouds|srgb");
			cirrus		=	Game.Content.Load<Texture2D>("cirrus|srgb");
			noise		=	Game.Content.Load<Texture2D>("cloudNoise");
			arrows		=	Game.Content.Load<Texture2D>("arrowsAll");


			vertexBuffers	=	skySphere.Meshes
							.Select( mesh => VertexBuffer.Create( Game.GraphicsDevice, mesh.Vertices.Select( v => VertexColorTextureTBN.Convert( v ) ).ToArray() ) )
							.ToArray();

			indexBuffers	=	skySphere.Meshes
							.Select( mesh => IndexBuffer.Create( Game.GraphicsDevice, mesh.GetIndices() ) )
							.ToArray();

			cloudVertexBuffers	=	cloudSphere.Meshes
							.Select( mesh => VertexBuffer.Create( Game.GraphicsDevice, mesh.Vertices.Select( v => VertexColorTextureTBN.Convert( v ) ).ToArray() ) )
							.ToArray();

			cloudIndexBuffers	=	cloudSphere.Meshes
							.Select( mesh => IndexBuffer.Create( Game.GraphicsDevice, mesh.GetIndices() ) )
							.ToArray();

			sky		=	Game.Content.Load<Ubershader>("sky");
			factory	=	new StateFactory( sky, typeof(SkyFlags), (ps,i) => EnumFunc(ps, (SkyFlags)i) );

		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="ps"></param>
		/// <param name="flags"></param>
		void EnumFunc ( PipelineState ps, SkyFlags flags )
		{
			ps.VertexInputElements	=	VertexInputElement.FromStructure<VertexColorTextureTBN>();
			ps.RasterizerState		=	RasterizerState.CullNone;
			ps.BlendState			=	BlendState.Opaque;
			ps.DepthStencilState	=	flags.HasFlag(SkyFlags.FOG) ? DepthStencilState.None : DepthStencilState.Readonly;

			if (flags.HasFlag(SkyFlags.CLOUDS)) {
				ps.BlendState	=	BlendState.AlphaBlend;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing ) {
				SafeDispose( ref factory );
				SafeDispose( ref sky );
				SafeDispose( ref skyCube );
				SafeDispose( ref skyConstsCB );
			}
			base.Dispose( disposing );
		}



		/// <summary>
		/// Does nothing
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update( GameTime gameTime )
		{
			//filteredSunPosition	=	Vector3.Lerp( filteredSunPosition, Params.SunDirection.Normalized(), 0.01f );
		}



		void ApplyColorSpace ( ref SkyFlags flags )
		{	
			switch (Params.RgbSpace) {
				case RgbSpace.CIE_RGB	: flags |= SkyFlags.CIERGB;	break;
				case RgbSpace.sRGB		: flags |= SkyFlags.SRGB;	break;
			}
		}



		/// <summary>
		/// Renders fog look-up table
		/// </summary>
		public void RenderFogTable()
		{
			var	sunPos		= GetSunDirection();
			var sunColor	= GetSunLightColor();

			var rotation	=	Matrix.Identity;
			var projection	=	MathUtil.ComputeCubemapProjectionMatrixLH( 0.125f, 10.0f );
			var cubeWVPS	=	MathUtil.ComputeCubemapViewMatriciesLH( Vector3.Zero, rotation, projection );

			var flags		=	SkyFlags.FOG;

			ApplyColorSpace( ref flags );
				
			rs.PipelineState	=	factory[(int)flags];
//			rs.DepthStencilState = DepthStencilState.None ;

			skyConstsData.SunPosition	= sunPos;
			skyConstsData.SunColor		= sunColor;
			skyConstsData.Turbidity		= Params.SkyTurbidity;
			skyConstsData.Temperature	= Temperature.Get( Params.SunTemperature ); 
			skyConstsData.SkyIntensity	= Params.SkyIntensity;

			for( int i = 0; i < 6; ++i ) {
				rs.SetTargets( null, SkyCube.GetSurface(0, (CubeFace)i ) );

				SkyCube.SetViewport();

				skyConstsData.MatrixWVP = cubeWVPS[i];
	
				skyConstsCB.SetData( skyConstsData );
				rs.VertexShaderConstants[0] = skyConstsCB;
				rs.PixelShaderConstants[0] = skyConstsCB;


				for ( int j=0; j<skySphere.Meshes.Count; j++) {
					var mesh = skySphere.Meshes[j];

					rs.SetupVertexInput( vertexBuffers[j], indexBuffers[j] );
					rs.DrawIndexed( mesh.IndexCount, 0, 0 );
				}
			}

			rs.ResetStates();

			SkyCube.BuildMipmaps();
		}



		/// <summary>
		/// Renders sky with specified technique
		/// </summary>
		/// <param name="rendCtxt"></param>
		/// <param name="techName"></param>
		public void Render( GameTime gameTime, DepthStencilSurface depthBuffer, RenderTargetSurface hdrTarget, Matrix view, Matrix projection )
		{
			var camera = Game.GetService<Camera>();

			var scale		=	Matrix.Scaling( Params.SkySphereSize );
			var rotation	=	Matrix.Identity;

			var	sunPos		=	GetSunDirection();
			var sunColor	=	GetSunGlowColor();

			rs.ResetStates();

			//rs.DepthStencilState = depthBuffer==null? DepthStencilState.None : DepthStencilState.Default ;

			rs.SetViewport( 0, 0, hdrTarget.Width, hdrTarget.Height );

			rs.SetTargets( depthBuffer, hdrTarget );

			var viewMatrix = view;
			var projMatrix = projection;

			skyConstsData.MatrixWVP		= scale * rotation * MathUtil.Transformation( viewMatrix.Right, viewMatrix.Up, viewMatrix.Backward ) * projMatrix;
			skyConstsData.SunPosition	= sunPos;
			skyConstsData.SunColor		= sunColor;
			skyConstsData.Turbidity		= Params.SkyTurbidity;
			skyConstsData.Temperature	= Temperature.Get( Params.SunTemperature ); 
			skyConstsData.SkyIntensity	= Params.SkyIntensity;
	
			skyConstsCB.SetData( skyConstsData );
			
			rs.VertexShaderConstants[0] = skyConstsCB;
			rs.PixelShaderConstants[0] = skyConstsCB;


			//
			//	Sky :
			//
			SkyFlags flags = SkyFlags.PROCEDURAL_SKY;

			ApplyColorSpace( ref flags );
				
			rs.PipelineState	=	factory[(int)flags];
						
			for ( int j=0; j<skySphere.Meshes.Count; j++) {
				var mesh = skySphere.Meshes[j];

				rs.SetupVertexInput( vertexBuffers[j], indexBuffers[j] );
				rs.DrawIndexed( mesh.IndexCount, 0, 0 );
			}



			//
			//	Clouds :
			//
			scale		=	Matrix.Scaling( Params.SkySphereSize, Params.SkySphereSize, Params.SkySphereSize );
			//scale		=	Matrix.Scaling( Params.SkySphereSize, Params.SkySphereSize * 0.1f, Params.SkySphereSize );
			skyConstsData.MatrixWVP		= scale * rotation * MathUtil.Transformation( viewMatrix.Right, viewMatrix.Up, viewMatrix.Backward ) * projMatrix;
			skyConstsData.SunPosition	= sunPos;
			skyConstsData.SunColor		= GetSunLightColor();
			skyConstsData.Turbidity		= Params.SkyTurbidity;
			skyConstsData.Temperature	= Temperature.Get( Params.SunTemperature ); 
			skyConstsData.SkyIntensity	= Params.SkyIntensity;
			skyConstsData.Ambient		= GetAmbientLevel().ToVector3();
			skyConstsData.Time			= (float)gameTime.Total.TotalSeconds;
	
			skyConstsCB.SetData( skyConstsData );


			flags = SkyFlags.CLOUDS;

			for (int i=0; i<3; i++) {

				 if (i==0) flags = SkyFlags.CLOUDS| SkyFlags.A;
				 if (i==1) flags = SkyFlags.CLOUDS| SkyFlags.B;
				 if (i==2) flags = SkyFlags.CLOUDS| SkyFlags.C;

				ApplyColorSpace( ref flags );
			
				rs.PipelineState			=	factory[(int)flags];
				rs.PixelShaderResources[0]	=	clouds;
				rs.PixelShaderResources[1]	=	cirrus;
				rs.PixelShaderResources[2]	=	noise;
				rs.PixelShaderResources[3]	=	arrows;
				rs.PixelShaderSamplers[0]	=	SamplerState.AnisotropicWrap;
					
				for ( int j=0; j<cloudSphere.Meshes.Count; j++) {
					var mesh = cloudSphere.Meshes[j];

					rs.SetupVertexInput( cloudVertexBuffers[j], cloudIndexBuffers[j] );
					rs.DrawIndexed( mesh.IndexCount, 0, 0 );
				}
			}

			rs.ResetStates();
		}



		/// <summary>
		/// Gets current Sun direction.
		/// </summary>
		/// <returns></returns>
		public Vector3 GetSunDirection()
		{
			return Params.SunDirection.Normalized();
		}



		Color4 SunColor ( Vector3 dir )
		{
			Color4 dusk		=	new Color4(Temperature.Get(2000), 1);
			Color4 zenith	=	new Color4(Temperature.Get(Params.SunTemperature), 1);

			Vector3 ndir	=	dir.Normalized();

			return Color4.Lerp( dusk, zenith, (float)Math.Pow(ndir.Y, 0.5f) );
		}


		/// <summary>
		/// Gets Sun color.
		/// </summary>
		/// <returns></returns>
		public Color4 GetSunLightColor()
		{
			var sunPos = GetSunDirection();

			return SunColor( sunPos ) * Params.SunLightIntensity;

			/*var zenithColorYxy = perezZenith( Params.SkyTurbidity, sunPos.Y );
			var sunColorYxy = perezSun( Params.SkyTurbidity, sunPos.Y, 10 );
			
			return new Color4( YxyToRGB( sunColorYxy * new Vector3( Params.SunLightIntensity, 1, 1 ) ) * Temperature.Get( Params.SunTemperature ), 1 );*/
		}



		/// <summary>
		/// Gets Sun color.
		/// </summary>
		/// <returns></returns>
		public Color4 GetSunGlowColor()
		{
			var sunPos = GetSunDirection();

			return SunColor( sunPos ) * Params.SunGlowIntensity;

			/*var zenithColorYxy = perezZenith( Params.SkyTurbidity, sunPos.Y );
			var sunColorYxy = perezSun( Params.SkyTurbidity, sunPos.Y, 10 );
			
			return new Color4( YxyToRGB( sunColorYxy * new Vector3( Params.SunGlowIntensity, 1, 1 ) ) * Temperature.Get( Params.SunTemperature ), 1 );*/
		}



		/// <summary>
		/// Gets average sky color.
		/// </summary>
		/// <returns></returns>
		public Color4 GetAmbientLevel()
		{
			var sunPos = GetSunDirection();
			var ambientLight = Vector3.Zero;

			var norm = randVectors.Length;// * 2 * MathUtil.Pi;

			for (int i = 0; i < randVectors.Length; i++) {
				var yxy = perezSky( Params.SkyTurbidity, randVectors[i], sunPos );
				var rgb = YxyToRGB( yxy ) * Temperature.Get( Params.SunTemperature );
				ambientLight += rgb / norm;
			}

			return new Color4(ambientLight,1);
		}
	}
}

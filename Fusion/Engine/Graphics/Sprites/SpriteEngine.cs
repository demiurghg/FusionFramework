using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Core;
using Fusion.Engine.Common;

namespace Fusion.Engine.Graphics {
	public class SpriteEngine : GameModule {

		enum Flags {
			OPAQUE				=	0x0001, 
			ALPHA_BLEND			=	0x0002, 
			ALPHA_BLEND_PREMUL	=	0x0004, 
			ADDITIVE			=	0x0008, 
			SCREEN				=	0x0010, 
			MULTIPLY			=	0x0020, 
			NEG_MULTIPLY		=	0x0040,
		}


		[StructLayout(LayoutKind.Explicit)]
		struct ConstData {
			[FieldOffset( 0)]	public Matrix	Transform;
			[FieldOffset(64)]	public Vector4	ClipRectangle;
			[FieldOffset(80)]	public Color4	MasterColor;
		}

		StateFactory	factory;
		Ubershader		shader;
		GraphicsDevice	device;
		ConstData		constData;
		ConstantBuffer	constBuffer;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ge"></param>
		public SpriteEngine( GraphicsEngine ge ) : base(ge.GameEngine)
		{
			this.device	=	ge.Device;
		}



		/// <summary>
		/// 
		/// </summary>
		public override void Initialize()
		{
			shader		=	device.GameEngine.Content.Load<Ubershader>("sprite");
			factory		=	new StateFactory( shader, typeof(Flags), (ps,i) => StateEnum( ps, (Flags)i) );
			constBuffer	=	new ConstantBuffer( device, typeof(ConstData) );
			constData	=	new ConstData();
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="ps"></param>
		/// <param name="flags"></param>
		void StateEnum ( PipelineState ps, Flags flags )
		{
			if ( flags==Flags.OPAQUE			) ps.BlendState		=	BlendState.Opaque			; else
			if ( flags==Flags.ALPHA_BLEND		) ps.BlendState		=	BlendState.AlphaBlend		; else
			if ( flags==Flags.ALPHA_BLEND_PREMUL) ps.BlendState		=	BlendState.AlphaBlendPremul	; else
			if ( flags==Flags.ADDITIVE			) ps.BlendState		=	BlendState.Additive			; else
			if ( flags==Flags.SCREEN			) ps.BlendState		=	BlendState.Screen			; else
			if ( flags==Flags.MULTIPLY			) ps.BlendState		=	BlendState.Multiply			; else
			if ( flags==Flags.NEG_MULTIPLY		) ps.BlendState		=	BlendState.NegMultiply		; else

			ps.RasterizerState		=	RasterizerState.CullNone;
			ps.DepthStencilState	=	DepthStencilState.None;
			ps.Primitive			=	Primitive.TriangleList;
			ps.VertexInputElements	=	VertexInputElement.FromStructure( typeof(SpriteVertex) );
		}




		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref factory );
				SafeDispose( ref constBuffer );
			}

			base.Dispose( disposing );
		}


		/// <summary>
		/// Draws sprite laters and all sublayers.
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		public void DrawSprites ( GameTime gameTime, StereoEye stereoEye, IEnumerable<SpriteLayer> layers )
		{
			device.ResetStates();
			device.RestoreBackbuffer();

			DrawSpritesRecursive( gameTime, stereoEye, layers, Matrix.Identity, new Color4(1f,1f,1f,1f) );
		}


		/// <summary>
		/// Draw sprite layers
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		/// <param name="layers"></param>
		public void DrawSpritesRecursive ( GameTime gameTime, StereoEye stereoEye, IEnumerable<SpriteLayer> layers, Matrix parentTransform, Color4 parentColor )
		{
			int	w	=	device.DisplayBounds.Width;
			int h	=	device.DisplayBounds.Height;
			var ofs	=	0f;

			var projection = Matrix.OrthoOffCenterRH(ofs, w + ofs, h + ofs, ofs, -9999, 9999);

			var orderedLayers	=	layers.OrderBy( layer => layer.Order );

			foreach ( var layer in orderedLayers ) {
				
				if (!layer.Visible) {
					continue;
				}

				Matrix absTransform	=	parentTransform * layer.Transform;
				Color4 absColor		=	parentColor * layer.Color.ToColor4();

				constData.Transform		=	absTransform * projection;
				constData.ClipRectangle	=	new Vector4(0,0,0,0);
				constData.MasterColor	=	absColor;

				constBuffer.SetData( constData );

				device.VertexShaderConstants[0]	=	constBuffer;
				device.PixelShaderConstants[0]	=	constBuffer;
				
				PipelineState ps = null;
				SamplerState ss = null;

				switch ( layer.FilterMode ) {
					case SpriteFilterMode.PointClamp		: ss = SamplerState.PointClamp;			break;
					case SpriteFilterMode.PointWrap			: ss = SamplerState.PointWrap;			break;
					case SpriteFilterMode.LinearClamp		: ss = SamplerState.LinearClamp;		break;
					case SpriteFilterMode.LinearWrap		: ss = SamplerState.LinearWrap;			break;
					case SpriteFilterMode.AnisotropicClamp	: ss = SamplerState.AnisotropicClamp;	break;
					case SpriteFilterMode.AnisotropicWrap	: ss = SamplerState.AnisotropicWrap;	break;
				}

				switch ( layer.BlendMode ) {
					case SpriteBlendMode.Opaque				: ps = factory[(int)Flags.OPAQUE			]; break;
					case SpriteBlendMode.AlphaBlend			: ps = factory[(int)Flags.ALPHA_BLEND		]; break;
					case SpriteBlendMode.AlphaBlendPremul	: ps = factory[(int)Flags.ALPHA_BLEND_PREMUL]; break;
					case SpriteBlendMode.Additive			: ps = factory[(int)Flags.ADDITIVE			]; break;
					case SpriteBlendMode.Screen				: ps = factory[(int)Flags.SCREEN			]; break;
					case SpriteBlendMode.Multiply			: ps = factory[(int)Flags.MULTIPLY			]; break;
					case SpriteBlendMode.NegMultiply		: ps = factory[(int)Flags.NEG_MULTIPLY		]; break;
				}

				device.PipelineState			=	ps;
				device.PixelShaderSamplers[0]	=	ss;

				layer.Draw( gameTime, stereoEye );

				DrawSpritesRecursive( gameTime, stereoEye, layer.Layers, absTransform, absColor );
			}
		}
	}
}

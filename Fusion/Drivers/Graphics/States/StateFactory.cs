using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Specialized;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using D3D11 = SharpDX.Direct3D11;
using Fusion.Core.Content;
using Fusion.Core.Mathematics;


namespace Fusion.Drivers.Graphics {

	/// <summary>
	/// 
	/// </summary>
	public sealed class StateFactory : GraphicsResource {

		Ubershader		ubershader;
		Dictionary<int, PipelineState>	pipelineStates;


		/// <summary>
		/// Initializes a new instance of the StateFactory
		/// </summary>
		/// <param name="ubershader"></param>
		/// <param name="enumType"></param>
		/// <param name="enumAction"></param>
		public StateFactory ( Ubershader ubershader, Type enumType, Action<PipelineState,int> enumAction ) : base(ubershader.GraphicsDevice)
		{
			this.ubershader		= ubershader;

			Enumerate( enumType, ubershader, enumAction );
		}



		/// <summary>
		/// Initializes a new instance of the StateFactory
		/// </summary>
		/// <param name="device"></param>
		/// <param name="enumType"></param>
		/// <param name="ubershader"></param>
		/// <param name="vertexInputElements"></param>
		/// <param name="blendState"></param>
		/// <param name="rasterizerState"></param>
		public StateFactory ( Ubershader ubershader, Type enumType, Primitive primitive, VertexInputElement[] vertexInputElements ) 
		 : base(ubershader.GraphicsDevice)
		{
			this.ubershader		= ubershader;

			Enumerate( enumType, ubershader, (ps,i) => { ps.VertexInputElements = vertexInputElements; ps.Primitive = primitive; } );
		}


		/// <summary>
		/// Initializes a new instance of the StateFactory
		/// </summary>
		/// <param name="device"></param>
		/// <param name="ubershader"></param>
		public StateFactory ( Ubershader ubershader, Type enumType, Primitive primitive, VertexInputElement[] vertexInputElements, BlendState blendState, RasterizerState rasterizerState )
		 : base(ubershader.GraphicsDevice)
		{
			this.ubershader		= ubershader;

			Enumerate( enumType, ubershader, (ps,i) => { 
					ps.Primitive = primitive;
					ps.VertexInputElements = vertexInputElements; 
					ps.BlendState		=	blendState;
					ps.RasterizerState	=	rasterizerState;
				} );
		}


		/// <summary>
		/// Initializes a new instance of the StateFactory
		/// </summary>
		/// <param name="device"></param>
		/// <param name="ubershader"></param>
		public StateFactory ( Ubershader ubershader, Type enumType, Primitive primitive, VertexInputElement[] vertexInputElements, BlendState blendState, RasterizerState rasterizerState, DepthStencilState depthStencilState )
		 : base(ubershader.GraphicsDevice)
		{
			this.ubershader		= ubershader;

			Enumerate( enumType, ubershader, (ps,i) => { 
					ps.Primitive = primitive;
					ps.VertexInputElements	=	vertexInputElements; 
					ps.BlendState			=	blendState;
					ps.RasterizerState		=	rasterizerState;
					ps.DepthStencilState	=	depthStencilState;
				} );
		}



		/// <summary>
		/// Disposes
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				
				foreach ( var ps  in pipelineStates ) {
					ps.Value.Dispose();
				}

				pipelineStates.Clear();

			}

			base.Dispose( disposing );
		}



		/// <summary>
		/// Gets pipeline state for given combination.
		/// </summary>
		/// <param name="combination"></param>
		/// <returns></returns>
		public PipelineState this[ int combination ] {

			get {
				
				PipelineState ps;

				if (!pipelineStates.TryGetValue( combination, out ps )) {
					var path	=	device.Game.Content.GetPathTo( ubershader );
					var message =	string.Format("Ubershader '{0}' does not contain given combination", path );
					throw new UbershaderException( message, combination, combinerEnum );
				}

				return ps;
			}
		}



		Type combinerEnum;



		/// <summary>
		/// 
		/// </summary>
		/// <param name="?"></param>
		void Enumerate ( Type enumType, Ubershader ubershader, Action<PipelineState,int> enumAction )
		{
			pipelineStates	=	new Dictionary<int,PipelineState>();

			combinerEnum	=	enumType;

			//
			//	validate enum :
			//
			if (Enum.GetUnderlyingType(enumType)!=typeof(int)) {
				throw new ArgumentException("Underlying type should be Int32");
			}

			Dictionary<string,int> enumDict = new Dictionary<string,int>();

			foreach ( var enumValue in Enum.GetValues( enumType ) ) {
				if ( !MathUtil.IsPowerOfTwo( (int)enumValue ) && (int)enumValue!=0 ) {
					throw new ArgumentException("Each value must be zero or power of two");
				}
				enumDict.Add( enumValue.ToString(), (int)enumValue );
			}



			//
			//	Enumerate :
			//
			var defineList = ubershader.Defines;

			foreach ( var defines in defineList ) {
				
				int combination = 0;

				if ( GetCombinerSet( enumDict, defines, out combination ) ) {
					
					var ps = new PipelineState( device );

					ps.PixelShader		=	ubershader.GetPixelShader		( defines );
					ps.VertexShader		=	ubershader.GetVertexShader		( defines );
					ps.GeometryShader	=	ubershader.GetGeometryShader	( defines );
					ps.HullShader		=	ubershader.GetHullShader		( defines );
					ps.DomainShader		=	ubershader.GetDomainShader		( defines );
					ps.ComputeShader	=	ubershader.GetComputeShader		( defines );
					
					enumAction( ps, combination );

					pipelineStates.Add( combination, ps );
				}	
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="delList"></param>
		/// <returns></returns>
		bool GetCombinerSet ( Dictionary<string,int> enumDict, string defList, out int combination )
		{				
			var defs	=	defList.Split( new[]{' ','\t'}, StringSplitOptions.RemoveEmptyEntries );
			combination	=	0;

			foreach ( var def in defs ) {
				if ( !enumDict.ContainsKey(def) ) {
					return false;
				}

				combination |= enumDict[ def ];
			}

			return true;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		string GetEnumName ( int value )
		{
			return Enum.GetName( combinerEnum, value );
		}
				


		/// <summary>
		/// 
		/// </summary>
		/// <param name="combination"></param>
		/// <returns></returns>
		string GetDefinitionsByCombination ( int combination )
		{
			List<string> defs = new List<string>();

            for (int i=0; i<32; i++) {
				int bit = 1<<i;
				
				if ( (bit & combination) != 0 ) {
					defs.Add( GetEnumName(bit) );					
				}
            }

			return "[" + string.Join( " ", defs.ToArray() ) + "]";
		}
	}
}

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
using Fusion.Content;
using Fusion.Mathematics;


namespace Fusion.Graphics {


	public partial class Ubershader : DisposableBase {

		protected readonly GraphicsDevice	device;

		class UsdbEntry {

			public string Defines;
			public ShaderBytecode PixelShader;
			public ShaderBytecode VertexShader;
			public ShaderBytecode GeometryShader;
			public ShaderBytecode HullShader;
			public ShaderBytecode DomainShader;
			public ShaderBytecode ComputeShader;


			public UsdbEntry ( string defines, byte[] ps, byte[] vs, byte[] gs, byte[] hs, byte[] ds, byte[] cs ) 
			{
				this.Defines		=	defines;
				this.PixelShader	=	NullOrShaderBytecode( ps );
				this.VertexShader	=	NullOrShaderBytecode( vs );
				this.GeometryShader	=	NullOrShaderBytecode( gs );
				this.HullShader		=	NullOrShaderBytecode( hs );
				this.DomainShader	=	NullOrShaderBytecode( ds );
				this.ComputeShader	=	NullOrShaderBytecode( cs );
			}


			ShaderBytecode NullOrShaderBytecode ( byte[] array )
			{
				if (array.Length==0) {
					return null;
				}
				return new ShaderBytecode( array );;
			}
		}


		Dictionary<string,UsdbEntry>	database = new Dictionary<string,UsdbEntry>();


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="rs"></param>
		/// <param name="path"></param>
		/// <param name="combinerEnum"></param>
		public Ubershader ( GraphicsDevice device, Stream stream )
		{
			this.device			=	device;
			Recreate( stream );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		void Recreate ( Stream stream )
		{
			database.Clear();

			using ( var br = new BinaryReader( stream ) ) {

				br.ExpectMagic("USDB", "ubershader");

				var count = br.ReadInt32();

				for (int i=0; i<count; i++) {
					var defines		=	br.ReadString();
					int length;

					br.ExpectMagic("PSBC", "ubershader");
					length	=	br.ReadInt32();
					var ps	=	br.ReadBytes( length );

					br.ExpectMagic("VSBC", "ubershader");
					length	=	br.ReadInt32();
					var vs	=	br.ReadBytes( length );

					br.ExpectMagic("GSBC", "ubershader");
					length	=	br.ReadInt32();
					var gs	=	br.ReadBytes( length );

					br.ExpectMagic("HSBC", "ubershader");
					length	=	br.ReadInt32();
					var hs	=	br.ReadBytes( length );

					br.ExpectMagic("DSBC", "ubershader");
					length	=	br.ReadInt32();
					var ds	=	br.ReadBytes( length );

					br.ExpectMagic("CSBC", "ubershader");
					length	=	br.ReadInt32();
					var cs	=	br.ReadBytes( length );

					//Log.Message("{0}", profile );
					//PrintSignature( bytecode, "ISGN" );
					//PrintSignature( bytecode, "OSGN" );
					//PrintSignature( bytecode, "OSG5" );

					database.Add( defines, new UsdbEntry( defines, ps, vs, gs, hs, ds, cs ) );
				}
			}
		}



		/// <summary>
		/// Gets all defines
		/// </summary>
		public ICollection<string>	Defines {
			get {
				return database.Select( dbe => dbe.Key ).ToArray();
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public ShaderBytecode GetPixelShader( string key = "" )
		{
			return ( database[key].PixelShader );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public ShaderBytecode GetVertexShader( string key = "" )
		{
			return ( database[key].VertexShader );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public ShaderBytecode GetGeometryShader( string key = "" )
		{
			return ( database[key].GeometryShader );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public ShaderBytecode GetHullShader( string key = "" )
		{
			return ( database[key].HullShader );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public ShaderBytecode GetDomainShader( string key = "" )
		{
			return ( database[key].DomainShader );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public ShaderBytecode GetComputeShader( string key = "" )
		{
			return ( database[key].ComputeShader );
		}






		#if false

		/// <summary>
		/// 
		/// </summary>
		/// <param name="enumType"></param>
		public void Map ( Type enumType )
		{
			computeShaders	.Clear();
			geometryShaders	.Clear();
			pixelShaders	.Clear();
			vertexShaders	.Clear();
			hullShaders		.Clear();
			domainShaders	.Clear();


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
			//	Create shaders :	
			//
			foreach ( var entry in database ) {
				
				int combination = 0;

				if ( GetCombinerSet( enumDict, entry.Defines, out combination ) ) {

					if ( entry.Target=="PixelShader" ) {
						pixelShaders.Add( combination, new ShaderBytecode( entry.Bytecode ) );
					} else

					if ( entry.Target=="VertexShader" ) {
						vertexShaders.Add( combination, new ShaderBytecode( entry.Bytecode ) );
						//var bc = new ShaderBytecode(entry.Bytecode);
					} else 

					if ( entry.Target=="GeometryShader" ) {
						geometryShaders.Add( combination, new ShaderBytecode( entry.Bytecode ) );
					} else 

					if ( entry.Target=="ComputeShader" ) {
						computeShaders.Add( combination, new ShaderBytecode( entry.Bytecode ) );
					} else 

					if ( entry.Target=="DomainShader" ) {
						domainShaders.Add( combination, new ShaderBytecode( entry.Bytecode ) );
					} else 

					if ( entry.Target=="HullShader" ) {
						hullShaders.Add( combination, new ShaderBytecode( entry.Bytecode ) );
					} else {

						throw new InvalidOperationException("Bad ubershader target: " + entry.Target);
					}

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



		/// <summary>
		/// Dispose uber shader
		/// </summary>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
			}
			base.Dispose(disposing);
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="shaderType"></param>
		/// <param name="combination"></param>
		void BadCombination ( string shaderType, int combination )
		{
			var path	=	device.Game.Content.GetPathTo( this );
			var message =	string.Format("Ubershader '{0}' does not contain {1} for given combination", path, shaderType );
			throw new UbershaderException( message, combination, combinerEnum );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="combination"></param>
		public ShaderBytecode GetPixelShader ( int combination )
		{ 
			ShaderBytecode shader;

			if ( !pixelShaders.TryGetValue( combination, out shader ) ) {
				BadCombination( "pixel shader", combination );
			}

			return	shader;
		}
		

		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="combination"></param>
		/// <param name="signature"></param>
		public ShaderBytecode GetVertexShader ( int combination )	
		{ 
			ShaderBytecode shader;

			if ( !vertexShaders.TryGetValue( combination, out shader ) ) {
				BadCombination( "vertex shader", combination );
			}

			return	shader;
		}
				   


		/// <summary>
		/// 
		/// </summary>
		/// <param name="combination"></param>
		public ShaderBytecode GetGeometryShader ( int combination )
		{ 
			ShaderBytecode shader;

			if ( !geometryShaders.TryGetValue( combination, out shader ) ) {
				BadCombination( "geometry shader", combination );
			}

			return	shader;
		}
		


		/// <summary>
		/// 
		/// </summary>
		/// <param name="combination"></param>
		public ShaderBytecode GetComputeShader ( int combination )
		{ 
			ShaderBytecode shader;

			if ( !computeShaders.TryGetValue( combination, out shader ) ) {
				BadCombination( "compute shader", combination );
			}

			return	shader;
		}
		
		

		/// <summary>
		/// 
		/// </summary>
		/// <param name="combination"></param>
		public ShaderBytecode GetDomainShader ( int combination )
		{ 
			ShaderBytecode shader;

			if ( !domainShaders.TryGetValue( combination, out shader ) ) {
				BadCombination( "domain shader", combination );
			}

			return	shader;
		}
		
		

		/// <summary>
		/// 
		/// </summary>
		/// <param name="combination"></param>
		public ShaderBytecode GetHullShader ( int combination )
		{ 
			ShaderBytecode shader;

			if ( !hullShaders.TryGetValue( combination, out shader ) ) {
				BadCombination( "hull shader", combination );
			}

			return	shader;
		}
		#endif
	}
}

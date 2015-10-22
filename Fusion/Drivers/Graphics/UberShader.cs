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
using Fusion.Core;
using Fusion.Core.Content;
using Fusion.Core.Mathematics;


namespace Fusion.Drivers.Graphics {


	public partial class Ubershader : GraphicsResource {


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
		public Ubershader ( GraphicsDevice device, Stream stream ) : base(device)
		{
			Recreate( stream );
		}



		/// <summary>
		/// Resets all data and recreates UberShader
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
		/// Gets PixelShader
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public ShaderBytecode GetPixelShader( string key = "" )
		{
			return ( database[key].PixelShader );
		}



		/// <summary>
		/// Gets VertexShader
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public ShaderBytecode GetVertexShader( string key = "" )
		{
			return ( database[key].VertexShader );
		}



		/// <summary>
		/// Gets GeometryShader
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public ShaderBytecode GetGeometryShader( string key = "" )
		{
			return ( database[key].GeometryShader );
		}



		/// <summary>
		/// Gets HullShader
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public ShaderBytecode GetHullShader( string key = "" )
		{
			return ( database[key].HullShader );
		}



		/// <summary>
		/// Gets DomainShader
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public ShaderBytecode GetDomainShader( string key = "" )
		{
			return ( database[key].DomainShader );
		}



		/// <summary>
		/// Gets ComputeShader
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public ShaderBytecode GetComputeShader( string key = "" )
		{
			return ( database[key].ComputeShader );
		}
	}
}

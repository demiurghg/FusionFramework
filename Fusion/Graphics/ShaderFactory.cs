using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3D11 = SharpDX.Direct3D11;
using SharpDX.Direct3D11;

namespace Fusion.Graphics {

	public class AbstractShader {
		public readonly string Bytecode;
		internal AbstractShader( string bytecode ) {
			this.Bytecode = bytecode;
		}
	}

	/// <summary>
	/// Compute shader
	/// </summary>
	public class ComputeShader : AbstractShader {
		internal readonly D3D11.ComputeShader Shader;

		internal ComputeShader ( GraphicsDevice device, string bytecode ) : base(bytecode) { 
			Shader = new D3D11.ComputeShader( device.Device, Misc.HexStringToByte(bytecode) ); 
		}
	}


	/// <summary>
	/// Domain shader
	/// </summary>
	public class DomainShader : AbstractShader {
		internal readonly D3D11.DomainShader Shader;		

		internal DomainShader ( GraphicsDevice device, string bytecode ) : base(bytecode) { 
			Shader = new D3D11.DomainShader( device.Device, Misc.HexStringToByte(bytecode) ); 
		}
	}


	/// <summary>
	/// Geometry shader
	/// </summary>
	public class GeometryShader : AbstractShader {
		
		internal readonly D3D11.GeometryShader Shader;		

		internal GeometryShader ( GraphicsDevice device, string bytecode ) : base(bytecode) { 
			Shader = new D3D11.GeometryShader( device.Device, Misc.HexStringToByte(bytecode) ); 
		}
	}


	/// <summary>
	/// Hull shader
	/// </summary>
	public class HullShader : AbstractShader {
		internal readonly D3D11.HullShader Shader;		
		
		internal HullShader ( GraphicsDevice device, string bytecode ) : base(bytecode) { 
			Shader = new D3D11.HullShader( device.Device, Misc.HexStringToByte(bytecode) ); 
		}
	}


	/// <summary>
	/// Pixel shader
	/// </summary>
	public class PixelShader : AbstractShader {
		internal readonly D3D11.PixelShader	Shader;		

		internal PixelShader ( GraphicsDevice device, string bytecode ) : base(bytecode) { 
			Shader = new D3D11.PixelShader( device.Device, Misc.HexStringToByte(bytecode) ); 
		}
	}


	/// <summary>
	/// Vertex shader.
	/// </summary>
	public class VertexShader : AbstractShader {
		internal readonly D3D11.VertexShader Shader;		
		
		internal VertexShader ( GraphicsDevice device, string bytecode ) : base(bytecode) { 
			Shader = new D3D11.VertexShader( device.Device, Misc.HexStringToByte(bytecode) ); 
		}
	}


	/// <summary>
	/// Caches and produce shaders by its bytecode
	/// </summary>
	internal class ShaderFactory : DisposableBase {				  

		readonly GraphicsDevice device;

		readonly Dictionary<string, PixelShader>	psDictionary	=	new Dictionary<string, PixelShader>		();
		readonly Dictionary<string, VertexShader>	vsDictionary	=	new Dictionary<string, VertexShader>	();
		readonly Dictionary<string, GeometryShader>	gsDictionary	=	new Dictionary<string, GeometryShader>	();
		readonly Dictionary<string, ComputeShader>	csDictionary	=	new Dictionary<string, ComputeShader>	();
		readonly Dictionary<string, DomainShader>	dsDictionary	=	new Dictionary<string, DomainShader>	();
		readonly Dictionary<string, HullShader>		hsDictionary	=	new Dictionary<string, HullShader>		();


		public ShaderFactory ( GraphicsDevice device )
		{
			this.device	=	device;
		}



		public PixelShader	GetPixelShader ( string bytecode )
		{
			PixelShader	shader;

			if (!psDictionary.TryGetValue( bytecode, out shader ) ) {
				shader = new PixelShader( device, bytecode );
				psDictionary.Add( bytecode, shader ); 
			}

			return shader;
		}



		public VertexShader	GetVertexShader ( string bytecode )
		{
			VertexShader	shader;

			if (!vsDictionary.TryGetValue( bytecode, out shader ) ) {
				shader = new VertexShader( device, bytecode );
				vsDictionary.Add( bytecode, shader ); 
			}

			return shader;
		}



		public GeometryShader	GetGeometryShader ( string bytecode )
		{
			GeometryShader	shader;

			if (!gsDictionary.TryGetValue( bytecode, out shader ) ) {
				shader = new GeometryShader( device, bytecode );
				gsDictionary.Add( bytecode, shader ); 
			}

			return shader;
		}



		public ComputeShader	GetComputeShader ( string bytecode )
		{
			ComputeShader	shader;

			if (!csDictionary.TryGetValue( bytecode, out shader ) ) {
				shader = new ComputeShader( device, bytecode );
				csDictionary.Add( bytecode, shader ); 
			}

			return shader;
		}



		public DomainShader	GetDomainShader ( string bytecode )
		{
			DomainShader	shader;

			if (!dsDictionary.TryGetValue( bytecode, out shader ) ) {
				shader = new DomainShader( device, bytecode );
				dsDictionary.Add( bytecode, shader ); 
			}

			return shader;
		}



		public HullShader	GetHullShader ( string bytecode )
		{
			HullShader	shader;

			if (!hsDictionary.TryGetValue( bytecode, out shader ) ) {
				shader = new HullShader( device, bytecode );
				hsDictionary.Add( bytecode, shader ); 
			}

			return shader;
		}



		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				foreach ( var s in psDictionary	 ) s.Value.Shader.Dispose();
				foreach ( var s in vsDictionary	 ) s.Value.Shader.Dispose();
				foreach ( var s in gsDictionary	 ) s.Value.Shader.Dispose();
				foreach ( var s in csDictionary	 ) s.Value.Shader.Dispose();
				foreach ( var s in dsDictionary	 ) s.Value.Shader.Dispose();
				foreach ( var s in hsDictionary	 ) s.Value.Shader.Dispose();
			}
			base.Dispose( disposing );
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="mapping"></param>
		/// <returns></returns>
		static internal StreamOutputElement[] ParseMapping ( string mapping, out int stride )
		{
			var maps = mapping.Split(new[]{';'});

			var list = new List<StreamOutputElement>(maps.Length);

			stride = 0;

			foreach ( var map in maps ) {

				var nameComp	=	map.Split(new[]{'.'});

				if (nameComp.Length!=2) {
					throw new ArgumentException(string.Format("Bad mapping: {0}", mapping));
				}

				var name = nameComp[0];
				var comp = nameComp[1].ToLowerInvariant();

				int index   = (int)char.GetNumericValue( name, name.Length-1 );

				if (index<0) {
					index = 0;
				} else {
					name  = name.Substring(0, name.Length-1);
				}

				byte start	= 0;
				byte count	= 0;

				if (comp=="x")		{ start = 0; count	= 1; } else
				if (comp=="y")		{ start = 1; count	= 1; } else
				if (comp=="z")		{ start = 2; count	= 1; } else
				if (comp=="w")		{ start = 3; count	= 1; } else
				if (comp=="xy")		{ start = 0; count	= 2; } else
				if (comp=="yz")		{ start = 1; count	= 2; } else
				if (comp=="zw")		{ start = 2; count	= 2; } else
				if (comp=="xyz")	{ start = 0; count	= 3; } else
				if (comp=="yzw")	{ start = 1; count	= 3; } else
				if (comp=="xyzw")	{ start = 0; count	= 4; } else {
					throw new ArgumentException(string.Format("Bad mapping: {0}", mapping));
				}

				stride += (count * 4);

				//Log.Message("{0} {1} {2} {3}", name, index, start, count );
				list.Add( new StreamOutputElement(0, name, index, (byte)start, (byte)count, 0) );
			}
			
			return list.ToArray();
		}



	}
}
																				  
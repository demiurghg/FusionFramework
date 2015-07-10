using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Shell;
using SharpDX.D3DCompiler;
using System.Diagnostics;


namespace UberFxc {

	public class Parameters {

		public Parameters ()
		{
			DefineEnum = new List<string>();

			Optimization	=	1;

			VSEntry =	"VSEntry";
			GSEntry =	"GSEntry";
			DSEntry =	"DSEntry";
			HSEntry =	"HSEntry";
			PSEntry =	"PSEntry";
			CSEntry =	"CSEntry";
		}
		
		[CommandLineParser.Required]
		public string Input { get; set; }
		
		[CommandLineParser.Required]
		public string Output { get; set; }
		
		
		[CommandLineParser.Name("nologo", "suppress copyright message")]
		public bool NoLogo { get; set; }
		
		[CommandLineParser.Name("mt", "use parallel compiling")]
		public bool Multithreaded { get; set; }



		[CommandLineParser.Name("prm", "pack matrices in row-major order")]
		public bool PackRowMajor { get; set; }
		
		[CommandLineParser.Name("pcm", "pack matrices in column-major order")]
		public bool PackColumnMajor { get; set; }

		[CommandLineParser.Name("opt", "optimization level 0..3. 1 is default")]
		public int Optimization { get; set; }

		[CommandLineParser.Name("pfc", "prefer flow control")]
		public bool PreferFlowControl { get; set; }
		
		[CommandLineParser.Name("afc", "avoid flow control")]
		public bool AvoidFlowControl { get; set; }
		


		[CommandLineParser.Name("def", "definition permutation rule")]
		public List<string> DefineEnum { get; set; }

		[CommandLineParser.Name("vi", "show generated permutations")]
		public string ShowPermutations { get; set; }
		
		[CommandLineParser.Name("ls", "output HTML listing")]
		public string ListingPath { get; set; }
		

		
		[CommandLineParser.Name("vse", "vertex shader entry point. VSMain is default")]
		public string VSEntry { get; set; }

		[CommandLineParser.Name("gse", "geometry shader entry point. GSMain is default")]
		public string GSEntry { get; set; }

		[CommandLineParser.Name("dse", "domain shader entry point. DSMain is default")]
		public string DSEntry { get; set; }

		[CommandLineParser.Name("hse", "hull shader entry point. HSMain is default")]
		public string HSEntry { get; set; }

		[CommandLineParser.Name("pse", "pixel shader entry point. PSMain is default")]
		public string PSEntry { get; set; }

		[CommandLineParser.Name("cse", "compute shader entry point. CSMain is default")]
		public string CSEntry { get; set; }
	}



	class UsdbEntry {

		public string Defines;
		public byte[] PSBytecode;
		public byte[] VSBytecode;
		public byte[] GSBytecode;
		public byte[] HSBytecode;
		public byte[] DSBytecode;
		public byte[] CSBytecode;

		public UsdbEntry ( string defines, byte[] ps, byte[] vs, byte[] gs, byte[] hs, byte[] ds, byte[] cs ) 
		{
			this.Defines	=	defines;
			this.PSBytecode	=	ps;
			this.VSBytecode	=	vs;
			this.GSBytecode	=	gs;
			this.HSBytecode	=	hs;
			this.DSBytecode	=	ds;
			this.CSBytecode	=	cs;
		}
	}


	

	class Program {
		static int Main ( string[] args )
		{	
			Trace.Listeners.Add( new ColoredTraceListener2() );

			var param  = new Parameters();
			var parser = new CommandLineParser( param );

			if (!parser.ParseCommandLine( args )) {
				return 1;
			}

			if (!param.NoLogo) {
				Log.Message("Fusion Framework Ubershader Compiler");
				Log.Message("");
			}





			return 0;
		}
	}
}

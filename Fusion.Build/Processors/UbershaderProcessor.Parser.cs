using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using Fusion.Core.Mathematics;


namespace Fusion.Build.Processors {
	
	enum Target {
		None,
		PixelShader,
		VertexShader,
		GeometryShader,
		HullShader,
		DomainShader,
		ComputeShader
	}

	public partial class UbershaderProcessor {

		#if false
			Expression	::=		'$pixel'|'$vertex'|'$geometry'|'$compute'|'$domain'|'$hull' Combination
			Combination	::=		Sequence (' '+ Sequence)*
			Sequence	::=		[+] Exclusion ('..' Exclusion)*
			Exclusion	::=		Factor ('|' Factor)*
			Factor		::=		Definition | "(" Combination ")"
			Definition	::=		IdentChar+
		#endif


		CharStream cs;


		enum Operation {
			Combination,
			Sequence,
			Exclusion,
			Definition,
		}


		class Node {
			public bool			Optional	= false;
			public Operation	Operation	= Operation.Combination;
			public string		Definition	= null;
			public List<Node>	Nodes		= new List<Node>();


			public Node ( string define )
			{
				Operation	=	Operation.Definition;
				Definition	=	define;
			}


			public Node ( Operation op, bool optional ) 
			{
				Operation	=	op;
				Optional	=	optional;
			}


			public void Add ( Node node ) 
			{
				Nodes.Add( node );
			}


			public List<string> Enumerate ()
			{
				var R = new List<string>();

				if ( Optional ) {
					R.Add("");
				}

				if ( Operation==Operation.Combination ) {
					
					foreach (var child in Nodes) {	
						R = Combine( R, child.Enumerate() );
					}

				} else if ( Operation==Operation.Sequence ) {
					
					var Q = new List<string>();

					foreach (var child in Nodes) {	
						Q = Combine( Q, child.Enumerate() );
						R.AddRange( Q );
					}

				} else if ( Operation==Operation.Exclusion ) {
					
					foreach (var child in Nodes) {	
						R.AddRange( child.Enumerate() );
					}

				} else if ( Operation==Operation.Definition ) {
					R.Add( Definition );
				}

				return R;
			}
		}



		/// <summary>
		/// Sorts words and removed duplicates
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		static string CleanupString ( string str )
		{
			//return str;
			var words = str
				.Split(new[]{' '}, StringSplitOptions.RemoveEmptyEntries)
				//.OrderBy( w => w )
				.Distinct()
				.ToArray();

			return string.Join(" ", words);
		}



		/// <summary>
		/// Removed duplicates
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		static List<string> CleanupList ( List<string> list )
		{
			return list.ToList();
			//return list.OrderBy( w => w ).Distinct().ToList();
		}



		/// <summary>
		/// Combines define sets
		/// </summary>
		/// <param name="A"></param>
		/// <param name="B"></param>
		/// <returns></returns>
		static List<string> Combine ( List<string> A, List<string> B )
		{
			var C = new List<string>();

			if ( !A.Any() ) return B;
			if ( !B.Any() ) return A;

			foreach ( var a in A ) {
				foreach ( var b in B ) {	
					C.Add( CleanupString( a + " " + b ) );
				}
			}

			return CleanupList( C );
		}


		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="line"></param>
		List<string> Parse ( string line, out Target target )
		{
			cs	=	new CharStream( line );

			var root =	Expression( out target );
			var list =	root.Enumerate();
			return list;
		}


		/*-----------------------------------------------------------------------------------------------
		 * 
		 *	Recursive Descent Parser Stuff :
		 * 
		-----------------------------------------------------------------------------------------------*/

		/// <summary>
		/// 
		/// </summary>
		Node Expression ( out Target target )
		{
			target = Target.None;
			if ( cs.Accept( "$pixel"	) ) target = Target.PixelShader		;
			if ( cs.Accept( "$vertex"	) ) target = Target.VertexShader	;
			if ( cs.Accept( "$geometry"	) ) target = Target.GeometryShader	;
			if ( cs.Accept( "$domain"	) ) target = Target.DomainShader	;
			if ( cs.Accept( "$hull"		) ) target = Target.HullShader		;
			if ( cs.Accept( "$compute"	) ) target = Target.ComputeShader	;

			if (target!=Target.None) {
				
				if (cs.AcceptSpace()) {
					return Combination(); 
				} else {
					return new Node("");
				}

			} else { 
				throw new BuildException("Bad shader type"); 
			}
		}


		Node Combination()
		{
			var node = new Node( Operation.Combination, false );

			node.Add( Sequence() );
			while (cs.AcceptSpace()) {
				node.Add( Sequence() );
			}
			
			return node;
		}


		Node Sequence()
		{
			var node = new Node( Operation.Sequence, cs.Accept("+") );

			node.Add( Exclusion() );
			while (cs.Accept("..")) {
				node.Add( Exclusion() );
			}
			
			return node;
		}


		Node Exclusion ()
		{
			var node = new Node( Operation.Exclusion, false );

			node.Add( Factor() );
			while (cs.Accept("|")) {
				node.Add( Factor() );
			}
			
			return node;
		}


		Node Factor ()
		{
			if ( cs.Accept("(") ) {
				var node = Combination();
				cs.Expect(")");
				return node;
			} else {
				return Definition();
			}
		}


		Node Definition ()
		{
			var s = cs.ExpectIdent();
			return new Node( s );
		}
	}
}

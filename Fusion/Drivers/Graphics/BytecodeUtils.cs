using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion.Core.Mathematics;

namespace Fusion.Drivers.Graphics {
	internal class BytecodeUtils {

		public const int	SV_Position		=	1;
		public const int	SV_VertexID		=	6;
		public const int	SV_InstanceID	=	8;

		public const int	Format_Float	=	3;
		public const int	Format_Integer	=	2;


		public class SignatureEntry {
			public string	Name;
			public int		Index;
			public int		Mask;
			public int		Register;
			public int		SysValue;
			public int		Format;
			public int		OsgM;

			public int SOComponentCount {
				get {
					return MathUtil.SparseBitcount(Mask & 0xFF);
				}
			}

			public override string ToString ()
			{
				return string.Format(
					"n:{0,-20} i:{1,1} r:{2,2:00} sv:{3,-2} f:{4,-2} rm:{5,8:X8} m{6}", 
					Name, Index, Register, SysValue, Format, Mask, OsgM);
			}
		}



		/// <summary>
		/// Reads shader signature from bytecode.
		/// </summary>
		/// <param name="bytecode">Bytecode.</param>
		/// <param name="token">Signature token: Possible values are 'ISGN', 'OSGN', 'OSG5'.</param>
		/// <returns>Array if signature entries.</returns>	   
		public static SignatureEntry[] ParseSignature ( byte[] bytecode, string token )
		{
			int length	=	0;
			var start	=	GetTokenPosition( bytecode, token, out length );

			if (start<0) {
				return null;
			}

			start += 4; // move start behind token.

			SignatureEntry[] sig = null;

			using ( var ms	=	new MemoryStream( bytecode, start, bytecode.Length - start ) ) {
				using ( var sr	=	new BinaryReader( ms ) ) {

					int sigSize		=	sr.ReadInt32();
					int sigCount	=	sr.ReadInt32();	
					int dummy		=	sr.ReadInt32();	//	always 8

					//Log.Message(" - start:{3} sz:{0} cnt:{1} dummy:{2}", sigSize, sigCount, dummy, start );

					sig	=	new SignatureEntry[ sigCount ];

					for ( int i=0; i<sigCount; i++ ) {
				
						var sigEntry =	new SignatureEntry();

						if (token=="OSG5") {
							sigEntry.OsgM		=	sr.ReadInt32();
							sigEntry.Name		=	ReadNullTermASCIIString( bytecode, start + sr.ReadInt32() + 4 );
							sigEntry.Index		=	sr.ReadInt32();
							sigEntry.SysValue	=	sr.ReadInt32();
							sigEntry.Format		=	sr.ReadInt32();
							sigEntry.Register	=	sr.ReadInt32();
							sigEntry.Mask		=	sr.ReadInt32();
						} else {
							sigEntry.Name		=	ReadNullTermASCIIString( bytecode, start + sr.ReadInt32() + 4 );
							sigEntry.Index		=	sr.ReadInt32();
							sigEntry.SysValue	=	sr.ReadInt32();
							sigEntry.Format		=	sr.ReadInt32();
							sigEntry.Register	=	sr.ReadInt32();
							sigEntry.Mask		=	sr.ReadInt32();
						}

						sig[i] = sigEntry;
					}
				}
			}

			return sig;
		} 



		/// <summary>
		/// 
		/// </summary>
		/// <param name="bytecode"></param>
		/// <param name="start"></param>
		/// <returns></returns>
		static string ReadNullTermASCIIString ( byte[] bytecode, int start )
		{
			List<byte> bytes = new List<byte>();

			int scan = start;

			while (bytecode[scan]!=0) {
				bytes.Add( bytecode[scan] );
				scan++;
			}

			return Encoding.ASCII.GetString( bytes.ToArray() );
		}



		/// <summary>
		/// Gets token position and subsection size.
		/// </summary>
		/// <param name="bc">Bytecode array</param>
		/// <param name="token">Token</param>
		/// <param name="length">Length of subsection. This values does not include token and subsection length (both 8 bytes).</param>
		/// <returns>Position of token.</returns>
		static int GetTokenPosition ( byte[] bc, string token, out int length )
		{
			if (token.Length!=4) {
				throw new ArgumentException("token must contain 4 characters");
			}

			int pos = -1;
			
			for (int i=0; i<bc.Length-4; i++) {
				if ( bc[i+0]==token[0] && bc[i+1]==token[1] && 
				     bc[i+2]==token[2] && bc[i+3]==token[3] ) 
				{
					pos = i;
					break;
				}
			}

			length = (bc[pos+4]) + (bc[pos+5]<<8) + (bc[pos+6]<<16) + (bc[pos+7]<<24);

			return pos;
		}

	}
}

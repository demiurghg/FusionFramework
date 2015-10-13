using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion;

namespace FBuild {
	class ContentParser {


		/// <summary>
		/// 
		/// Grammar:
		///		content = { line }
		///		line	= comment | header | entry "\r\n"|"\r"|"\n"
		///		comment	= "#" { anychar }
		///		header	= "[" {alpha} "]"
		///		entry	= path {option} 
		///		path	= {alpha|digit|"\"|"/"|"."|"*"|"?"}
		///		option	= "-" {alpha|digit} [ ":" string|ident|number ]
		///		number	= [-] digit {digit} [ "." digit {digit} ]
		///		ident	= alpha {alpha|digit}
		///		string	= """ {alpha|digit|space|punct} """
		/// 
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="inputDirectory"></param>
		/*public ContentParser ( string fileName, string inputDirectory )
		{
			data		=	new Dictionary<string,List<lines>>();

			var lines	=	File.ReadAllLines(fileName)
							.Select( line1 => line1.Trim(' ', '\t') )
							.ToArray();
			
			foreach ( var line in lines ) {
				
				if ( IsCommentOrEmpty( line ) ) {
					
					ProcessCommentOrEmpty( line );

				} else if ( IsSectionName( line ) ) {
					
					ProcessSectionName( line );
				
				} else {
					
					ProcessContentDeclaration( line );

				}
			}

		}  */
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion {

	public class CharStream {

		string stream;
		int ptr;

		/// <summary>
		/// Creates char stream from string.
		/// </summary>
		/// <param name="s"></param>
		public CharStream ( string s )
		{
			stream = s;
			ptr = 0;
		}


		/// <summary>
		/// Peeks char.
		/// </summary>
		/// <param name="count"></param>
		/// <returns></returns>
		public char Peek ( int count = 0 )
		{
			if ( ptr + count >= stream.Length ) {
				return '\0';
			} else {
				return stream[ ptr + count ];
			}
		}


		/// <summary>
		///	Reads char from stream, advances pointer.
		/// </summary>
		/// <returns></returns>
		public char Read ()
		{
			char ch = Peek(0);
			ptr++;
			return ch;
		}


		/// <summary>
		/// Read char stream while predicate 
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public string ReadWhile ( Func<char,bool> predicate )
		{
			StringBuilder sb = new StringBuilder();

			while ( predicate( Peek() ) ) {
				sb.Append( Read() );
			}

			return sb.ToString();
		}


		/// <summary>
		/// Expect identifier
		/// </summary>
		/// <returns></returns>
		public string ExpectIdent ()
		{
			var s = ReadWhile( ch => Char.IsLetter(ch) || Char.IsNumber(ch) || ch=='_' );
			if (string.IsNullOrEmpty(s)) {
				throw new InvalidOperationException("identifier expected");
			}
			return s;
		}


		/// <summary>
		/// Accepts string
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public bool Accept ( string s )
		{
			var substr = stream.Substring( ptr );

			if (substr.IndexOf(s)==0) {
				ptr += s.Length;
				return true;
			}

			return false;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="s"></param>
		public void Expect ( string s )
		{
			if (!Accept(s)) {
				throw new InvalidOperationException(string.Format("'{0}' expected", s ));
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool AcceptSpace ()
		{
			var s = ReadWhile( ch => ch==' ' || ch=='\t' );
			return !string.IsNullOrEmpty(s);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public void ExpectSpace ()
		{
			if (!AcceptSpace()) {
				throw new InvalidOperationException("whitespace expected");
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public bool Match ( string s )
		{
			return Accept(s);
		}
	}

}

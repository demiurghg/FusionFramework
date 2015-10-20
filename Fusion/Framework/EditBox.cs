using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;

namespace Fusion.Framework {
	internal class EditBox {

		StringBuilder text;
		int cursor;

		List<string> history = new List<string>();
		int historyCursor = 0;


		public string Text {
			get {
				return text.ToString();
			}
			set {
				text.Clear();
				text.Append( value );
				cursor = text.Length;
			}
		}


		public int Cursor {
			get {
				return cursor;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		public EditBox ()
		{
			text	=	new StringBuilder();
			cursor	=	0;
		}



		public void TypeChar ( char ch )
		{						
			if (cursor>=text.Length) {
				text.Append(ch);
				cursor++;
			} else {
				text.Insert( cursor, ch );
				cursor++;
			}
		}


		public void Enter ()
		{
			if (Text!="") {
				history.Insert( 0, Text );
				historyCursor = -1;
				Text = "";
			}
		}


		public void Tab ()
		{
		}


		public void Backspace ()
		{
			if (cursor>0) {
				text.Remove(cursor-1,1);
				cursor--;
			}
		}


		public void Delete ()
		{
			if (cursor<text.Length) {
				text.Remove(cursor,1);
			}
		}



		public void Move ( int value )
		{
			cursor += value;
			cursor = MathUtil.Clamp(cursor, 0, text.Length);
		}



		public void Prev ()
		{
			if (!history.Any()) {
				return;
			}
			historyCursor = MathUtil.Clamp( historyCursor + 1, 0, history.Count - 1 );
			Text = history[ historyCursor ];
		}


		public void Next ()
		{
			if (!history.Any()) {
				return;
			}
			historyCursor = MathUtil.Clamp( historyCursor - 1, 0, history.Count - 1 );
			Text = history[ historyCursor ];
		}

	}
}

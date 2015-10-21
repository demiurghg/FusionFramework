using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Common;


namespace Fusion.Core.Shell {
	public partial class Invoker {

		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <param name="candidates"></param>
		/// <returns></returns>
		public string AutoComplete ( string input )
		{
			if (string.IsNullOrWhiteSpace(input)) {
				return "";
			}

			string output = input;

			var cmd  =	input.Trim().ToLower();

			var list =	CommandList
						.ToList();

			list.AddRange( variables.Select(var => var.Key) );


			//list = list.OrderBy( n=>n ).ToList();

			string longestCommon = null;
			int count = 0;

			foreach ( var name in list ) {
				if (cmd.ToLower()==name.ToLower()) {
					return name + " ";
				}
				if (name.StartsWith(cmd, StringComparison.OrdinalIgnoreCase)) {
					longestCommon = LongestCommon( longestCommon, name );
					output = longestCommon;
					count++;
					Log.Message(" {0}", name);
				}
			}

			if (count==1) {
				output = output + " ";
			}

			return output;
		}
		


		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		string LongestCommon ( string a, string b )
		{
			if (string.IsNullOrEmpty(a)) {
				return b;
			}
			if (string.IsNullOrEmpty(b)) {
				return a;
			}
			int len = Math.Min( a.Length, b.Length );

			StringBuilder sb = new StringBuilder();

			for (int i=0; i<len; i++) {
				if (char.ToLower(a[i])==char.ToLower(b[i])) {
					sb.Append(b[i]);
				} else {
					break;
				}
			}

			return sb.ToString();
		}

	}
}

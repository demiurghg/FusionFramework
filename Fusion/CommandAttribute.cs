using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion {

	/// <summary>
	/// Marks function as editor command.
	/// Function should not have arguments.
	/// </summary>
	public sealed class CommandAttribute : Attribute {

		public readonly string Name;
		public readonly int GroupIndex;
		public bool Unnamed { get { return Name==null; } }

		public CommandAttribute ( string name, int groupIndex = 0 ) {
			Name = name;
			GroupIndex	= groupIndex;
		}

		public CommandAttribute () {
			Name = null;
		}
	}
}

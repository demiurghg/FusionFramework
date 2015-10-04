using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Core {

	/// <summary>
	/// Marks function as editor command.
	/// Function should not have arguments.
	/// </summary>
	public sealed class UICommandAttribute : Attribute {

		public readonly string Name;
		public readonly int GroupIndex;
		public bool Unnamed { get { return Name==null; } }

		public UICommandAttribute ( string name, int groupIndex = 0 ) {
			Name = name;
			GroupIndex	= groupIndex;
		}

		public UICommandAttribute () {
			Name = null;
		}
	}
}

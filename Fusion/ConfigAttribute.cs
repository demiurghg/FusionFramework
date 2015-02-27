using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion {

	/// <summary>
	/// Marks property as config
	/// </summary>
	public sealed class ConfigAttribute : Attribute {

		public readonly string Name;
		public bool Unnamed { get { return Name==null; } }

		public ConfigAttribute ( string name ) {
			Name = name;
		}
		public ConfigAttribute () {
			Name = null;
		}
	}
}

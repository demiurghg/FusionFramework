using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Pipeline {
	public interface IFileDerivable {
		void InitFromFile ( string path );
	}
}

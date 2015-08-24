using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Pipeline {
	interface IAssetDerivable {
		void InitFromAsset ( Asset other );
	}
}

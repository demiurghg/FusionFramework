using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using Fusion;
using Fusion.Graphics;

namespace Fusion {
	public partial class TexturePump : GameService {

		class Item {

			public Texture2D			Texture = null;
			public string				Path;
			public TexturePumpStatus	Status;

			public DateTime LastRequestTime;
			public int		SizeInBytes;


			/// <summary>
			/// 
			/// </summary>
			/// <param name="path"></param>
			public Item ( string path )
			{
				Path	= path;
				Status	= TexturePumpStatus.Loading;
			}



			/// <summary>
			/// 
			/// </summary>
			void Load ()
			{
			}


		}

	}
}

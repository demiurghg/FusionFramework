using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.UserInterface {


	/// <summary>
	/// Frame status
	/// </summary>
	public enum FrameStatus {
		None,
		Hovered,
		Pushed,
	}


	public enum TextEffect {
		None,
		Shadow,
	}


	public enum ClippingMode {
		None,
		ClipByFrame,
		ClipByBorder,
		ClipByPadding,
	}


	/// <summary>
	/// Frame anchors
	/// </summary>
	[Flags]
	public enum FrameAnchor {	
		None	=	0x0000,
		Left	=	0x0001,
		Right	=	0x0002,
		Top		=	0x0004,
		Bottom	=	0x0008,
		All		=	0x000F,
	}


	/// <summary>
	/// Alignment enum
	/// </summary>
	public enum Alignment {
		TopLeft,
		TopCenter,
		TopRight,
		MiddleLeft,
		MiddleCenter,
		MiddleRight,
		BottomLeft,
		BottomCenter,
		BottomRight,
		BaselineLeft,
		BaselineCenter,
		BaselineRight,
	}


	/// <summary>
	/// Image drawing mode
	/// </summary>
	public enum FrameImageMode {
		Centered,
		Tiled,
		Stretched,
		DirectMapped,
		//ImageAndText,
		//LetterBox,
		//
	}
}

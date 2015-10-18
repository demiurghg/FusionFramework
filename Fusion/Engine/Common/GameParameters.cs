using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Windows.Forms;
using Fusion.Drivers.Graphics;
using System.Reflection;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using Fusion.Core.Mathematics;
using Fusion.Core.Shell;


namespace Fusion.Engine.Common {

	public class GameParameters {

		[Category("GameEngine")]
		[Description("GameEngine window caption\r\n/title:<title>")]
		[CommandLineParser.Name("title")]
		public string		Title			{ get; set; }

		[Browsable(false)]
		[CommandLineParser.Ignore]
		public Icon 	Icon		{ get; set; }


		[Category("Development")]
		[Description("Enable device object tracking\r\n/trackobj")]
		[CommandLineParser.Name("trackobj")]
		public bool 	TrackObjects	{ 
			get { return SharpDX.Configuration.EnableObjectTracking; } 
			set { SharpDX.Configuration.EnableObjectTracking = value; } 
		}


		[Category("Development")]
		[Description("Enable graphics device debugging\r\n/dxdebug")]
		[CommandLineParser.Name("dxdebug")]
		public bool 	UseDebugDevice	{ get; set; }

		[Category("Graphics")]
		[Description("Display width\r\n/width:<value>")]
		[CommandLineParser.Name("width")]
		public int			Width			{ get; set; }

		[Category("Graphics")]
		[Description("Display height\r\n/height:<value>")]
		[CommandLineParser.Name("height")]
		public int			Height			{ get; set; }

		[Category("Graphics")]
		[Description("Display height\r\n/fullscr")]
		[CommandLineParser.Name("fullscr")]
		public bool			FullScreen		{ get; set; }

		[Category("Graphics")]
		[Description("Vertical synchronization interval (0 - no sync, 1 - 60 Hz, 2 - 30 Hz)\r\n/vsync:<value>")]
		[CommandLineParser.Name("vsync")]
		public int			VSyncInterval	{ get; set; }

		[Category("Graphics")]
		[Description("Stereo mode (Disabled, NVidia3Dvision, DualHead)\r\n/stereo:<value>")]
		[CommandLineParser.Name("stereo")]
		public StereoMode	StereoMode		{ get; set; }

		[Category("Graphics")]
		[Description("Stereo interlacing mode.")]
		public InterlacingMode	InterlacingMode		{ get; set; }

		[Category("Graphics")]
		[Description("MSAA level. Acceptable values are 1,2,4,8 or 16.")]
		[CommandLineParser.Name("msaa")]
		public int	MsaaLevel		{ get; set; }

		[Category("Graphics")]
		[Description("Hardware profile (Reach, HiDef)\r\n/hardware:<value>")]
		[CommandLineParser.Name("hardware")]
		public GraphicsProfile	GraphicsProfile		{ get; set; }



		public GameParameters()
		{
			SetDefault();
		}



		public void ToggleVSync ()
		{
			VSyncInterval = (VSyncInterval == 0) ? 1 : 0;
		}



		void SetDefault ()
		{
			Title			=	Path.GetFileNameWithoutExtension( Process.GetCurrentProcess().ProcessName );
			
			MsaaLevel		=	1;
			Width			=	800;
			Height			=	600;
			FullScreen		=	false;
			VSyncInterval	=	1;
			StereoMode		=	StereoMode.Disabled;

			TrackObjects	=	true;
		}
	}
}

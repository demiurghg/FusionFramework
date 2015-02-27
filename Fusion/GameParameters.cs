using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Windows.Forms;
using Fusion.Graphics;
using System.Reflection;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using Fusion.Mathematics;


namespace Fusion {

	public class GameParameters {

		[Category("Game")]
		[Description("Game window caption\r\n/title:<title>")]
		[CmdLineParser.Name("title")]
		public string		Title			{ get; set; }

		[Browsable(false)]
		[CmdLineParser.Ignore]
		public Icon 	Icon		{ get; set; }


		[Category("Development")]
		[Description("Enable device object tracking\r\n/trackobj")]
		[CmdLineParser.Name("trackobj")]
		public bool 	TrackObjects	{ 
			get { return SharpDX.Configuration.EnableObjectTracking; } 
			set { SharpDX.Configuration.EnableObjectTracking = value; } 
		}


		[Category("Development")]
		[Description("Enable graphics device debugging\r\n/dxdebug")]
		[CmdLineParser.Name("dxdebug")]
		public bool 	UseDebugDevice	{ get; set; }


		[Category("Development")]
		[Description("Current directory")]
		[ReadOnly(true)]
		[CmdLineParser.Ignore()]
		public string		CurrentDirectory { get { return Directory.GetCurrentDirectory(); } }


		[Category("Development")]
		[Description("FUSION_BIN environment variable.")]
		[ReadOnly(true)]
		[CmdLineParser.Ignore()]
		public string		FusionBinary { get { return Environment.GetEnvironmentVariable("FUSION_BIN"); } }


		[Category("Development")]
		[Description("FUSION_CONTENT environment variable.")]
		[ReadOnly(true)]
		[CmdLineParser.Ignore()]
		public string		FusionContent { get { return Environment.GetEnvironmentVariable("FUSION_CONTENT"); } }


		[Category("Development")]
		[Description("Verbosity level\r\n/verbose")]
		[CmdLineParser.Name("verbose")]
		public bool	Verbosity		{ get { return Log.Verbocity; } set { Log.Verbocity = value; } }



		[Category("Graphics")]
		[Description("Display width\r\n/width:<value>")]
		[CmdLineParser.Name("width")]
		public int			Width			{ get; set; }

		[Category("Graphics")]
		[Description("Display height\r\n/height:<value>")]
		[CmdLineParser.Name("height")]
		public int			Height			{ get; set; }

		[Category("Graphics")]
		[Description("Display height\r\n/fullscr")]
		[CmdLineParser.Name("fullscr")]
		public bool			FullScreen		{ get; set; }

		[Category("Graphics")]
		[Description("Vertical synchronization interval (0 - no sync, 1 - 60 Hz, 2 - 30 Hz)\r\n/vsync:<value>")]
		[CmdLineParser.Name("vsync")]
		public int			VSyncInterval	{ get; set; }

		[Category("Graphics")]
		[Description("Stereo mode (Disabled, NVidia3Dvision, DualHead)\r\n/stereo:<value>")]
		[CmdLineParser.Name("stereo")]
		public StereoMode	StereoMode		{ get; set; }

		[Category("Graphics")]
		[Description("Stereo interlacing mode.")]
		public InterlacingMode	InterlacingMode		{ get; set; }

		[Category("Graphics")]
		[Description("MSAA level. Acceptable values are 1,2,4,8 or 16.")]
		[CmdLineParser.Name("msaa")]
		public int	MsaaLevel		{ get; set; }

		[Category("Graphics")]
		[Description("Hardware profile (Reach, HiDef)\r\n/hardware:<value>")]
		[CmdLineParser.Name("hardware")]
		public HardwareProfile	HardwareProfile		{ get; set; }



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
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using Fusion;
using Fusion.Mathematics;
using Fusion.Input;
using System.ComponentModel;


namespace Fusion.Graphics {

	public class CameraConfig {
		[ Category("FreeCamera") ]	
		[ Description("Enables direct first person camera control. Warning: Enabling free camera will force hiding, clipping and cenetring of the mouse") ]
		public bool		FreeCamEnabled		{ set; get; }

		[ Category("FreeCamera") ]	public float	FreeCamFov			{ set; get; }
		[ Category("FreeCamera") ]	public float	FreeCamZFar			{ set; get; }
		[ Category("FreeCamera") ]	public float	FreeCamZNear		{ set; get; }
		[ Category("FreeCamera") ]	public float	FreeCamVelocity		{ set; get; }
		[ Category("FreeCamera") ]	public float	FreeCamSensitivity	{ set; get; }
		[ Category("FreeCamera") ]	public bool		FreeCamInvertMouse	{ set; get; }
		[ Category("FreeCamera") ]	public Keys		FreeCamMoveForward	{ set; get; }
		[ Category("FreeCamera") ]	public Keys		FreeCamMoveBackward	{ set; get; }
		[ Category("FreeCamera") ]	public Keys		FreeCamMoveLeft		{ set; get; }
		[ Category("FreeCamera") ]	public Keys		FreeCamMoveRight	{ set; get; }
		[ Category("FreeCamera") ]	public Keys		FreeCamMoveUp		{ set; get; }
		[ Category("FreeCamera") ]	public Keys		FreeCamMoveDown		{ set; get; }
		[ Category("FreeCamera") ]	public float	FreeCamGamepadSensitivity	{ set; get; }
		[ Category("FreeCamera") ]	public bool		FreeCamGamepadInvert		{ set; get; }

		[ Category("Stereo") ]		public float	StereoSeparation		{ set; get; }
		[ Category("Stereo") ]		public float	StereoConvergenceDist	{ set; get; }


		public CameraConfig()
		{
			FreeCamEnabled		=	false;
			FreeCamFov			=	90;
			FreeCamVelocity		=	10;
			FreeCamSensitivity	=	0.1f;
			FreeCamInvertMouse	=	false;
			FreeCamMoveForward	=	Keys.W;
			FreeCamMoveBackward	=	Keys.S;
			FreeCamMoveLeft		=	Keys.A;
			FreeCamMoveRight	=	Keys.D;
			FreeCamMoveUp		=	Keys.Space;
			FreeCamMoveDown		=	Keys.C;
			FreeCamZFar			=	5000;
			FreeCamZNear		=	0.1f;

			FreeCamGamepadInvert		=	false;
			FreeCamGamepadSensitivity	=	90;

			StereoSeparation		=	0.5f;
			StereoConvergenceDist	=	10.0f;
		}
	}
}

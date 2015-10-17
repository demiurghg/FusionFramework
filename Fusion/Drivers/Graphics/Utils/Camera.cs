using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using Fusion;
using Fusion.Core;
using Fusion.Core.Content;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Audio;
using Fusion.Drivers.Input;
using Fusion.Engine.Common;
using System.ComponentModel;


namespace Fusion.Drivers.Graphics {

	public partial class Camera : GameService {

		[Config]
		public CameraConfig Config { get; set; }

		private Matrix	CameraMatrix		{ get; set; }
		private Matrix	CameraMatrixL		{ get; set; }
		private Matrix	CameraMatrixR		{ get; set; }
		private Matrix	ViewMatrix			{ get; set; }
		private Matrix	ViewMatrixL			{ get; set; }
		private Matrix	ViewMatrixR			{ get; set; }
		private Matrix	ProjMatrix			{ get; set; }
		private Matrix	ProjMatrixL			{ get; set; }
		private Matrix	ProjMatrixR			{ get; set; }

		public Vector3	FreeCamPosition		{ get; set; }
		public float	FreeCamYaw			{ get; set; }
		public float	FreeCamPitch		{ get; set; }
		
		public bool		IsFreeCamEnabled	{ get; protected set; }

		/// <summary>
		/// Actual camera velocity
		/// </summary>
		public Vector3	CameraVelocity	{ get; protected set; }



		/// <summary>
		/// Constrcutor
		/// </summary>
		/// <param name="game"></param>
		public Camera ( GameEngine game ) : base(game)
		{
			Config		=	new CameraConfig();
			/*Listener	=	new Listener {
										OrientFront = new Vector3(0, 0, 1),
										OrientTop	= new Vector3(0, 1, 0),
										Position	= new Vector3(0, 0, 0),
										Velocity	= new Vector3(0, 0, 0)
									};*/
		}



		/// <summary>
		/// Does nothing
		/// </summary>
		public override void Initialize ()
		{
		}



		/// <summary>
		/// Toggles fly mode
		/// </summary>
		[UICommand("Toggle Fly Mode")]
		public void ToggleFlyMode ()
		{
			Config.FreeCamEnabled = !Config.FreeCamEnabled;
		}



		/// <summary>
		/// Updates camera state
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update ( GameTime gameTime )
		{
			var input	=	GameEngine.InputDevice;
			var vel		=	Config.FreeCamVelocity;
			var dt		=	gameTime.ElapsedSec;

			IsFreeCamEnabled	=	Config.FreeCamEnabled;

			if (Config.FreeCamEnabled) {

				input.IsMouseHidden		=	true;
				input.IsMouseCentered	=	true;
				input.IsMouseClipped	=	true;
				//input.CenterAndClipMouse	=	true;

				float inv       =	Config.FreeCamInvertMouse ? -1 : 1;
				FreeCamYaw		-=	input.RelativeMouseOffset.X * MathUtil.DegreesToRadians( Config.FreeCamSensitivity );
				FreeCamPitch	-=	input.RelativeMouseOffset.Y * MathUtil.DegreesToRadians( Config.FreeCamSensitivity ) * inv;
				FreeCamPitch	=	MathUtil.Clamp( FreeCamPitch, -MathUtil.Pi*0.499f, MathUtil.Pi*0.499f );
				
				var rotation	=	Matrix.RotationYawPitchRoll( FreeCamYaw, FreeCamPitch, 0 );

				Vector3 relVelocity	=	Vector3.Zero;

				var gamepad	=	GameEngine.InputDevice.GetGamepad(0);

				relVelocity += rotation.Forward * gamepad.LeftStick.Y * vel;
				relVelocity += rotation.Right   * gamepad.LeftStick.X * vel;
				FreeCamPitch	+= gamepad.RightStick.Y * MathUtil.DegreesToRadians( Config.FreeCamGamepadSensitivity ) * dt;
				FreeCamYaw		-= gamepad.RightStick.X * MathUtil.DegreesToRadians( Config.FreeCamGamepadSensitivity ) * dt;

				if ( input.IsKeyDown( Config.FreeCamMoveForward		) ) relVelocity	+= 	rotation.Forward * vel;
				if ( input.IsKeyDown( Config.FreeCamMoveBackward	) ) relVelocity	+= 	rotation.Backward * vel;
				if ( input.IsKeyDown( Config.FreeCamMoveLeft		) ) relVelocity	+= 	rotation.Left * vel;
				if ( input.IsKeyDown( Config.FreeCamMoveRight		) ) relVelocity	+= 	rotation.Right * vel;
				if ( input.IsKeyDown( Config.FreeCamMoveUp			) ) relVelocity	+= 	Vector3.UnitY * vel;
				if ( input.IsKeyDown( Config.FreeCamMoveDown		) ) relVelocity	-= 	Vector3.UnitY * vel;

				if ( gamepad.IsKeyPressed( GamepadButtons.A ) ) relVelocity	+= 	Vector3.UnitY * vel;
				if ( gamepad.IsKeyPressed( GamepadButtons.B ) ) relVelocity	-= 	Vector3.UnitY * vel;

				FreeCamPosition		+=	relVelocity * dt;

				var near		=	Config.FreeCamZNear;
				var far			=	Config.FreeCamZFar;
				var fov			=	MathUtil.DegreesToRadians( Config.FreeCamFov );
				var separation	=	Config.FreeCamStereoSeparation;
				var convergence	=	Config.FreeCamStereoConvergence;
				var origin		=	FreeCamPosition;
				var target		=	FreeCamPosition + rotation.Forward;

				SetupCamera( origin, target, Vector3.Up, relVelocity, fov, near, far, convergence, separation, -1 );
			}
		}



		/// <summary>
		/// Returns matrix with camera view
		/// </summary>
		/// <param name="eye"></param>
		/// <returns></returns>
		public Matrix GetViewMatrix ( StereoEye eye )
		{
			if (eye==StereoEye.Mono) return ViewMatrix;
			if (eye==StereoEye.Left) return ViewMatrixL;
			if (eye==StereoEye.Right) return ViewMatrixR;
			throw new ArgumentException("eye");
		}



		/// <summary>
		/// Gets projection matrix
		/// </summary>
		/// <param name="eye"></param>
		/// <returns></returns>
		public Matrix GetProjectionMatrix ( StereoEye eye )
		{
			if (eye==StereoEye.Mono) return ProjMatrix;
			if (eye==StereoEye.Left) return ProjMatrixL;
			if (eye==StereoEye.Right) return ProjMatrixR;
			throw new ArgumentException("eye");
		}



		/// <summary>
		/// Gets camera matrix
		/// </summary>
		/// <param name="eye"></param>
		/// <returns></returns>
		public Matrix GetCameraMatrix ( StereoEye eye )
		{
			if (eye==StereoEye.Mono) return CameraMatrix;
			if (eye==StereoEye.Left) return CameraMatrixL;
			if (eye==StereoEye.Right) return CameraMatrixR;
			throw new ArgumentException("eye");
		}



		/// <summary>
		/// Gets vector of camera position
		/// </summary>
		/// <param name="eye"></param>
		/// <returns></returns>
		public Vector3 GetCameraPosition ( StereoEye eye )
		{
			return GetCameraMatrix(eye).TranslationVector;
		}



		/// <summary>
		/// Gets vector of camera position
		/// </summary>
		/// <param name="eye"></param>
		/// <returns></returns>
		public Vector4 GetCameraPosition4 ( StereoEye eye )
		{
			return new Vector4( GetCameraMatrix(eye).TranslationVector, 1 );
		}



		/// <summary>
		/// 
		/// </summary>
		public AudioListener Listener {
			get {
				return new Drivers.Audio.AudioListener() {
					Position	=	CameraMatrix.TranslationVector,
					Up			=	CameraMatrix.Up,
					Forward		=	CameraMatrix.Forward,
					Velocity	=	CameraVelocity,
				};
			}
		}



		/// <summary>
		/// Sets camera up.
		/// </summary>
		/// <param name="viewMatrix">View matrix. The left-eye and right-eye view matricies will be constructed from this matrix.</param>
		/// <param name="height">Frustum with at near plane.</param>
		/// <param name="width">Frustum height ar near place.</param>
		/// <param name="near">Camera near clipping plane distance.</param>
		/// <param name="far">Camera far clipping plane distance.</param>
		/// <param name="convergenceDistance">Stereo convergence distance. </param>
		/// <param name="separation">Stereo separation or distance between eyes.</param>
		public void SetupCamera ( Matrix viewMatrix, Vector3 velocity, float height, float width, float near, float far, float convergence, float separation )
		{
			/*if (GameEngine.Parameters.StereoMode == StereoMode.OculusRift)
			{
				//ProjMatrixL = Input.OculusRiftSensors.LeftEye.Projection;
				width = 0.75f;
				height = 0.98f;
				near = 0.36f;
				far = 5000;

				viewMatrix = viewMatrix * Matrix.Invert(Matrix.RotationQuaternion(OculusRiftSensors.HeadRotation)  * Matrix.Translation(OculusRiftSensors.HeadPosition) );

				//Log.Information("{0}", OculusRiftSensors.HeadPosition);
			} */


			float offset		=	separation / convergence * near / 2;
			float nearHeight	=	height;
			float nearWidth		=	width;

			//	Projection :
			ProjMatrix		=	Matrix.PerspectiveOffCenterRH( -nearWidth/2, nearWidth/2, -nearHeight/2, nearHeight/2, near, far );

			ProjMatrixR		=	Matrix.PerspectiveOffCenterRH( -nearWidth/2 - offset, nearWidth/2 - offset, -nearHeight/2, nearHeight/2, near, far );
			ProjMatrixL		=	Matrix.PerspectiveOffCenterRH( -nearWidth/2 + offset, nearWidth/2 + offset, -nearHeight/2, nearHeight/2, near, far );
																					
			//	View :
			ViewMatrix		=	viewMatrix;
			ViewMatrixL		=	viewMatrix	*	Matrix.Translation( Vector3.UnitX * separation / 2 );
			ViewMatrixR		=	viewMatrix	*	Matrix.Translation( -Vector3.UnitX * separation / 2 );

			//ProjMatrixR = Input.OculusRiftSensors.RightEye.Projection;



			//	Camera :
			CameraMatrix	=	Matrix.Invert( ViewMatrix );
			CameraMatrixL	=	Matrix.Invert( ViewMatrixL );
			CameraMatrixR	=	Matrix.Invert( ViewMatrixR );

			//	Camera velocity :
			CameraVelocity	=	velocity;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="viewMatrix"></param>
		/// <param name="velocity"></param>
		/// <param name="fov"></param>
		/// <param name="width"></param>
		/// <param name="near"></param>
		/// <param name="far"></param>
		/// <param name="convergence"></param>
		/// <param name="separation"></param>
		public void SetupCameraFromViewAndFov ( Matrix viewMatrix, Vector3 velocity, float fov, float near, float far, float convergence, float separation )
		{
			var bounds	=	GameEngine.GraphicsDevice.DisplayBounds;
			var aspect	=	((float)bounds.Width) / (float)bounds.Height;

			var nearHeight	=	near * (float)Math.Tan( fov/2 ) * 2;
			var nearWidth	=	nearHeight * aspect;
			var view		=	viewMatrix;

			SetupCamera( view, velocity, nearHeight, nearWidth, near, far, convergence, separation );
		}



		/// <summary>
		/// Sets camera up.
		/// </summary>
		/// <param name="origin">Camera position.</param>
		/// <param name="target">Camera's target.</param>
		/// <param name="up">Camera up vector.</param>
		/// <param name="fov">Field of view in radians.</param>
		/// <param name="near">Camera near clipping plane distance.</param>
		/// <param name="far">Camera far clipping plane distance.</param>
		/// <param name="convergenceDistance">Stereo convergence distance. </param>
		/// <param name="separation">Stereo separation or distance between eyes.</param>
		/// <param name="aspectRatio">Target aspect ratio. Negative value means Display's bounds aspect ratio.</param>
		public void SetupCamera ( Vector3 origin, Vector3 target, Vector3 up, Vector3 velocity, float fov, float near, float far, float convergence, float separation, float aspectRatio=-1 )
		{
			float aspect		=	0;
			
			if (aspectRatio<0) {
				var bounds	=	GameEngine.GraphicsDevice.DisplayBounds;
				aspect		=	((float)bounds.Width) / (float)bounds.Height;
			} else {
				aspect	=	aspectRatio;
			}

			var nearHeight	=	near * (float)Math.Tan( fov/2 ) * 2;
			var nearWidth	=	nearHeight * aspect;
			var view		=	Matrix.LookAtRH( origin, target, up );

			SetupCamera( view, velocity, nearHeight, nearWidth, near, far, convergence, separation );
		}



		/// <summary>
		/// Computes bounding frustum
		/// </summary>
		/// <returns></returns>
		public BoundingFrustum ComputeFrustum()
		{
			return new BoundingFrustum( ViewMatrix * ProjMatrix );
		}



		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Config stuff :
		 * 
		-----------------------------------------------------------------------------------------*/

		public void SetWASD ()
		{
			Config.FreeCamMoveForward	=	Keys.W;
			Config.FreeCamMoveBackward	=	Keys.S;
			Config.FreeCamMoveLeft		=	Keys.A;
			Config.FreeCamMoveRight		=	Keys.D;
			Config.FreeCamMoveUp		=	Keys.Space;
			Config.FreeCamMoveDown		=	Keys.C;
		}


		public void SetASZX ()
		{
			Config.FreeCamMoveForward	=	Keys.S;
			Config.FreeCamMoveBackward	=	Keys.Z;
			Config.FreeCamMoveLeft		=	Keys.A;
			Config.FreeCamMoveRight		=	Keys.X;
			Config.FreeCamMoveUp		=	Keys.Space;
			Config.FreeCamMoveDown		=	Keys.C;
		}


		public void ToggleMouseInversion ()
		{
			Config.FreeCamInvertMouse	=	!Config.FreeCamInvertMouse;
		}



	}
}

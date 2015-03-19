using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using Fusion;
using Fusion.Mathematics;
using Fusion.Audio;
using Fusion.Input;
using System.ComponentModel;


namespace Fusion.Graphics {

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

		//public Listener Listener			{ get; protected set; }
		
		public bool		IsFreeCamEnabled	{ get; protected set; }

		public float	FrustumWidth		{ get; protected set; }
		public float	FrustumHeight		{ get; protected set; }
		public float	FrustumZNear		{ get; protected set; }
		public float	FrustumZFar			{ get; protected set; }

		/// <summary>
		/// Actual free camera velocity
		/// </summary>
		public Vector3	FreeCameraVelocity		{ get; protected set; }



		/// <summary>
		/// Constrcutor
		/// </summary>
		/// <param name="game"></param>
		public Camera ( Game game ) : base(game)
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
		[Command("Toggle Fly Mode")]
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
			var input	=	Game.InputDevice;
			var vel		=	Config.FreeCamVelocity;
			var dt		=	gameTime.ElapsedSec;

			IsFreeCamEnabled	=	Config.FreeCamEnabled;

			if (Config.FreeCamEnabled) {

				input.IsMouseHidden		=	true;
				input.IsMouseCentered	=	true;
				input.IsMouseClipped	=	true;
				//input.CenterAndClipMouse	=	true;

				float inv       =	Config.FreeCamInvertMouse ? -1 : 1;
				FreeCamYaw		-=	input.RelativeMouseOffset.X * Config.FreeCamSensitivity;
				FreeCamPitch	-=	input.RelativeMouseOffset.Y * Config.FreeCamSensitivity * inv;

				FreeCamPitch	=	MathUtil.Clamp( FreeCamPitch, -90, 90 );
				
				SetPose( Vector3.Zero, FreeCamYaw, FreeCamPitch, 0 );
				SetFov( Config.FreeCamFov, Config.FreeCamZNear, Config.FreeCamZFar );

				Vector3 relVelocity	=	Vector3.Zero;

				var gamepad	=	Game.InputDevice.GetGamepad(0);

				relVelocity += CameraMatrix.Forward * gamepad.LeftStick.Y * vel;
				relVelocity += CameraMatrix.Right   * gamepad.LeftStick.X * vel;
				FreeCamPitch	+= gamepad.RightStick.Y * Config.FreeCamGamepadSensitivity * dt;
				FreeCamYaw		-= gamepad.RightStick.X * Config.FreeCamGamepadSensitivity * dt;

				if ( input.IsKeyDown( Config.FreeCamMoveForward		) ) relVelocity	+= 	CameraMatrix.Forward * vel;
				if ( input.IsKeyDown( Config.FreeCamMoveBackward	) ) relVelocity	+= 	CameraMatrix.Backward * vel;
				if ( input.IsKeyDown( Config.FreeCamMoveLeft		) ) relVelocity	+= 	CameraMatrix.Left * vel;
				if ( input.IsKeyDown( Config.FreeCamMoveRight		) ) relVelocity	+= 	CameraMatrix.Right * vel;
				if ( input.IsKeyDown( Config.FreeCamMoveUp			) ) relVelocity	+= 	Vector3.UnitY * vel;
				if ( input.IsKeyDown( Config.FreeCamMoveDown		) ) relVelocity	-= 	Vector3.UnitY * vel;

				if ( gamepad.IsKeyPressed( GamepadButtons.A ) ) relVelocity	+= 	Vector3.UnitY * vel;
				if ( gamepad.IsKeyPressed( GamepadButtons.B ) ) relVelocity	-= 	Vector3.UnitY * vel;

				FreeCameraVelocity	=	relVelocity;
				FreeCamPosition	+=	relVelocity * dt;
				/*if ( input.IsKeyDown( Config.FreeCamMoveForward	) ) FreeCamPosition	+= 	CameraMatrix.Forward * vel * dt;
				if ( input.IsKeyDown( Config.FreeCamMoveBackward	) ) FreeCamPosition	+= 	CameraMatrix.Backward * vel * dt;
				if ( input.IsKeyDown( Config.FreeCamMoveLeft		) ) FreeCamPosition	+= 	CameraMatrix.Left * vel * dt;
				if ( input.IsKeyDown( Config.FreeCamMoveRight		) ) FreeCamPosition	+= 	CameraMatrix.Right * vel * dt;
				if ( input.IsKeyDown( Config.FreeCamMoveUp			) ) FreeCamPosition	+= 	Vector3.UnitY * vel * dt;
				if ( input.IsKeyDown( Config.FreeCamMoveDown		) ) FreeCamPosition	-= 	Vector3.UnitY * vel * dt;*/

				SetPose( FreeCamPosition, FreeCamYaw, FreeCamPitch, 0 );

			}

			// Sound stuff
			/*Listener.Velocity = (CameraMatrix.TranslationVector - Listener.Position)/gameTime.ElapsedSec;
			Listener.Position = CameraMatrix.TranslationVector;

			Listener.OrientTop		= CameraMatrix.Up;
			Listener.OrientFront	= -CameraMatrix.Forward;*/
		}



		public Matrix GetViewMatrix ( StereoEye eye )
		{
			if (eye==StereoEye.Mono) return ViewMatrix;
			if (eye==StereoEye.Left) return ViewMatrixL;
			if (eye==StereoEye.Right) return ViewMatrixR;
			throw new ArgumentException("eye");
		}



		public Matrix GetProjectionMatrix ( StereoEye eye )
		{
			if (eye==StereoEye.Mono) return ProjMatrix;
			if (eye==StereoEye.Left) return ProjMatrixL;
			if (eye==StereoEye.Right) return ProjMatrixR;
			throw new ArgumentException("eye");
		}



		public Matrix GetCameraMatrix ( StereoEye eye )
		{
			if (eye==StereoEye.Mono) return CameraMatrix;
			if (eye==StereoEye.Left) return CameraMatrixL;
			if (eye==StereoEye.Right) return CameraMatrixR;
			throw new ArgumentException("eye");
		}


		public Vector3 GetCameraPosition ( StereoEye eye )
		{
			return GetCameraMatrix(eye).TranslationVector;
		}



		public Vector4 GetCameraPosition4 ( StereoEye eye )
		{
			return new Vector4( GetCameraMatrix(eye).TranslationVector, 1 );
		}



		public AudioListener Listener {
			get {
				return new Audio.AudioListener() {
					Position	=	CameraMatrix.TranslationVector,
					Up			=	CameraMatrix.Up,
					Forward		=	CameraMatrix.Forward,
					Velocity	=	FreeCameraVelocity,
				};
			}
		}



		/// <summary>
		///	Sets projection camera parameters 
		/// </summary>
		/// <param name="fovDeg"></param>
		/// <param name="near"></param>
		/// <param name="far"></param>
		public void	SetFov	( float fovDeg, float near=0.1f, float far=12000.0f ) 
		{
			float separation	=	Config.StereoSeparation;
			float planeDist		=	Config.StereoConvergenceDist;

			var bounds			=	Game.GraphicsDevice.DisplayBounds;

			float aspect		=	((float)bounds.Width) / (float)bounds.Height;

			float nearHeight	=	near * (float)Math.Tan( MathUtil.DegreesToRadians( fovDeg/2 ) ) * 2;
			float nearWidth		=	nearHeight * aspect;
			float offset		=	separation / planeDist * near / 2;

			FrustumWidth		=	nearWidth;
			FrustumHeight		=	nearHeight;
			FrustumZNear		=	near;
			FrustumZFar			=	far;

			//ProjMatrix		=	Matrix.PerspectiveRH( nearWidth, nearHeight, near, far );
			ProjMatrix		=	Matrix.PerspectiveOffCenterRH( -nearWidth/2, nearWidth/2, -nearHeight/2, nearHeight/2, near, far );

			ProjMatrixR		=	Matrix.PerspectiveOffCenterRH( -nearWidth/2 - offset, nearWidth/2 - offset, -nearHeight/2, nearHeight/2, near, far );
			ProjMatrixL		=	Matrix.PerspectiveOffCenterRH( -nearWidth/2 + offset, nearWidth/2 + offset, -nearHeight/2, nearHeight/2, near, far );

			if (OculusRiftSensors.LeftEye != null) {
				ProjMatrix = OculusRiftSensors.LeftEye.Projection;
				ProjMatrixL = OculusRiftSensors.LeftEye.Projection;
				ProjMatrixR = OculusRiftSensors.RightEye.Projection;
				
				//var ds = Game.GetService<DebugStrings>();
				
				//ds.Add(ProjMatrixL.ToString());
				//ds.Add(ProjMatrixR.ToString());
			}
		}


		//public Matrix ComputeStereoCameraMatrix ( b


		/// <summary>
		/// Set camera position and rotation
		/// </summary>
		/// <param name="position"></param>
		/// <param name="yaw"></param>
		/// <param name="pitch"></param>
		/// <param name="roll"></param>
		public void SetPose ( Vector3 position, float yaw, float pitch, float roll )
		{
			var separation = Config.StereoSeparation;

			ViewMatrix	=	Matrix.Translation( -position )
						*	Matrix.Transpose( Matrix.RotationYawPitchRoll( MathUtil.Rad(yaw), MathUtil.Rad(pitch), MathUtil.Rad(roll) ) );

			ViewMatrixL	=	Matrix.Translation( -position )
						*	Matrix.Transpose( Matrix.RotationYawPitchRoll( MathUtil.Rad(yaw), MathUtil.Rad(pitch), MathUtil.Rad(roll) ) )
						*	Matrix.Translation( Vector3.UnitX * separation / 2 );

			ViewMatrixR	=	Matrix.Translation( -position )
						*	Matrix.Transpose( Matrix.RotationYawPitchRoll( MathUtil.Rad(yaw), MathUtil.Rad(pitch), MathUtil.Rad(roll) ) )
						*	Matrix.Translation( -Vector3.UnitX * separation / 2 );

			CameraMatrix = Matrix.Invert( ViewMatrix );

			CameraMatrixL	=	Matrix.Invert( ViewMatrixL );
			CameraMatrixR	=	Matrix.Invert( ViewMatrixR );
			//Listener.Position	=	cameraMatrix.Translation;
			//Listener.Forward	=	cameraMatrix.Forward;
			//Listener.Up			=	cameraMatrix.Up;
			//Listener.Velocity	=	Vector3.Zero;



			if (OculusRiftSensors.LeftEye != null) {
				var rot = Quaternion.RotationYawPitchRoll(MathUtil.Rad(yaw), MathUtil.Rad(pitch), MathUtil.Rad(roll));


				//ViewMatrix = Matrix.Translation(-position - OculusRiftSensors.HeadPosition*5)
				//		* Matrix.Transpose(Matrix.RotationQuaternion(OculusRiftSensors.HeadRotation * rot));

				ViewMatrixL = Matrix.Translation(-position)
							* Matrix.Transpose(Matrix.RotationQuaternion(rot*OculusRiftSensors.LeftEye.Rotation))
							* Matrix.Translation(-OculusRiftSensors.LeftEye.Position * 1);

				ViewMatrixR = Matrix.Translation(-position)
							* Matrix.Transpose(Matrix.RotationQuaternion(rot * OculusRiftSensors.RightEye.Rotation))
							* Matrix.Translation(-OculusRiftSensors.RightEye.Position * 1);


				//CameraMatrix	= Matrix.Invert(ViewMatrix);
				CameraMatrixL	= Matrix.Invert(ViewMatrixL);
				CameraMatrixR	= Matrix.Invert(ViewMatrixR);
			}
		}


		/// <summary>
		/// Sets camera "look at"
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="target"></param>
		/// <param name="up"></param>
		public void LookAt ( Vector3 origin, Vector3 target, Vector3 up, float separation = 0 )
		{
			ViewMatrix		=	Matrix.LookAtRH( origin, target, up );

			ViewMatrixL		=	Matrix.LookAtRH( origin, target, up )
							*	Matrix.Translation( Vector3.UnitX * separation / 2 );

			ViewMatrixR		=	Matrix.LookAtRH( origin, target, up )
							*	Matrix.Translation( -Vector3.UnitX * separation / 2 );

			CameraMatrix	=	Matrix.Invert( ViewMatrix );
			CameraMatrixL	=	Matrix.Invert( ViewMatrixL );
			CameraMatrixR	=	Matrix.Invert( ViewMatrixR );
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

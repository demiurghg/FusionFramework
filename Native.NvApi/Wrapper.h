// Native.NvApi;.h

#include <d3d11.h>
#include "nvapi.h"

#pragma once

using namespace System;

namespace Native {

	namespace NvApi {

		public value struct Stereo_Caps
		{
		public:
			UInt32 Version;
			UInt32 SupportsWindowedModeOff;
			UInt32 SupportsWindowedModeAutomatic;
			UInt32 SupportsWindowedModePersistent;
		};

		public enum class NvStereoActiveEye {
			Left, Right, Mono
		};

		public enum class NvStereoDriverMode {
			Direct, Automatic
		};


		public enum class NvStatus {
			Ok											= NVAPI_OK												,
			Error										= NVAPI_ERROR											,
			LibraryNotFound								= NVAPI_LIBRARY_NOT_FOUND								,
			NoImplementation							= NVAPI_NO_IMPLEMENTATION								,
			ApiNotInitialized							= NVAPI_API_NOT_INITIALIZED								,
			InvalidArgument								= NVAPI_INVALID_ARGUMENT								,
			NvidiaDeviceNotFound						= NVAPI_NVIDIA_DEVICE_NOT_FOUND							,
			EndEnumeration								= NVAPI_END_ENUMERATION									,
			InvalidHandle								= NVAPI_INVALID_HANDLE									,
			IncompatibleStructVersion					= NVAPI_INCOMPATIBLE_STRUCT_VERSION						,
			HandleInvalidated							= NVAPI_HANDLE_INVALIDATED								,
			OpenglContextNotCurrent						= NVAPI_OPENGL_CONTEXT_NOT_CURRENT						,
			InvalidPointer								= NVAPI_INVALID_POINTER									,
			NoGlExpert									= NVAPI_NO_GL_EXPERT									,
			InstrumentationDisabled						= NVAPI_INSTRUMENTATION_DISABLED						,
			NoGlNsight									= NVAPI_NO_GL_NSIGHT									,
			ExpectedLogicalGpuHandle					= NVAPI_EXPECTED_LOGICAL_GPU_HANDLE						,
			ExpectedPhysicalGpuHandle					= NVAPI_EXPECTED_PHYSICAL_GPU_HANDLE					,
			ExpectedDisplayHandle						= NVAPI_EXPECTED_DISPLAY_HANDLE							,
			InvalidCombination							= NVAPI_INVALID_COMBINATION								,
			NotSupported								= NVAPI_NOT_SUPPORTED									,
			PortidNotFound								= NVAPI_PORTID_NOT_FOUND								,
			ExpectedUnattachedDisplayHandle				= NVAPI_EXPECTED_UNATTACHED_DISPLAY_HANDLE				,
			InvalidPerfLevel							= NVAPI_INVALID_PERF_LEVEL								,
			DeviceBusy									= NVAPI_DEVICE_BUSY										,
			NvPersistFileNotFound						= NVAPI_NV_PERSIST_FILE_NOT_FOUND						,
			PersistDataNotFound							= NVAPI_PERSIST_DATA_NOT_FOUND							,
			ExpectedTvDisplay							= NVAPI_EXPECTED_TV_DISPLAY								,
			ExpectedTvDisplayOnDconnector				= NVAPI_EXPECTED_TV_DISPLAY_ON_DCONNECTOR				,
			NoActiveSliTopology							= NVAPI_NO_ACTIVE_SLI_TOPOLOGY							,
			SliRenderingModeNotallowed					= NVAPI_SLI_RENDERING_MODE_NOTALLOWED					,
			ExpectedDigitalFlatPanel					= NVAPI_EXPECTED_DIGITAL_FLAT_PANEL						,
			ArgumentExceedMaxSize						= NVAPI_ARGUMENT_EXCEED_MAX_SIZE						,
			DeviceSwitchingNotAllowed					= NVAPI_DEVICE_SWITCHING_NOT_ALLOWED					,
			TestingClocksNotSupported					= NVAPI_TESTING_CLOCKS_NOT_SUPPORTED					,
			UnknownUnderscanConfig						= NVAPI_UNKNOWN_UNDERSCAN_CONFIG						,
			TimeoutReconfiguringGpuTopo					= NVAPI_TIMEOUT_RECONFIGURING_GPU_TOPO					,
			DataNotFound								= NVAPI_DATA_NOT_FOUND									,
			ExpectedAnalogDisplay						= NVAPI_EXPECTED_ANALOG_DISPLAY							,
			NoVidlink									= NVAPI_NO_VIDLINK										,
			RequiresReboot								= NVAPI_REQUIRES_REBOOT									,
			InvalidHybridMode							= NVAPI_INVALID_HYBRID_MODE								,
			MixedTargetTypes							= NVAPI_MIXED_TARGET_TYPES								,
			Syswow64NotSupported						= NVAPI_SYSWOW64_NOT_SUPPORTED							,
			ImplicitSetGpuTopologyChangeNotAllowed		= NVAPI_IMPLICIT_SET_GPU_TOPOLOGY_CHANGE_NOT_ALLOWED	,
			RequestUserToCloseNonMigratableApps			= NVAPI_REQUEST_USER_TO_CLOSE_NON_MIGRATABLE_APPS		,
			OutOfMemory									= NVAPI_OUT_OF_MEMORY									,
			WasStillDrawing								= NVAPI_WAS_STILL_DRAWING								,
			FileNotFound								= NVAPI_FILE_NOT_FOUND									,
			TooManyUniqueStateObjects					= NVAPI_TOO_MANY_UNIQUE_STATE_OBJECTS					,
			InvalidCall									= NVAPI_INVALID_CALL									,
			D3d101LibraryNotFound						= NVAPI_D3D10_1_LIBRARY_NOT_FOUND						,
			FunctionNotFound							= NVAPI_FUNCTION_NOT_FOUND								,
			InvalidUserPrivilege						= NVAPI_INVALID_USER_PRIVILEGE							,
			ExpectedNonPrimaryDisplayHandle				= NVAPI_EXPECTED_NON_PRIMARY_DISPLAY_HANDLE				,
			ExpectedComputeGpuHandle					= NVAPI_EXPECTED_COMPUTE_GPU_HANDLE						,
			StereoNotInitialized						= NVAPI_STEREO_NOT_INITIALIZED							,
			StereoRegistryAccessFailed					= NVAPI_STEREO_REGISTRY_ACCESS_FAILED					,
			StereoRegistryProfileTypeNotSupported		= NVAPI_STEREO_REGISTRY_PROFILE_TYPE_NOT_SUPPORTED		,
			StereoRegistryValueNotSupported				= NVAPI_STEREO_REGISTRY_VALUE_NOT_SUPPORTED				,
			StereoNotEnabled							= NVAPI_STEREO_NOT_ENABLED								,
			StereoNotTurnedOn							= NVAPI_STEREO_NOT_TURNED_ON							,
			StereoInvalidDeviceInterface				= NVAPI_STEREO_INVALID_DEVICE_INTERFACE					,
			StereoParameterOutOfRange					= NVAPI_STEREO_PARAMETER_OUT_OF_RANGE					,
			StereoFrustumAdjustModeNotSupported			= NVAPI_STEREO_FRUSTUM_ADJUST_MODE_NOT_SUPPORTED		,
			TopoNotPossible								= NVAPI_TOPO_NOT_POSSIBLE								,
			ModeChangeFailed							= NVAPI_MODE_CHANGE_FAILED								,
			D3d11LibraryNotFound						= NVAPI_D3D11_LIBRARY_NOT_FOUND							,
			InvalidAddress								= NVAPI_INVALID_ADDRESS									,
			StringTooSmall								= NVAPI_STRING_TOO_SMALL								,
			MatchingDeviceNotFound						= NVAPI_MATCHING_DEVICE_NOT_FOUND						,
			DriverRunning								= NVAPI_DRIVER_RUNNING									,
			DriverNotrunning							= NVAPI_DRIVER_NOTRUNNING								,
			ErrorDriverReloadRequired					= NVAPI_ERROR_DRIVER_RELOAD_REQUIRED					,
			SetNotAllowed								= NVAPI_SET_NOT_ALLOWED									,
			AdvancedDisplayTopologyRequired				= NVAPI_ADVANCED_DISPLAY_TOPOLOGY_REQUIRED				,
			SettingNotFound								= NVAPI_SETTING_NOT_FOUND								,
			SettingSizeTooLarge							= NVAPI_SETTING_SIZE_TOO_LARGE							,
			TooManySettingsInProfile					= NVAPI_TOO_MANY_SETTINGS_IN_PROFILE					,
			ProfileNotFound								= NVAPI_PROFILE_NOT_FOUND								,
			ProfileNameInUse							= NVAPI_PROFILE_NAME_IN_USE								,
			ProfileNameEmpty							= NVAPI_PROFILE_NAME_EMPTY								,
			ExecutableNotFound							= NVAPI_EXECUTABLE_NOT_FOUND							,
			ExecutableAlreadyInUse						= NVAPI_EXECUTABLE_ALREADY_IN_USE						,
			DatatypeMismatch							= NVAPI_DATATYPE_MISMATCH								,
			ProfileRemoved								= NVAPI_PROFILE_REMOVED									,
			UnregisteredResource						= NVAPI_UNREGISTERED_RESOURCE							,
			IdOutOfRange								= NVAPI_ID_OUT_OF_RANGE									,
			DisplayconfigValidationFailed				= NVAPI_DISPLAYCONFIG_VALIDATION_FAILED					,
			DpmstChanged								= NVAPI_DPMST_CHANGED									,
			InsufficientBuffer							= NVAPI_INSUFFICIENT_BUFFER								,
			AccessDenied								= NVAPI_ACCESS_DENIED									,
			MosaicNotActive								= NVAPI_MOSAIC_NOT_ACTIVE								,
			ShareResourceRelocated						= NVAPI_SHARE_RESOURCE_RELOCATED						,
			RequestUserToDisableDwm						= NVAPI_REQUEST_USER_TO_DISABLE_DWM						,
			D3dDeviceLost								= NVAPI_D3D_DEVICE_LOST									,
			InvalidConfiguration						= NVAPI_INVALID_CONFIGURATION							,
			StereoHandshakeNotDone						= NVAPI_STEREO_HANDSHAKE_NOT_DONE						,
			ExecutablePathIsAmbiguous					= NVAPI_EXECUTABLE_PATH_IS_AMBIGUOUS					,
			DefaultStereoProfileIsNotDefined			= NVAPI_DEFAULT_STEREO_PROFILE_IS_NOT_DEFINED			,
			DefaultStereoProfileDoesNotExist			= NVAPI_DEFAULT_STEREO_PROFILE_DOES_NOT_EXIST			,
			ClusterAlreadyExists						= NVAPI_CLUSTER_ALREADY_EXISTS							,
			DpmstDisplayIdExpected						= NVAPI_DPMST_DISPLAY_ID_EXPECTED						,
			InvalidDisplayId							= NVAPI_INVALID_DISPLAY_ID								,
			StreamIsOutOfSync							= NVAPI_STREAM_IS_OUT_OF_SYNC							,
			IncompatibleAudioDriver						= NVAPI_INCOMPATIBLE_AUDIO_DRIVER						,
			ValueAlreadySet								= NVAPI_VALUE_ALREADY_SET								,
			Timeout										= NVAPI_TIMEOUT											,
			GpuWorkstationFeatureIncomplete				= NVAPI_GPU_WORKSTATION_FEATURE_INCOMPLETE				,
			StereoInitActivationNotDone					= NVAPI_STEREO_INIT_ACTIVATION_NOT_DONE					,
			SyncNotActive								= NVAPI_SYNC_NOT_ACTIVE									,
			SyncMasterNotFound							= NVAPI_SYNC_MASTER_NOT_FOUND							,
			InvalidSyncTopology							= NVAPI_INVALID_SYNC_TOPOLOGY							,
		};


	
		public ref class NVException : public Exception {

			NvStatus errorCode;

			public:
				NVException( String ^message, NvStatus status ) : Exception(message)
				{
					errorCode = status;
				}

				NvStatus GetStatus ()
				{
					return errorCode;
				}
		};


		public ref class NvApi
		{
		public:

			static void Initialize ()
			{
				NvCall(	NvAPI_Initialize() );
			}
			


			static void Stereo_CreateConfigurationProfileRegistryKey ( NV_STEREO_REGISTRY_PROFILE_TYPE registryProfileType )
			{
				NvCall( NvAPI_Stereo_CreateConfigurationProfileRegistryKey( registryProfileType ) );
			}



			static void	Stereo_DeleteConfigurationProfileRegistryKey ( NV_STEREO_REGISTRY_PROFILE_TYPE registryProfileType )
			{
				NvCall( NvAPI_Stereo_DeleteConfigurationProfileRegistryKey( registryProfileType ) );
			}



			static void Stereo_SetConfigurationProfileValue ( NV_STEREO_REGISTRY_PROFILE_TYPE registryProfileType, NV_STEREO_REGISTRY_ID valueRegistryID, IntPtr pValue )
			{
				NvCall( NvAPI_Stereo_SetConfigurationProfileValue( registryProfileType, valueRegistryID, pValue.ToPointer() ) );
			}



			static void Stereo_DeleteConfigurationProfileValue ( NV_STEREO_REGISTRY_PROFILE_TYPE registryProfileType, NV_STEREO_REGISTRY_ID valueRegistryID )
			{
				NvCall( NvAPI_Stereo_DeleteConfigurationProfileValue( registryProfileType, valueRegistryID ) );
			}
			
					
				
			static void Stereo_Enable ()
			{
				NvCall( NvAPI_Stereo_Enable() );
			}



			static void Stereo_Disable ()
			{
				NvCall( NvAPI_Stereo_Disable() );
			}
			
			

			static bool Stereo_IsEnabled ()
			{
				NvU8 r = 0;
				NvCall( NvAPI_Stereo_IsEnabled( &r ) );
				return r != 0;
			}



			static Stereo_Caps Stereo_GetStereoSupport ( IntPtr monitor )
			{
				NVAPI_STEREO_CAPS nvCaps;
				NvCall( NvAPI_Stereo_GetStereoSupport( (NvMonitorHandle)monitor.ToPointer(), &nvCaps) );

				Stereo_Caps caps;
				caps.Version = nvCaps.version;
				caps.SupportsWindowedModeOff = nvCaps.supportsWindowedModeOff;
				caps.SupportsWindowedModeAutomatic = nvCaps.supportsWindowedModeAutomatic;
				caps.SupportsWindowedModePersistent = nvCaps.supportsWindowedModePersistent;
				return caps;
			}



			static IntPtr Stereo_CreateHandleFromIUnknown ( IntPtr device )
			{
				StereoHandle handle = nullptr;
				NvCall( NvAPI_Stereo_CreateHandleFromIUnknown( (IUnknown*)device.ToPointer(), &handle ) );
				return IntPtr( handle );
			}



			static void Stereo_DestroyHandle ( IntPtr stereoHandle )
			{
				NvCall( NvAPI_Stereo_DestroyHandle( stereoHandle.ToPointer() ) );
			}



			static void	Stereo_Activate ( IntPtr stereoHandle )
			{
				NvCall( NvAPI_Stereo_Activate( stereoHandle.ToPointer() ) );
			}



			static void	Stereo_Deactivate ( IntPtr stereoHandle )
			{
				NvCall( NvAPI_Stereo_Deactivate( stereoHandle.ToPointer() ) );
			}



			static bool Stereo_IsActivated ( IntPtr stereoHandle )
			{
				NvU8 r = 0;
				NvCall( NvAPI_Stereo_IsActivated( stereoHandle.ToPointer(), &r ) );
				return r != 0;
			}



			static float Stereo_GetSeparation ( IntPtr stereoHandle)
			{
				float separationPercentage;
				NvCall( NvAPI_Stereo_GetSeparation( stereoHandle.ToPointer(), &separationPercentage ) );
				return separationPercentage;
			}



			static void	Stereo_SetSeparation ( IntPtr stereoHandle, float newSeparationPercentage )
			{
				NvCall( NvAPI_Stereo_SetSeparation( stereoHandle.ToPointer(), newSeparationPercentage ) );
			}



			static void	Stereo_DecreaseSeparation ( IntPtr stereoHandle )
			{
				NvCall( NvAPI_Stereo_DecreaseSeparation( stereoHandle.ToPointer() ) );
			}



			static void	Stereo_IncreaseSeparation ( IntPtr stereoHandle )
			{
				NvCall( NvAPI_Stereo_IncreaseSeparation( stereoHandle.ToPointer() ) );
			}



			static float Stereo_GetConvergence ( IntPtr stereoHandle )
			{
				float convergence;
				NvCall( NvAPI_Stereo_GetConvergence( stereoHandle.ToPointer(), &convergence ) );
				return convergence;
			}



			static void Stereo_SetConvergence ( IntPtr stereoHandle, float newConvergence )
			{
				NvCall( NvAPI_Stereo_SetConvergence( stereoHandle.ToPointer(), newConvergence ) );
			}



			static void	Stereo_DecreaseConvergence ( IntPtr stereoHandle )
			{
				NvCall( NvAPI_Stereo_DecreaseConvergence( stereoHandle.ToPointer() ) );
			}



			static void	Stereo_IncreaseConvergence ( IntPtr stereoHandle )
			{
				NvCall( NvAPI_Stereo_IncreaseConvergence( stereoHandle.ToPointer() ) );
			}



			static NV_FRUSTUM_ADJUST_MODE Stereo_GetFrustumAdjustMode ( IntPtr stereoHandle)
			{
				NV_FRUSTUM_ADJUST_MODE frustumAdjustMode;
				NvCall( NvAPI_Stereo_GetFrustumAdjustMode( stereoHandle.ToPointer(), &frustumAdjustMode ) );
				return frustumAdjustMode;
			}



			static void Stereo_SetFrustumAdjustMode ( IntPtr stereoHandle, NV_FRUSTUM_ADJUST_MODE newFrustumAdjustModeValue )
			{
				NvCall( NvAPI_Stereo_SetFrustumAdjustMode( stereoHandle.ToPointer(), newFrustumAdjustModeValue ) );
			}



			static void	Stereo_CaptureJpegImage ( IntPtr stereoHandle, unsigned int quality)
			{
				NvCall( NvAPI_Stereo_CaptureJpegImage( stereoHandle.ToPointer(), quality ) );
			}



			static void Stereo_InitActivation ( IntPtr stereoHandle, NVAPI_STEREO_INIT_ACTIVATION_FLAGS flags )
			{
				NvCall( NvAPI_Stereo_InitActivation( stereoHandle.ToPointer(), flags ) );
			}



			static void Stereo_Trigger_Activation ( IntPtr stereoHandle)
			{
				NvCall( NvAPI_Stereo_Trigger_Activation( stereoHandle.ToPointer() ) );
			}



			static void Stereo_CapturePngImage ( IntPtr stereoHandle ) 
			{
				NvCall( NvAPI_Stereo_CapturePngImage( stereoHandle.ToPointer() ) );
			}



			static void Stereo_ReverseStereoBlitControl ( IntPtr stereoHandle, bool TurnOn )
			{
				NvCall( NvAPI_Stereo_ReverseStereoBlitControl( stereoHandle.ToPointer(), TurnOn ? 1 : 0 ) );
			}



			static void	Stereo_SetNotificationMessage ( IntPtr stereoHandle, UInt64 hWnd, UInt64 messageID )
			{
				NvCall( NvAPI_Stereo_SetNotificationMessage( stereoHandle.ToPointer(), hWnd, messageID ) );
			}



			static void	Stereo_SetActiveEye ( IntPtr stereoHandle, NvStereoActiveEye stereoEye ) 
			{
				NV_STEREO_ACTIVE_EYE eye = NVAPI_STEREO_EYE_MONO;
				if (stereoEye==NvStereoActiveEye::Left)  eye = NVAPI_STEREO_EYE_LEFT;
				if (stereoEye==NvStereoActiveEye::Right) eye = NVAPI_STEREO_EYE_RIGHT;
				if (stereoEye==NvStereoActiveEye::Mono)  eye = NVAPI_STEREO_EYE_MONO;
				NvCall( NvAPI_Stereo_SetActiveEye( stereoHandle.ToPointer(), eye ) );
			}



			static void	Stereo_SetDriverMode ( NvStereoDriverMode mode )
			{
				NV_STEREO_DRIVER_MODE _mode = NVAPI_STEREO_DRIVER_MODE_AUTOMATIC;

				if (mode==NvStereoDriverMode::Direct)	 _mode = NVAPI_STEREO_DRIVER_MODE_DIRECT;
				if (mode==NvStereoDriverMode::Automatic) _mode = NVAPI_STEREO_DRIVER_MODE_AUTOMATIC;

				NvCall( NvAPI_Stereo_SetDriverMode( _mode ) );
			}



			static float Stereo_GetEyeSeparation ( IntPtr stereoHandle )
			{
				float separation;
				NvCall( NvAPI_Stereo_GetEyeSeparation( stereoHandle.ToPointer(), &separation ) );
				return separation;
			}



			static void	Stereo_SetSurfaceCreationMode ( IntPtr stereoHandle, NVAPI_STEREO_SURFACECREATEMODE creationMode )
			{
				NvCall( NvAPI_Stereo_SetSurfaceCreationMode( stereoHandle.ToPointer(), creationMode ) );
			}



			static NVAPI_STEREO_SURFACECREATEMODE Stereo_GetSurfaceCreationMode ( IntPtr stereoHandle )
			{
				NVAPI_STEREO_SURFACECREATEMODE creationMode = NVAPI_STEREO_SURFACECREATEMODE_AUTO;
				NvCall( NvAPI_Stereo_GetSurfaceCreationMode( stereoHandle.ToPointer(), &creationMode) );
				return creationMode;
			}

		private:
			/*-----------------------------------------------------------------------------

				NVAPI call error checking

			-----------------------------------------------------------------------------*/
	
			/*
			**	NvCall
			*/
			static void NvCall ( NvAPI_Status status )
			{
				String ^result = gcnew String( TranslateCall( status ) );

				//System::Console::WriteLine("NvApi : {0}", result );

				if ( status != NVAPI_OK ) {
					throw gcnew NVException( result, (NvStatus)status );
				}
			}


			/*
			**	TranslateCall	
			*/
			static wchar_t *TranslateCall ( NvAPI_Status status )
			{																									
				if (status==NVAPI_OK											) return L"Success. Request is completed."																					   ;
				if (status==NVAPI_ERROR											) return L"Generic error"																									   ;
				if (status==NVAPI_LIBRARY_NOT_FOUND								) return L"NVAPI support library cannot be loaded."																			   ;
				if (status==NVAPI_NO_IMPLEMENTATION								) return L"not implemented in current driver installation"																	   ;
				if (status==NVAPI_API_NOT_INITIALIZED							) return L"NvAPI_Initialize has not been called (successfully)"																   ;
				if (status==NVAPI_INVALID_ARGUMENT								) return L"The argument/parameter value is not valid or NULL."																   ;
				if (status==NVAPI_NVIDIA_DEVICE_NOT_FOUND						) return L"No NVIDIA display driver, or NVIDIA GPU driving a display, was found."											   ;
				if (status==NVAPI_END_ENUMERATION								) return L"No more items to enumerate"																						   ;
				if (status==NVAPI_INVALID_HANDLE								) return L"Invalid handle "																									   ;
				if (status==NVAPI_INCOMPATIBLE_STRUCT_VERSION					) return L"An argument's structure version is not supported"																	   ;
				if (status==NVAPI_HANDLE_INVALIDATED							) return L"The handle is no longer valid (likely due to GPU or display re-configuration)"									   ;
				if (status==NVAPI_OPENGL_CONTEXT_NOT_CURRENT					) return L"No NVIDIA OpenGL context is current (but needs to be)"															   ;
				if (status==NVAPI_INVALID_POINTER								) return L"An invalid pointer, usually NULL, was passed as a parameter"														   ;
				if (status==NVAPI_NO_GL_EXPERT									) return L"OpenGL Expert is not supported by the current drivers	"															   ;
				if (status==NVAPI_INSTRUMENTATION_DISABLED						) return L"OpenGL Expert is supported, but driver instrumentation is currently disabled"										   ;
				if (status==NVAPI_NO_GL_NSIGHT									) return L"OpenGL does not support Nsight"																					   ;
				if (status==NVAPI_EXPECTED_LOGICAL_GPU_HANDLE					) return L"Expected a logical GPU handle for one or more parameters"															   ;
				if (status==NVAPI_EXPECTED_PHYSICAL_GPU_HANDLE					) return L"Expected a physical GPU handle for one or more parameters"														   ;
				if (status==NVAPI_EXPECTED_DISPLAY_HANDLE						) return L"Expected an NV display handle for one or more parameters"															   ;
				if (status==NVAPI_INVALID_COMBINATION							) return L"The combination of parameters is not valid." 																		   ;
				if (status==NVAPI_NOT_SUPPORTED									) return L"Requested feature is not supported in the selected GPU"															   ;
				if (status==NVAPI_PORTID_NOT_FOUND								) return L"No port ID was found for the I2C transaction"																		   ;
				if (status==NVAPI_EXPECTED_UNATTACHED_DISPLAY_HANDLE			) return L"Expected an unattached display handle as one of the input parameters."											   ;
				if (status==NVAPI_INVALID_PERF_LEVEL							) return L"Invalid perf level" 																								   ;
				if (status==NVAPI_DEVICE_BUSY									) return L"Device is busy; request not fulfilled"																			   ;
				if (status==NVAPI_NV_PERSIST_FILE_NOT_FOUND						) return L"NV persist file is not found"																						   ;
				if (status==NVAPI_PERSIST_DATA_NOT_FOUND						) return L"NV persist data is not found"																						   ;
				if (status==NVAPI_EXPECTED_TV_DISPLAY							) return L"Expected a TV output display"																						   ;
				if (status==NVAPI_EXPECTED_TV_DISPLAY_ON_DCONNECTOR				) return L"Expected a TV output on the D Connector - HDTV_EIAJ4120."															   ;
				if (status==NVAPI_NO_ACTIVE_SLI_TOPOLOGY						) return L"SLI is not active on this device."																				   ;
				if (status==NVAPI_SLI_RENDERING_MODE_NOTALLOWED					) return L"Setup of SLI rendering mode is not possible right now."															   ;
				if (status==NVAPI_EXPECTED_DIGITAL_FLAT_PANEL					) return L"Expected a digital flat panel."																					   ;
				if (status==NVAPI_ARGUMENT_EXCEED_MAX_SIZE						) return L"Argument exceeds the expected size."																				   ;
				if (status==NVAPI_DEVICE_SWITCHING_NOT_ALLOWED					) return L"Inhibit is ON due to one of the flags in NV_GPU_DISPLAY_CHANGE_INHIBIT or SLI active."							   ;
				if (status==NVAPI_TESTING_CLOCKS_NOT_SUPPORTED					) return L"Testing of clocks is not supported."																				   ;
				if (status==NVAPI_UNKNOWN_UNDERSCAN_CONFIG						) return L"The specified underscan config is from an unknown source (e.g. INF)"												   ;
				if (status==NVAPI_TIMEOUT_RECONFIGURING_GPU_TOPO				) return L"Timeout while reconfiguring GPUs"																					   ;
				if (status==NVAPI_DATA_NOT_FOUND								) return L"Requested data was not found"																						   ;
				if (status==NVAPI_EXPECTED_ANALOG_DISPLAY						) return L"Expected an analog display"																						   ;
				if (status==NVAPI_NO_VIDLINK									) return L"No SLI video bridge is present"																					   ;
				if (status==NVAPI_REQUIRES_REBOOT								) return L"NVAPI requires a reboot for the settings to take effect"															   ;
				if (status==NVAPI_INVALID_HYBRID_MODE							) return L"The function is not supported with the current Hybrid mode."														   ;
				if (status==NVAPI_MIXED_TARGET_TYPES							) return L"The target types are not all the same"																			   ;
				if (status==NVAPI_SYSWOW64_NOT_SUPPORTED						) return L"The function is not supported from 32-bit on a 64-bit system."													   ;
				if (status==NVAPI_IMPLICIT_SET_GPU_TOPOLOGY_CHANGE_NOT_ALLOWED	) return L"There is no implicit GPU topology active. Use NVAPI_SetHybridMode to change topology."							   ;
				if (status==NVAPI_REQUEST_USER_TO_CLOSE_NON_MIGRATABLE_APPS		) return L"Prompt the user to close all non-migratable applications."    													   ;
				if (status==NVAPI_OUT_OF_MEMORY									) return L"Could not allocate sufficient memory to complete the call."														   ;
				if (status==NVAPI_WAS_STILL_DRAWING								) return L"The previous operation that is transferring information to or from this surface is incomplete."					   ;
				if (status==NVAPI_FILE_NOT_FOUND								) return L"The file was not found."																							   ;
				if (status==NVAPI_TOO_MANY_UNIQUE_STATE_OBJECTS					) return L"There are too many unique instances of a particular type of state object."										   ;
				if (status==NVAPI_INVALID_CALL									) return L"The method call is invalid. For example, a method's parameter may not be a valid pointer."						   ;
				if (status==NVAPI_D3D10_1_LIBRARY_NOT_FOUND						) return L"d3d10_1.dll cannot be loaded."																					   ;
				if (status==NVAPI_FUNCTION_NOT_FOUND							) return L"Couldn't find the function in the loaded DLL."																	   ;
				if (status==NVAPI_INVALID_USER_PRIVILEGE						) return L"Current User is not Admin."																						   ;
				if (status==NVAPI_EXPECTED_NON_PRIMARY_DISPLAY_HANDLE			) return L"The handle corresponds to GDIPrimary."																			   ;
				if (status==NVAPI_EXPECTED_COMPUTE_GPU_HANDLE					) return L"Setting Physx GPU requires that the GPU is compute-capable."														   ;
				if (status==NVAPI_STEREO_NOT_INITIALIZED						) return L"The Stereo part of NVAPI failed to initialize completely. Check if the stereo driver is installed."				   ;
				if (status==NVAPI_STEREO_REGISTRY_ACCESS_FAILED					) return L"Access to stereo-related registry keys or values has failed."														   ;
				if (status==NVAPI_STEREO_REGISTRY_PROFILE_TYPE_NOT_SUPPORTED	) return L"The given registry profile type is not supported."																   ;
				if (status==NVAPI_STEREO_REGISTRY_VALUE_NOT_SUPPORTED			) return L"The given registry value is not supported."																		   ;
				if (status==NVAPI_STEREO_NOT_ENABLED							) return L"Stereo is not enabled and the function needed it to execute completely."											   ;
				if (status==NVAPI_STEREO_NOT_TURNED_ON							) return L"Stereo is not turned on and the function needed it to execute completely."										   ;
				if (status==NVAPI_STEREO_INVALID_DEVICE_INTERFACE				) return L"Invalid device interface."																						   ;
				if (status==NVAPI_STEREO_PARAMETER_OUT_OF_RANGE					) return L"Separation percentage or JPEG image capture quality is out of [0-100] range."										   ;
				if (status==NVAPI_STEREO_FRUSTUM_ADJUST_MODE_NOT_SUPPORTED		) return L"The given frustum adjust mode is not supported."																	   ;
				if (status==NVAPI_TOPO_NOT_POSSIBLE								) return L"The mosaic topology is not possible given the current state of the hardware."										   ;
				if (status==NVAPI_MODE_CHANGE_FAILED							) return L"An attempt to do a display resolution mode change has failed."													   ;
				if (status==NVAPI_D3D11_LIBRARY_NOT_FOUND						) return L"d3d11.dll/d3d11_beta.dll cannot be loaded."																		   ;
				if (status==NVAPI_INVALID_ADDRESS								) return L"Address is outside of valid range."																				   ;
				if (status==NVAPI_STRING_TOO_SMALL								) return L"The pre-allocated string is too small to hold the result."														   ;
				if (status==NVAPI_MATCHING_DEVICE_NOT_FOUND						) return L"The input does not match any of the available devices."															   ;
				if (status==NVAPI_DRIVER_RUNNING								) return L"Driver is running."																								   ;
				if (status==NVAPI_DRIVER_NOTRUNNING								) return L"Driver is not running."																							   ;
				if (status==NVAPI_ERROR_DRIVER_RELOAD_REQUIRED					) return L"A driver reload is required to apply these settings."																   ;
				if (status==NVAPI_SET_NOT_ALLOWED								) return L"Intended setting is not allowed."																					   ;
				if (status==NVAPI_ADVANCED_DISPLAY_TOPOLOGY_REQUIRED			) return L"Information can't be returned due to 'advanced display topology'"												   ;
				if (status==NVAPI_SETTING_NOT_FOUND								) return L"Setting is not found."																							   ;
				if (status==NVAPI_SETTING_SIZE_TOO_LARGE						) return L"Setting size is too large."																						   ;
				if (status==NVAPI_TOO_MANY_SETTINGS_IN_PROFILE					) return L"There are too many settings for a profile."																		   ;
				if (status==NVAPI_PROFILE_NOT_FOUND								) return L"Profile is not found."																							   ;
				if (status==NVAPI_PROFILE_NAME_IN_USE							) return L"Profile name is duplicated."																						   ;
				if (status==NVAPI_PROFILE_NAME_EMPTY							) return L"Profile name is empty."																							   ;
				if (status==NVAPI_EXECUTABLE_NOT_FOUND							) return L"Application not found in the Profile."																			   ;
				if (status==NVAPI_EXECUTABLE_ALREADY_IN_USE						) return L"Application already exists in the other profile."																	   ;
				if (status==NVAPI_DATATYPE_MISMATCH								) return L"Data Type mismatch "																								   ;
				if (status==NVAPI_PROFILE_REMOVED								) return L"The profile passed as parameter has been removed and is no longer valid."											   ;
				if (status==NVAPI_UNREGISTERED_RESOURCE							) return L"An unregistered resource was passed as a parameter."																   ;
				if (status==NVAPI_ID_OUT_OF_RANGE								) return L"The DisplayId corresponds to a display which is not within the normal outputId range."							   ;
				if (status==NVAPI_DISPLAYCONFIG_VALIDATION_FAILED				) return L"Display topology is not valid so the driver cannot do a mode set on this configuration."							   ;
				if (status==NVAPI_DPMST_CHANGED									) return L"Display Port Multi-Stream topology has been changed."																   ;
				if (status==NVAPI_INSUFFICIENT_BUFFER							) return L"Input buffer is insufficient to hold the contents."																   ;
				if (status==NVAPI_ACCESS_DENIED									) return L"No access to the caller."																							   ;
				if (status==NVAPI_MOSAIC_NOT_ACTIVE								) return L"The requested action cannot be performed without Mosaic being enabled."											   ;
				if (status==NVAPI_SHARE_RESOURCE_RELOCATED						) return L"The surface is relocated away from video memory."																	   ;
				if (status==NVAPI_REQUEST_USER_TO_DISABLE_DWM					) return L"The user should disable DWM before calling NvAPI."																   ;
				if (status==NVAPI_D3D_DEVICE_LOST								) return L"D3D device status is D3DERR_DEVICELOST or D3DERR_DEVICENOTRESET - the user has to reset the device."				   ;
				if (status==NVAPI_INVALID_CONFIGURATION							) return L"The requested action cannot be performed in the current state."													   ;
				if (status==NVAPI_STEREO_HANDSHAKE_NOT_DONE						) return L"Call failed as stereo handshake not completed."																	   ;
				if (status==NVAPI_EXECUTABLE_PATH_IS_AMBIGUOUS					) return L"The path provided was too short to determine the correct NVDRS_APPLICATION"										   ;
				if (status==NVAPI_DEFAULT_STEREO_PROFILE_IS_NOT_DEFINED			) return L"Default stereo profile is not currently defined"																	   ;
				if (status==NVAPI_DEFAULT_STEREO_PROFILE_DOES_NOT_EXIST			) return L"Default stereo profile does not exist"																			   ;
				if (status==NVAPI_CLUSTER_ALREADY_EXISTS						) return L"A cluster is already defined with the given configuration."														   ;
				if (status==NVAPI_DPMST_DISPLAY_ID_EXPECTED						) return L"The input display id is not that of a multi stream enabled connector or a display device in a multi stream topology ";
				if (status==NVAPI_INVALID_DISPLAY_ID							) return L"The input display id is not valid or the monitor associated to it does not support the current operation"			   ;
				if (status==NVAPI_STREAM_IS_OUT_OF_SYNC							) return L"While playing secure audio stream, stream goes out of sync"														   ;
				if (status==NVAPI_INCOMPATIBLE_AUDIO_DRIVER						) return L"Older audio driver version than required"																			   ;
				if (status==NVAPI_VALUE_ALREADY_SET								) return L"Value already set, setting again not allowed."																	   ;
				if (status==NVAPI_TIMEOUT										) return L"Requested operation timed out."																					   ;
				if (status==NVAPI_GPU_WORKSTATION_FEATURE_INCOMPLETE			) return L"The requested workstation feature set has incomplete driver internal allocation resources"						   ;
				if (status==NVAPI_STEREO_INIT_ACTIVATION_NOT_DONE				) return L"Call failed because InitActivation was not called."																   ;
				if (status==NVAPI_SYNC_NOT_ACTIVE								) return L"The requested action cannot be performed without Sync being enabled."												   ;
				if (status==NVAPI_SYNC_MASTER_NOT_FOUND							) return L"The requested action cannot be performed without Sync Master being enabled."										   ;
				if (status==NVAPI_INVALID_SYNC_TOPOLOGY							) return L"Invalid displays passed in the NV_GSYNC_DISPLAY pointer."															   ;
				return L"Unknown";
			}
		};
	}
}

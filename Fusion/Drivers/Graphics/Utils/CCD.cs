using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Fusion.Drivers.Graphics
{
	// This class takes care of wrapping "Connecting and Configuring Displays(CCD) Win32 API"
	internal static class CCD
	{
		public enum DisplayTopology
        {
            Internal,
            External,
            Extend,
            Clone
        }
		
		public static DisplayTopology GetDisplayTopology() 
		{
			uint numPathArrayElements = 0;
			uint numModeInfoArrayElements = 0;

			NativeMethods.GetDisplayConfigBufferSizes( QueryDisplayFlags.DatabaseCurrent, out numPathArrayElements, out numModeInfoArrayElements );

			var pathArray = new DisplayConfigPathInfo[numPathArrayElements];
			var modeArray = new DisplayConfigModeInfo[numModeInfoArrayElements];

			DisplayConfigTopologyId displayTopology;

			NativeMethods.QueryDisplayConfig( QueryDisplayFlags.DatabaseCurrent, out numPathArrayElements, pathArray, out numModeInfoArrayElements, modeArray, out displayTopology );

			switch( displayTopology ) {
				case DisplayConfigTopologyId.External:	return DisplayTopology.External;
				case DisplayConfigTopologyId.Internal:	return DisplayTopology.Internal;
				case DisplayConfigTopologyId.Extend:	return DisplayTopology.Extend;
			}

			return DisplayTopology.Clone;
		}



		public static void SetDisplayTopology( DisplayTopology displayTopology )
        {
			switch( displayTopology ) {
				case DisplayTopology.External:
					NativeMethods.SetDisplayConfig( 0, null, 0, null, ( SdcFlags.Apply | SdcFlags.TopologyExternal ) );
					break;
				case DisplayTopology.Internal:
					NativeMethods.SetDisplayConfig( 0, null, 0, null, ( SdcFlags.Apply | SdcFlags.TopologyInternal ) );
					break;
				case DisplayTopology.Extend:
					NativeMethods.SetDisplayConfig( 0, null, 0, null, ( SdcFlags.Apply | SdcFlags.TopologyExtend ) );
					break;
				case DisplayTopology.Clone:
					NativeMethods.SetDisplayConfig( 0, null, 0, null, ( SdcFlags.Apply | SdcFlags.TopologyClone ) );
					break;
			}
		}

		[StructLayout(LayoutKind.Sequential)]
        struct LUID
        {
            uint LowPart;
            uint HighPart;
        }

        [Flags]
        enum DisplayConfigVideoOutputTechnology : uint
        {
            Other				= 4294967295, // -1
            Hd15				= 0,
            Svideo				= 1,
            CompositeVideo		= 2,
            ComponentVideo		= 3,
            Dvi					= 4,
            Hdmi				= 5,
            Lvds				= 6,
            DJpn				= 8,
            Sdi					= 9,
            DisplayportExternal = 10,
            DisplayportEmbedded = 11,
            UdiExternal			= 12,
            UdiEmbedded			= 13,
            Sdtvdongle			= 14,
            Internal			= 0x80000000,
            ForceUint32			= 0xFFFFFFFF
        }

        #region SdcFlags enum

        [Flags]
        enum SdcFlags : uint
        {
            Zero = 0,

            TopologyInternal			= 0x00000001,
            TopologyClone				= 0x00000002,
            TopologyExtend				= 0x00000004,
            TopologyExternal			= 0x00000008,
            TopologySupplied			= 0x00000010,

            UseSuppliedDisplayConfig	= 0x00000020,
            Validate					= 0x00000040,
            Apply						= 0x00000080,
            NoOptimization				= 0x00000100,
            SaveToDatabase				= 0x00000200,
            AllowChanges				= 0x00000400,
            PathPersistIfRequired		= 0x00000800,
            ForceModeEnumeration		= 0x00001000,
            AllowPathOrderChanges		= 0x00002000,

            UseDatabaseCurrent = TopologyInternal | TopologyClone | TopologyExtend | TopologyExternal
        }

        [Flags]
        enum DisplayConfigFlags : uint
        {
            Zero		= 0x0,
            PathActive	= 0x00000001
        }

        [Flags]
        enum DisplayConfigSourceStatus
        {
            Zero	= 0x0,
            InUse	= 0x00000001
        }

        [Flags]
        enum DisplayConfigTargetStatus : uint
        {
            Zero = 0x0,

            InUse                        = 0x00000001,
            FORCIBLE                     = 0x00000002,
            ForcedAvailabilityBoot       = 0x00000004,
            ForcedAvailabilityPath       = 0x00000008,
            ForcedAvailabilitySystem     = 0x00000010,
        }

        [Flags]
        enum DisplayConfigRotation : uint
        {
            Zero		= 0x0,

            Identity	= 1,
            Rotate90	= 2,
            Rotate180	= 3,
            Rotate270	= 4,
            ForceUint32 = 0xFFFFFFFF
        }

        [Flags]
        enum DisplayConfigPixelFormat : uint
        {
            Zero					= 0x0,

            Pixelformat8Bpp			= 1,
            Pixelformat16Bpp		= 2,
            Pixelformat24Bpp		= 3,
            Pixelformat32Bpp		= 4,
            PixelformatNongdi		= 5,
            PixelformatForceUint32	= 0xffffffff
        }

        [Flags]
        enum DisplayConfigScaling : uint
        {
            Zero					= 0x0, 

            Identity				= 1,
            Centered				= 2,
            Stretched				= 3,
            Aspectratiocenteredmax	= 4,
            Custom					= 5,
            Preferred				= 128,
            ForceUint32				= 0xFFFFFFFF
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DisplayConfigRational
        {
            uint numerator;
            uint denominator;
        }

        [Flags]
        enum DisplayConfigScanLineOrdering : uint
        {
            Unspecified					= 0,
            Progressive					= 1,
            Interlaced					= 2,
            InterlacedUpperfieldfirst	= Interlaced,
            InterlacedLowerfieldfirst	= 3,
            ForceUint32					= 0xFFFFFFFF
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DisplayConfigPathInfo
        {
            DisplayConfigPathSourceInfo sourceInfo;
            DisplayConfigPathTargetInfo targetInfo;
            uint flags;
        }

        [Flags]
        enum DisplayConfigModeInfoType : uint
        {
            Zero	= 0,

            Source	= 1,
            Target	= 2,
            ForceUint32 = 0xFFFFFFFF
        }

        [StructLayout(LayoutKind.Explicit)]
        struct DisplayConfigModeInfo
        {
            [FieldOffset(0)]
            DisplayConfigModeInfoType infoType;

            [FieldOffset(4)]
            uint id;

            [FieldOffset(8)]
            LUID adapterId;

            [FieldOffset(16)]
            DisplayConfigTargetMode targetMode;

            [FieldOffset(16)]
            DisplayConfigSourceMode sourceMode;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DisplayConfig2DRegion
        {
            uint cx;
            uint cy;
        }

        [Flags]
        enum D3DmdtVideoSignalStandard : uint
        {
            Uninitialized	= 0,
            VesaDmt			= 1,
            VesaGtf			= 2,
            VesaCvt			= 3,
            Ibm				= 4,
            Apple			= 5,
            NtscM			= 6,
            NtscJ			= 7,
            Ntsc443			= 8,
            PalB			= 9,
            PalB1			= 10,
            PalG			= 11,
            PalH			= 12,
            PalI			= 13,
            PalD			= 14,
            PalN			= 15,
            PalNc			= 16,
            SecamB			= 17,
            SecamD			= 18,
            SecamG			= 19,
            SecamH			= 20,
            SecamK			= 21,
            SecamK1			= 22,
            SecamL			= 23,
            SecamL1			= 24,
            Eia861			= 25,
            Eia861A			= 26,
            Eia861B			= 27,
            PalK			= 28,
            PalK1			= 29,
            PalL			= 30,
            PalM			= 31,
            Other			= 255
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DisplayConfigVideoSignalInfo
        {
            long pixelRate;
            DisplayConfigRational hSyncFreq;
            DisplayConfigRational vSyncFreq;
            DisplayConfig2DRegion activeSize;
            DisplayConfig2DRegion totalSize;

            D3DmdtVideoSignalStandard videoStandard;
            DisplayConfigScanLineOrdering ScanLineOrdering;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DisplayConfigTargetMode
        {
            DisplayConfigVideoSignalInfo targetVideoSignalInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct PointL
        {
            int x;
            int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DisplayConfigSourceMode
        {
            uint width;
            uint height;
            DisplayConfigPixelFormat pixelFormat;
            PointL position;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DisplayConfigPathSourceInfo
        {
            LUID adapterId;
            uint id;
            uint modeInfoIdx;

            DisplayConfigSourceStatus statusFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DisplayConfigPathTargetInfo
        {
            LUID adapterId;
            uint id;
            uint modeInfoIdx;
            DisplayConfigVideoOutputTechnology outputTechnology; 
            DisplayConfigRotation rotation;
            DisplayConfigScaling scaling;
            DisplayConfigRational refreshRate;
            DisplayConfigScanLineOrdering scanLineOrdering;

            bool targetAvailable;
            DisplayConfigTargetStatus statusFlags;
        }

        [Flags]
        enum QueryDisplayFlags : uint
        {
            Zero			= 0x0,

            AllPaths		= 0x00000001,
            OnlyActivePaths = 0x00000002,
            DatabaseCurrent = 0x00000004
        }

        [Flags]
        enum DisplayConfigTopologyId : uint
        {
            Zero		= 0x0,

            Internal	= 0x00000001,
            Clone		= 0x00000002,
            Extend		= 0x00000004,
            External	= 0x00000008,
            ForceUint32 = 0xFFFFFFFF
        }

        #endregion

		static class NativeMethods {
			[DllImport("User32.dll")]
			public static extern int GetDisplayConfigBufferSizes( QueryDisplayFlags flags, out uint numPathArrayElements, out uint numModeInfoArrayElements );

			[DllImport("User32.dll")]
			public static extern int SetDisplayConfig( uint numPathArrayElements, [In] DisplayConfigPathInfo[] pathArray, uint numModeInfoArrayElements, [In] DisplayConfigModeInfo[] modeInfoArray, SdcFlags flags );

			[DllImport("User32.dll")]
			public static extern int QueryDisplayConfig( QueryDisplayFlags flags, out uint numPathArrayElements, [Out] DisplayConfigPathInfo[] pathInfoArray, out uint modeInfoArrayElements, [Out] DisplayConfigModeInfo[] modeInfoArray, out DisplayConfigTopologyId id );
		}
	}
}

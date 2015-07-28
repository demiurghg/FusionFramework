// Fusion.DDS.h

#pragma once

#include "DDSTextureLoader.h"

using namespace System;

namespace Native {
	namespace Dds {
		public ref class DdsLoader
		{
		public:
			static bool CreateTextureFromMemory ( IntPtr device, array<Byte>^ fileInMemory, bool forceSRgb, IntPtr %resource, IntPtr %srv );
		};
	}
}

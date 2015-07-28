// Fusion.DDS.h

#pragma once

#include "WICTextureLoader.h"

using namespace System;

namespace Native {
	namespace Wic {
		public ref class WicLoader
		{
		public:
			static bool CreateTextureFromMemory ( IntPtr device, array<Byte>^ fileInMemory, bool forceSRgb, IntPtr %resource, IntPtr %srv );
		};
	}
}

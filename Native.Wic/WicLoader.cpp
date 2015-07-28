// This is the main DLL file.

#include "WicLoader.h"
#include "WICTextureLoader.h"


using namespace DirectX;

bool Native::Wic::WicLoader::CreateTextureFromMemory( IntPtr device, array<Byte>^ fileInMemory, bool forceSRgb, IntPtr %resource, IntPtr %srv )
{
	ID3D11Resource *d3dres;
	ID3D11ShaderResourceView *d3dsrv;

	pin_ptr<Byte> dataPtr	=	&fileInMemory[0];
	int dataSize			=	fileInMemory->Length;

	HRESULT	hr = CreateWICTextureFromMemoryEx( 
						(ID3D11Device*)device.ToPointer(), 
						(uint8_t*)dataPtr, dataSize, 0, D3D11_USAGE_DEFAULT, D3D11_BIND_SHADER_RESOURCE, 0, 0, forceSRgb,
						&d3dres, &d3dsrv );

	if (FAILED(hr)) {
		return false;
	}

	resource	=	IntPtr( d3dres );
	srv			=	IntPtr( d3dsrv );

	return true;//*/
}

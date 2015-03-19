

#if 0
$ubershader	VERTICAL_LR|VERTICAL_RL|HORIZONTAL_LR|HORIZONTAL_RL|OCULUS_RIFT
#endif

Texture2D		LeftChannel 	: 	register(t0);
Texture2D		RightChannel 	: 	register(t1);
SamplerState	Linear			: 	register(s0);

struct PS_IN {
	float4 pos : SV_POSITION;
};


PS_IN VSMain( uint id: SV_VertexID )
{
	PS_IN output = (PS_IN)0;
	
	if (id==0) output.pos = float4(-1, 1, 0, 1);
	if (id==1) output.pos = float4(-1,-3, 0, 1);
	if (id==2) output.pos = float4( 3, 1, 0, 1);
	
	return output;
}


float4 PSMain( PS_IN input ) : SV_Target
{	
	uint x = (uint)input.pos.x;
	uint y = (uint)input.pos.y;
	
#if VERTICAL_LR
	float3 left		=	LeftChannel .Load( int3(x,y,0) ).rgb;
	float3 right	=	RightChannel.Load( int3(x,y,0) ).rgb;
	float3 merged	=	lerp( left, right, x%2==1 );
	return float4(merged,1);
#endif	
#if VERTICAL_RL
	float3 left		=	LeftChannel .Load( int3(x,y,0) ).rgb;
	float3 right	=	RightChannel.Load( int3(x,y,0) ).rgb;
	float3 merged	=	lerp( left, right, x%2==0 );
	return float4(merged,1);
#endif	
	
#if HORIZONTAL_LR
	float3 left		=	LeftChannel .Load( int3(x,y,0) ).rgb;
	float3 right	=	RightChannel.Load( int3(x,y,0) ).rgb;
	float3 merged	=	lerp( left, right, y%2==1 );
	return float4(merged,1);
#endif	
#if HORIZONTAL_RL
	float3 left		=	LeftChannel .Load( int3(x,y,0) ).rgb;
	float3 right	=	RightChannel.Load( int3(x,y,0) ).rgb;
	float3 merged	=	lerp( left, right, y%2==0 );
	return float4(merged,1);
#endif	

	return float4(1,0,0,1);

}
/*-----------------------------------------------------------------------------
	Debug render shader :
-----------------------------------------------------------------------------*/

#if 0
$ubershader
#endif

struct BATCH {
	float4x4	Transform;
};

cbuffer CBBatch : register(b0) { BATCH Batch : packoffset( c0 ); }

struct VS_IN
{
	float3 pos : POSITION;
	float4 col : COLOR;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 col : COLOR;
};

PS_IN VSMain( VS_IN input )
{
	PS_IN output = (PS_IN)0;
	
	output.pos = mul( float4(input.pos,1), Batch.Transform );
	output.col = input.col;
	
	return output;
}

float4 PSMain( PS_IN input ) : SV_Target
{
	return input.col;
}
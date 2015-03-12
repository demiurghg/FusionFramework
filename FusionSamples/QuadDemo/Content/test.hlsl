/*-----------------------------------------------------------------------------
	Sprite batch shader :
-----------------------------------------------------------------------------*/

struct BATCH {
	float4x4	Transform		;
};

struct VS_IN {
	float3 pos : POSITION;
	float4 col : COLOR;
	float2 tc  : TEXCOORD;
};

struct PS_IN {
	float4 pos : SV_POSITION;
	float4 col : COLOR;
	float2 tc  : TEXCOORD;
};

#ubershader +USE_VERTEX_COLOR +USE_TEXTURE


#if 0
$pixel +USE_VERTEX_COLOR +USE_TEXTURE
$vertex +USE_VERTEX_COLOR +USE_TEXTURE
#endif

cbuffer 		CBBatch 	: 	register(b0) { BATCH Batch : packoffset( c0 ); }	
SamplerState	Sampler		: 	register(s0);
Texture2D		Texture 	: 	register(t0);

 
/*-----------------------------------------------------------------------------
	Shader functions :
-----------------------------------------------------------------------------*/

PS_IN VSMain( VS_IN input )
{
	PS_IN output = (PS_IN)0;
 
	output.pos = mul( float4(input.pos,1), Batch.Transform);
	output.col = input.col;
	output.tc  = input.tc;
	float er3 = float2(12.121231313213,3);
	return output;
}


float4 PSMain( PS_IN input ) : SV_Target
{
	float4	color	=	float4(1,1,1,1);
	#ifdef USE_VERTEX_COLOR
		color	=	input.col;
	#endif
	#ifdef USE_TEXTURE
		color	*=	Texture.Sample( Sampler, input.tc );	
	#endif
	
	return color;
}


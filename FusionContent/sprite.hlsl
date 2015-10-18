/*-----------------------------------------------------------------------------
	Sprite batch shader :
-----------------------------------------------------------------------------*/

#if 0
$ubershader OPAQUE|ALPHA_BLEND|ALPHA_BLEND_PREMUL|ADDITIVE|SCREEN|MULTIPLY|NEG_MULTIPLY
#endif

struct BATCH {
	float4x4	Transform		;
	float4		ClipRectangle	;
	float4		MasterColor		;
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
	output.col = input.col * Batch.MasterColor;
	output.tc  = input.tc;

	return output;
}


float4 PSMain( PS_IN input ) : SV_Target
{
	float2 	vpos	=	input.pos.xy;
	float4	tex		=	Texture.Sample( Sampler, input.tc );	
	//float3	bw	=	dot( tex.rgb, float3(0.3f, 0.5f, 0.2f) );
	
	// clip stiff :
	/*
	float4 clipRect 	= Batch.ClipRectangle;
	float  clipVal 	= 1;
	
	if ( vpos.x < clipRect.x || vpos.x > clipRect.x + clipRect.z ) {
		clipVal = -1;
	}
	
	if ( vpos.y < clipRect.y || vpos.y > clipRect.y + clipRect.w ) {
		clipVal = -1;
	}
	
	clip( clipVal );
	*/
	
	return input.col * tex;
}


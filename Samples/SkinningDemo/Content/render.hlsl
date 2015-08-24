
struct BATCH {
	float4x4	Projection		;
	float4x4	View			;
	float4x4	World			;
	
	float4		ViewPos			;
	
	float4		SkyLightDir		;
	float4		SkyLightColor	;
	
	float4		LightPos0		;
	float4		LightPos1		;
	float4		LightPos2		;
	float4		LightColor0		;
	float4		LightColor1		;
	float4		LightColor2		;
};



struct VS_IN {
	float3 Position 	: POSITION;
	float3 Normal 		: NORMAL;
	float4 Color 		: COLOR;
    int4   BoneIndices  : BLENDINDICES0;
    float4 BoneWeights  : BLENDWEIGHT0;
};

struct PS_IN {
	float4 	Position 	: SV_POSITION;
	float4 	Color 		: COLOR;
	float3	WNormal		: TEXCOORD1;
};


cbuffer 		CBBatch 	: 	register(b0) { BATCH Batch : packoffset( c0 ); }	
cbuffer 		CBBatch 	: 	register(b1) { float4x4 Bones[128] : packoffset( c0 ); }	
SamplerState	Sampler		: 	register(s0);
Texture2D		Texture 	: 	register(t0);

#if 0
$ubershader
#endif



/*-----------------------------------------------------------------------------
	Skinning stuff :
-----------------------------------------------------------------------------*/

#pragma warning (disable: 3206)

float4x3 AccumulateSkin( float4 boneWeights0, float4 boneIndices0 )
{
	float4x3 result = boneWeights0.x * Bones[boneIndices0.x];
	result = result + boneWeights0.y * Bones[boneIndices0.y];
	result = result + boneWeights0.z * Bones[boneIndices0.z];
	result = result + boneWeights0.w * Bones[boneIndices0.w];
	return result;
}

float4 TransformPosition( VS_IN input, float3 inputPos )
{
	float3 position = 0; 
	
	float4x3 xform  = AccumulateSkin(input.BoneWeights, input.BoneIndices); 
	position = mul( float4(inputPos,1), xform );
	
	return float4(position, 1);
}


float4 TransformNormal( VS_IN input, float3 inputNormal )
{
    float3 normal = 0;

	float4x3 xform  = AccumulateSkin(input.BoneWeights, input.BoneIndices); 
	normal = mul( float4(inputNormal,0), xform );
	
	return float4(normal, 0);
}

 
/*-----------------------------------------------------------------------------
	Shader functions :
-----------------------------------------------------------------------------*/
PS_IN VSMain( VS_IN input )
{
	PS_IN output 	= (PS_IN)0;
	
	float4 	sPos	=	TransformPosition	( input, input.Position	);
	float4  sNormal	=	TransformNormal		( input, input.Normal	);
	
	float4	wPos	=	mul( sPos, Batch.World 		);
	float4	vPos	=	mul( wPos, Batch.View 		);
	float4	pPos	=	mul( vPos, Batch.Projection );
	float4	normal	=	mul( sNormal,  Batch.World 		);
	
	output.Position = pPos;
	output.Color 	= 1;
	output.WNormal	= normalize(normal);
	
	
	
	return output;
}


float4 PSMain( PS_IN input ) : SV_Target
{
	float3 amb = float3(100,149,237) / 256.0f / 3;
	float3 sun = float3(255,240,120) / 256.0f * 1;
	float3 light = (0.2 + 0.8*dot( input.WNormal, normalize(float3(2,3,1)))) * sun + amb;
	return float4(light, 1);
}








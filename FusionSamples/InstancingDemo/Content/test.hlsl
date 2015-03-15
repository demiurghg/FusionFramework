/*-----------------------------------------------------------------------------
	Sprite batch shader :
-----------------------------------------------------------------------------*/

#if 0
$ubershader
#endif

struct BATCH {
	float4x4	Transform		;
	float		Time;
};

struct VS_IN {
	float3 pos : POSITION;
	float4 col : COLOR;
	float2 tc  : TEXCOORD;
	uint instanceID : SV_InstanceID;
};

struct PS_IN {
	float4 pos : SV_POSITION;
	float4 col : COLOR;
	float2 tc  : TEXCOORD;
};

struct INSTANCE {
	float2	offset;
	float	rotation;
	float	size;
	float4	color;
	float	texId;
};


cbuffer 		CBBatch 	: 	register(b0) { BATCH Batch : packoffset( c0 ); }	
SamplerState	Sampler		: 	register(s0);
Texture2D		Texture 	: 	register(t0);
StructuredBuffer<INSTANCE>	InstanceData : register(t1);


 
/*-----------------------------------------------------------------------------
	Shader functions :
-----------------------------------------------------------------------------*/

// random noise [0;1]
float2 random( float2 input )
{
	float2 sincos_noise;
	sincos(dot( input.xy, float2(12.9898,78.233)),sincos_noise.x,sincos_noise.y);
	return frac(sincos_noise * 43758.5453 + input.yx);
}

PS_IN VSMain( VS_IN input )
{
	PS_IN output = (PS_IN)0;

	/*float2 rnd1 = random( float2(input.instanceID, input.instanceID) );
	float2 rnd2 = random( rnd1 );
	
	float4 randColor = float4( random(float2(input.instanceID, input.instanceID*3)), random(float2(input.instanceID*7, input.instanceID*11)) );

	float2 sinCos;
	float rotationPhase0 = rnd2.x * 10.0f;
	float rotationSign = (((int)input.instanceID & 1) << 1) - 1;
	float rotationSpeed = saturate(rnd2.x + rnd2.y);
	sincos( rotationPhase0 + rotationSign * rotationSpeed * Batch.Time, sinCos.x, sinCos.y);

	float  scale = min( 0.5f, 0.1f + 0.075f * (rnd2.x + rnd2.y) );
	float2 offset = 1.25f * (2.0f * rnd1 - 1.0f);
	float4 rotation = input.pos.xyyx * sinCos.xyxy;
	float2 pos = offset + scale * float2(rotation.w - rotation.z, rotation.x + rotation.y);*/
	//float
	float	scale	=	InstanceData[ input.instanceID ].size;
	float2	offset	=	InstanceData[ input.instanceID ].offset;
	float4	color	=	InstanceData[ input.instanceID ].color;
	float	a		=	InstanceData[ input.instanceID ].rotation;
	float2x2 rot	=	float2x2( cos(a), sin(a), -sin(a), cos(a) );

	output.pos	= mul( float4(mul(input.pos.xy, rot) * scale + offset, 0.0f ,1.0f ), Batch.Transform);
	output.col	= color;
	output.tc.y = input.tc.y;
	output.tc.x = input.tc.x * 0.25f + 0.25f * InstanceData[ input.instanceID ].texId;

	return output;
}


float4 PSMain( PS_IN input ) : SV_Target
{
	float4	color	=	float4(1,1,1,1);
	color	=	input.col;
	color	*=	Texture.Sample( Sampler, input.tc );	
	
	return color;
}


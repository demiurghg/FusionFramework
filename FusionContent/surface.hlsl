
struct BATCH {
	float4x4	Projection		;
	float4x4	View			;
	float4x4	World			;
	float4		ViewPos			;
	float4		BiasSlopeFar	;
};

struct VSInput {
	float3 Position : POSITION;
	float3 Tangent 	: TANGENT;
	float3 Binormal	: BINORMAL;
	float3 Normal 	: NORMAL;
	float4 Color 	: COLOR;
	float2 TexCoord : TEXCOORD0;
};

struct PSInput {
	float4 	Position 	: SV_POSITION;
	float4 	Color 		: COLOR;
	float2 	TexCoord 	: TEXCOORD0;
	float3	Tangent 	: TEXCOORD1;
	float3	Binormal	: TEXCOORD2;
	float3	Normal 		: TEXCOORD3;
	float4	ProjPos		: TEXCOORD4;
};

struct GBuffer {
	float4	hdr		 : SV_Target0;
	float4	diffuse	 : SV_Target1;
	float4	specular : SV_Target2;
	float4	normals	 : SV_Target3;
};

cbuffer 		CBBatch 	: 	register(b0) { BATCH Batch : packoffset( c0 ); }	
SamplerState	Sampler		: 	register(s0);
Texture2D		DiffuseTexture 		: 	register(t0);
Texture2D		SpecularTexture 	: 	register(t1);
Texture2D		NormalMapTexture 	: 	register(t2);
Texture2D		EmissionTexture 	: 	register(t3);

#if 0
$ubershader GBUFFER|SHADOW
#endif
 
/*-----------------------------------------------------------------------------
	Shader functions :
-----------------------------------------------------------------------------*/
PSInput VSMain( VSInput input )
{
	PSInput output;
	
	float4 	pos			=	float4( input.Position, 1 );
	float4	wPos		=	mul( pos,  Batch.World 		);
	float4	vPos		=	mul( wPos, Batch.View 		);
	float4	pPos		=	mul( vPos, Batch.Projection );
	float4	normal		=	mul( float4(input.Normal,0),  Batch.World 		);
	float4	tangent		=	mul( float4(input.Tangent,0),  Batch.World 		);
	float4	binormal	=	mul( float4(input.Binormal,0),  Batch.World 	);
	
	output.Position 	= 	pPos;
	output.ProjPos		=	pPos;
	output.Color 		= 	1;
	output.TexCoord		= 	input.TexCoord;
	output.Normal		= 	normalize(normal.xyz);
	output.Tangent 		=  	normalize(tangent.xyz);
	output.Binormal		=  	normalize(binormal.xyz);
	
	return output;
}


#ifdef GBUFFER
GBuffer PSMain( PSInput input )
{
	GBuffer output;

	float3x3 tbnToWorld	= float3x3(
			input.Tangent.x,	input.Tangent.y,	input.Tangent.z,	
			input.Binormal.x,	input.Binormal.y,	input.Binormal.z,	
			input.Normal.x,		input.Normal.y,		input.Normal.z		
		);
	
	
	float3	white		=	float3(1,1,1);
	float3 	diffuse		=	DiffuseTexture.Sample( Sampler, input.TexCoord ).xyz;
	float3 	specular	=	SpecularTexture.Sample( Sampler, input.TexCoord ).xyz;
	float3 	normal		=	NormalMapTexture.Sample( Sampler, input.TexCoord ).xyz * 2 - 1;// input.WNormal.xyz;
	float3 	normalBias	=	NormalMapTexture.SampleBias( Sampler, input.TexCoord,0 ).xyz * 2 - 1;// input.WNormal.xyz;
	float3 	emission	=	EmissionTexture.Sample( Sampler, input.TexCoord ).xyz * 100;// input.WNormal.xyz;
	
	//specular	=	float3(1,0.2,1);
	
	// specular AA :
	float nl  = saturate((length( normalBias )-0.7)*3.33);
	float f  = 1 - nl;
	//specular.r	=	lerp( specular.r, 0, pow(1-nl,0.5) );
	specular.g 	= 	lerp( specular.g, 0.8, 1-nl );
			
	//	decode specular :
	float  	roughness	=	specular.g;
	float3	specular2	=	lerp( white, diffuse, specular.b ) * specular.r;
			diffuse		=	diffuse * (1-specular.r);
			
	float3 worldNormal 	= 	normalize( mul( normal, tbnToWorld ) );

	output.hdr		=	float4( emission, 0 );
	output.diffuse	=	float4( diffuse, 1 );
	output.specular =	float4( specular2, roughness );
	output.normals	=	float4( worldNormal * 0.5 + 0.5, 0 );
	
	return output;
}
#endif


#ifdef SHADOW
float4 PSMain( PSInput input ) : SV_TARGET0
{
	float z		= input.ProjPos.z / Batch.BiasSlopeFar.z;

	float dzdx	 = ddx(z);
	float dzdy	 = ddy(z);
	float slope = abs(dzdx) + abs(dzdy);

	return z + Batch.BiasSlopeFar.x + slope * Batch.BiasSlopeFar.y;
}
#endif







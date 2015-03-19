#if 0
$ubershader FXAA|COPY|DOWNSAMPLE_4
$ubershader (DOWNSAMPLE_2_2x2..TO_CUBE_FACE)|(DOWNSAMPLE_2_4x4..TO_CUBE_FACE)
$ubershader GAUSS_BLUR_3x3 PASS1|PASS2
$ubershader GAUSS_BLUR PASS1|PASS2
$ubershader LINEARIZE_DEPTH|RESOLVE_AND_LINEARIZE_DEPTH_MSAA
#endif


//-------------------------------------------------------------------------------
#ifdef COPY

Texture2D	Source : register(t0);

float4 VSMain(uint VertexID : SV_VertexID) : SV_POSITION
{
	return float4((VertexID == 0) ? 3.0f : -1.0f, (VertexID == 2) ? 3.0f : -1.0f, 1.0f, 1.0f);
}

float4 PSMain(float4 position : SV_POSITION) : SV_Target
{
	return Source.Load(int3(position.xy, 0));
}

#endif

//-------------------------------------------------------------------------------
#ifdef DOWNSAMPLE_2_2x2

SamplerState	SamplerLinearClamp : register(s0);
Texture2D Source : register(t0);

struct PS_IN {
    float4 position : SV_POSITION;
  	float2 uv : TEXCOORD0;
};

PS_IN VSMain(uint VertexID : SV_VertexID)
{
	PS_IN output;
	output.position.x = (VertexID == 0) ? 3.0f : -1.0f;
	output.position.y = (VertexID == 2) ? 3.0f : -1.0f;
	output.position.zw = 1.0f;

	output.uv = output.position.xy * float2(0.5f, -0.5f) + 0.5f;

#ifdef TO_CUBE_FACE
	output.uv = output.position.xy * float2(-0.5f, -0.5f) + 0.5f;
#endif  

	return output;
}

float4 PSMain(PS_IN input) : SV_Target
{
	return Source.SampleLevel(SamplerLinearClamp, input.uv, 0);
}

#endif

//-------------------------------------------------------------------------------
#ifdef DOWNSAMPLE_2_4x4

SamplerState	SamplerLinearClamp : register(s0);
Texture2D Source : register(t0);

struct PS_IN {
    float4 position : SV_POSITION;
  	float4 uv0 : TEXCOORD0;
	float4 uv1 : TEXCOORD1;
};

PS_IN VSMain(uint VertexID : SV_VertexID)
{
	PS_IN output;
	output.position = float4((VertexID == 0) ? 3.0f : -1.0f, (VertexID == 2) ? 3.0f : -1.0f, 1.0f, 1.0f);

#ifdef TO_CUBE_FACE
	float2 uv0 = output.position.xy * float2(-0.5f, -0.5f) + 0.5f;
#else
	float2 uv0 = output.position.xy * float2(0.5f, -0.5f) + 0.5f;
#endif  

	float texWidth, texHeight;
	Source.GetDimensions(texWidth, texHeight);

	float2 texelSize = float2(1.0f/texWidth, 1.0f/texHeight);

	output.uv0.xy = uv0 + texelSize * float2(-1.0f, -1.0f);
	output.uv0.zw = uv0 + texelSize * float2( 1.0f, -1.0f);
	output.uv1.xy = uv0 + texelSize * float2(-1.0f,  1.0f);
	output.uv1.zw = uv0 + texelSize * float2( 1.0f,  1.0f);

	return output;
}

float4 PSMain(PS_IN input) : SV_Target
{
	float4 sample0 = Source.SampleLevel(SamplerLinearClamp, input.uv0.xy, 0);
	float4 sample1 = Source.SampleLevel(SamplerLinearClamp, input.uv0.zw, 0);
	float4 sample2 = Source.SampleLevel(SamplerLinearClamp, input.uv1.xy, 0);
	float4 sample3 = Source.SampleLevel(SamplerLinearClamp, input.uv1.zw, 0);

	return 0.25f*(sample0 + sample1 + sample2 + sample3);
}

#endif

//-------------------------------------------------------------------------------
#ifdef DOWNSAMPLE_4

SamplerState	SamplerLinearClamp : register(s0);
Texture2D Source : register(t0);

struct PS_IN {
    float4 position : SV_POSITION;
  	float4 uv0 : TEXCOORD0;
    float4 uv1 : TEXCOORD1;
};

PS_IN VSMain(uint VertexID : SV_VertexID)
{
  PS_IN output;
  output.position.x = (VertexID == 0) ? 3.0f : -1.0f;
  output.position.y = (VertexID == 2) ? 3.0f : -1.0f;
  output.position.zw = 1.0f;

  float texWidth, texHeight;
  Source.GetDimensions(texWidth, texHeight);

  float2 texelSize1 = float2(1.0f / texWidth, 1.0f / texHeight);
  float2 texelSize2 = float2(texelSize1.x, -texelSize1.y);
  float2 uv = output.position.xy * float2(0.5f, -0.5f) + 0.5f;

  output.uv0 = float4(uv + texelSize1, uv - texelSize1);
  output.uv1 = float4(uv + texelSize2, uv - texelSize2);

  return output;
}

float4 PSMain(PS_IN input) : SV_Target
{
  float4 sample0 = Source.SampleLevel(SamplerLinearClamp, input.uv0.xy, 0);
  float4 sample1 = Source.SampleLevel(SamplerLinearClamp, input.uv0.zw, 0);
  float4 sample2 = Source.SampleLevel(SamplerLinearClamp, input.uv1.xy, 0);
  float4 sample3 = Source.SampleLevel(SamplerLinearClamp, input.uv1.zw, 0);

	return 0.25f * (sample0 + sample1 + sample2 + sample3);
}

#endif

//-------------------------------------------------------------------------------
#ifdef FXAA

Texture2D		Texture : register(t0);
SamplerState	SamplerLinearClamp : register(s0);

struct VSOutput {
    float4 position : SV_POSITION;
  	float4 uv : TEXCOORD0;
};

VSOutput VSMain(uint VertexID : SV_VertexID)
{
	float texWidth, texHeight;
	Texture.GetDimensions(texWidth, texHeight);
	
	VSOutput output;
	output.position = float4((VertexID == 0) ? 3.0f : -1.0f, (VertexID == 2) ? 3.0f : -1.0f, 1.0f, 1.0f);

	output.uv.xy = output.position.xy * float2(0.5f, -0.5f) + 0.5f;
	output.uv.z = 1.0f / texWidth;
	output.uv.w = 1.0f / texHeight;

	return output;
}

#define FXAA_PC 1
#define FXAA_HLSL_4 1
#define FXAA_QUALITY__SUBPIX 1.0f
#define FXAA_CONSOLE__EDGE_SHARPNESS 2.0
//#define FXAA_CONSOLE__EDGE_SHARPNESS 8.0
#define FXAA_QUALITY__EDGE_THRESHOLD 1/16.0f
#include "fxaa39.fx"

float4 PSMain( VSOutput input) : SV_Target
{
	float2 dudv = input.uv.zw;
	float2 uv   = input.uv.xy + dudv;

	FxaaTex fxaaTex;
	fxaaTex.smpl = SamplerLinearClamp;
	fxaaTex.tex = Texture;
	
	float4 fxaaImage = FxaaPixelShader( uv, float4(uv-dudv/2, uv+dudv/2), fxaaTex, dudv, float4(2 * dudv, 0.5 * dudv) );

	return float4(fxaaImage.rgb, 1);
}

#endif

//-------------------------------------------------------------------------------
#ifdef GAUSS_BLUR_3x3

SamplerState	SamplerLinearClamp : register(s0);
Texture2D Source : register(t0);

struct PS_IN {
    float4 position : SV_POSITION;
  	float2 uv : TEXCOORD0;
};

PS_IN VSMain(uint VertexID : SV_VertexID)
{
  PS_IN output;
  output.position.x = (VertexID == 0) ? 3.0f : -1.0f;
  output.position.y = (VertexID == 2) ? 3.0f : -1.0f;
  output.position.zw = 1.0f;

  float texWidth, texHeight;
  Source.GetDimensions(texWidth, texHeight);

  float2 uv = output.position.xy * float2(0.5f, -0.5f) + 0.5f;

  #ifdef PASS2
     output.uv = uv + float2( 0.5f / texWidth, -0.5f / texHeight);
  #endif
  #ifdef PASS1
     output.uv = uv + float2(-0.5f / texWidth,  0.5f / texHeight);
  #endif

  return output;
}

float4 PSMain(PS_IN input) : SV_Target
{
  return Source.SampleLevel(SamplerLinearClamp, input.uv, 0);
}

#endif

//-------------------------------------------------------------------------------
#ifdef GAUSS_BLUR

static const int MaxBlurTaps = 33;

cbuffer GaussWeightsCB : register(b0) {
  float4 Weights[MaxBlurTaps];
};

SamplerState	SamplerLinearClamp : register(s0);
Texture2D Source : register(t0);

struct PS_IN {
    float4 position : SV_POSITION;
  	float2 uv : TEXCOORD0;
    float2 texelSize : TEXCCORD1;
};


PS_IN VSMain(uint VertexID : SV_VertexID)
{
	PS_IN output;
	output.position.x = (VertexID == 0) ? 3.0f : -1.0f;
	output.position.y = (VertexID == 2) ? 3.0f : -1.0f;
	output.position.zw = 1.0f;

	float texWidth, texHeight;
	Source.GetDimensions(texWidth, texHeight);

	output.uv = output.position.xy * float2(0.5f, -0.5f) + 0.5f;

	#ifdef PASS2
		output.texelSize = float2(0.0f, 1.0f / texHeight);
	#endif
	#ifdef PASS1
		output.texelSize = float2(1.0f / texWidth, 0.0f);
	#endif

	return output;
}

float4 PSMain(PS_IN input) : SV_Target
{
	float4 color = Source.SampleLevel(SamplerLinearClamp, input.uv, 0) * Weights[0].x;

	[unroll]
	for (int i = 1; i <33; ++i) {
		color += Source.SampleLevel(SamplerLinearClamp, input.uv + input.texelSize * Weights[i].w, Weights[i].y) * Weights[i].x;
	}

	return color;
}

#endif

//-------------------------------------------------------------------------------
#ifdef LINEARIZE_DEPTH

cbuffer ConstantBuffer : register(b0)
{
	float linearizeDepthA;
	float linearizeDepthB;
};

Texture2D<float> Depth : register(t0);

float4 VSMain(uint VertexID : SV_VertexID) : SV_POSITION
{
	return float4((VertexID == 0) ? 3.0f : -1.0f, (VertexID == 2) ? 3.0f : -1.0f, 1.0f, 1.0f);
}

float PSMain(float4 position : SV_POSITION) : SV_Target
{
	float depth = Depth.Load(int3(position.xy, 0)).x;
	return 1.0f / (depth * linearizeDepthA + linearizeDepthB);
}

#endif

//----------------------------------------------------------------------------------
#ifdef RESOLVE_AND_LINEARIZE_DEPTH_MSAA

Texture2DMS<float> Depth : register(t0);

float4 VSMain(uint VertexID : SV_VertexID) : SV_POSITION
{
	return float4((VertexID == 0) ? 3.0f : -1.0f, (VertexID == 2) ? 3.0f : -1.0f, 1.0f, 1.0f);
}

float PSMain(float4 position : SV_POSITION) : SV_Target
{
	return 0;
	//return ConvertToViewDepth( Depth.Load(int2(position.xy), 0) );
}

#endif
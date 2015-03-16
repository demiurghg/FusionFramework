

#if 0
$ubershader		(TONEMAPPING LINEAR|REINHARD|FILMIC)|MEASURE_ADAPT
#endif

struct PARAMS {
	float 	AdaptationRate;
	float 	LuminanceLowBound;
	float	LuminanceHighBound;
	float	KeyValue;
	float	BloomAmount;
};

Texture2D		SourceHdrImage 		: register(t0);
Texture2D		MasuredLuminance	: register(t1);
Texture2D		BloomTexture		: register(t2);
Texture2D		BloomMask			: register(t3);
SamplerState	LinearSampler		: register(s0);
	
cbuffer PARAMS 		: register(b0) { 
	PARAMS Params 	: packoffset(c0); 
};


/*
**	FSQuad
*/
float4 FSQuad( uint VertexID ) 
{
	return float4((VertexID == 0) ? 3.0f : -1.0f, (VertexID == 2) ? 3.0f : -1.0f, 1.0f, 1.0f);
}

float2 FSQuadUV ( uint VertexID )
{
	return float2((VertexID == 0) ? 2.0f : -0.0f, 1-((VertexID == 2) ? 2.0f : -0.0f));
}

/*-----------------------------------------------------------------------------
	Luminance Measurement and Adaptation:
	Assumed 128x128 input image.
-----------------------------------------------------------------------------*/
#if MEASURE_ADAPT

float4 VSMain(uint VertexID : SV_VertexID) : SV_POSITION
{
	return FSQuad( VertexID );
}


float4 PSMain(float4 position : SV_POSITION) : SV_Target
{
	float sumLum = 0;
	const float3 lumVector = float3(0.213f, 0.715f, 0.072f );
	
	float oldLum = MasuredLuminance.Load(int3(0,0,0));
	
	for (int x=0; x<32; x++) {
		for (int y=0; y<32; y++) {
			
			sumLum += log( dot( lumVector, SourceHdrImage.Load(int3(x,y,3)).rgb ) + 0.0001f );
		}
	}
	sumLum = clamp( exp(sumLum / 1024.0f), Params.LuminanceLowBound, Params.LuminanceHighBound );
	
	return lerp( oldLum, sumLum, Params.AdaptationRate );
}

#endif


/*-----------------------------------------------------------------------------
	Tonemapping and Final Composing :
-----------------------------------------------------------------------------*/

#ifdef TONEMAPPING

float4 VSMain(uint VertexID : SV_VertexID, out float2 uv : TEXCOORD) : SV_POSITION
{
	uv = FSQuadUV( VertexID );
	return FSQuad( VertexID );
}

static const float dither[4][4] = {{1,9,3,11},{13,5,15,7},{4,12,2,10},{16,8,14,16}};


float4 PSMain(float4 position : SV_POSITION, float2 uv : TEXCOORD0 ) : SV_Target
{
	uint width;
	uint height;
	uint xpos = position.x;
	uint ypos = position.y;
	SourceHdrImage.GetDimensions( width, height );
	
	float3	hdrImage	=	SourceHdrImage.Load(int3(position.xy, 0)).rgb;
	float3	bloom0		=	BloomTexture.SampleLevel( LinearSampler, uv, 0 ).rgb;
	float3	bloom1		=	BloomTexture.SampleLevel( LinearSampler, uv, 1 ).rgb;
	float3	bloom2		=	BloomTexture.SampleLevel( LinearSampler, uv, 2 ).rgb;
	float3	bloom3		=	BloomTexture.SampleLevel( LinearSampler, uv, 3 ).rgb;
	float4	bloomMask	=	BloomMask.SampleLevel( LinearSampler, uv, 0 );
	float 	sum			=	dot( bloomMask, float4(1,1,1,1) );
	float3	bloom		=	( bloom0 * 1  
							+ bloom1 * 1  
							+ bloom2 * 1  
							+ bloom3 * 1 )/4;//*/
							
	bloom	=	bloom * bloomMask.rgb;
                                       
	/*float3	bloom		=	( bloom0 * bloomMask.x  
							+ bloom1 * bloomMask.y  
							+ bloom2 * bloomMask.z  
							+ bloom3 * bloomMask.w )/4;//*/

	/*float3	bloom		=	( bloom0 * 1 * float3(1.0,1.0,1.0) 
							+ bloom1 * 1 * float3(1.0,0.6,0.3) 
							+ bloom2 * 1 * float3(0.5,1.0,0.5) 
							+ bloom3 * 1 * float3(0.3,0.6,1.0))/4;//*/
	
	hdrImage			=	lerp( hdrImage, bloom, Params.BloomAmount);
	
	float	luminance	=	MasuredLuminance.Load(int3(0,0,0)).r;
	float3	exposured	=	Params.KeyValue * hdrImage / luminance;
	
	#ifdef LINEAR
		float3 	tonemapped	=	pow( exposured, 1/2.2f );
	#endif
	
	#ifdef REINHARD
		float3 tonemapped	=	pow( exposured / (1+exposured), 1/2.2f );
	#endif
	
	#ifdef FILMIC
		float3 x = max(0,exposured-0.004);
		float3 tonemapped = (x*(6.2*x+.5))/(x*(6.2*x+1.7)+0.06);
	#endif
	
	
	
	
	
	// dithering :
	tonemapped += dither[(xpos+ypos/7)%4][(ypos+xpos/7)%4]/256.0f/5;
	tonemapped -= dither[(ypos+xpos/7)%4][(xpos+ypos/7)%4]/256.0f/5;//*/
	
	return  float4( tonemapped, dot( tonemapped, float3(0.3f,0.6f,0.2f)) );
}

#endif














#if 0
$ubershader 	HBAO
#endif

struct PARAMS {
	float4x4	ProjMatrix;
	float4x4	View;
	float4x4	ViewProjection;
	float4x4	InvViewProjection;
	float 		TraceStep;
	float 		DecayRate;
};

Texture2D		DepthTexture 		: register(t0);
Texture2D		NormalsTexture 		: register(t1);
Texture2D		RandomTexture 		: register(t2);
SamplerState	LinearSampler		: register(s0);
	
cbuffer PARAMS 		: register(b0) { 
	PARAMS Params 	: packoffset(c0); 
};


float4 FSQuad( uint VertexID ) 
{
	return float4((VertexID == 0) ? 3.0f : -1.0f, (VertexID == 2) ? 3.0f : -1.0f, 1.0f, 1.0f);
}

float2 FSQuadUV ( uint VertexID )
{
	return float2((VertexID == 0) ? 2.0f : -0.0f, 1-((VertexID == 2) ? 2.0f : -0.0f));
}

/*-----------------------------------------------------------------------------
	SSAO
-----------------------------------------------------------------------------*/
#ifdef HBAO

static const float pattern[4][4] = {{1,9,3,11},{13,5,15,7},{4,12,2,10},{16,8,14,16}};


float4 VSMain(uint VertexID : SV_VertexID, out float2 uv : TEXCOORD0 ) : SV_POSITION
{
	uv = FSQuadUV( VertexID );
	return FSQuad( VertexID );

}


float LinearZ( float depth, float4x4 proj )
{
	return abs(proj._43 / ( proj._33 + depth ));
}


float4 PSMain(float4 position : SV_POSITION, float2 uv : TEXCOORD0 ) : SV_Target
{
	uint width;
	uint height;
	DepthTexture.GetDimensions( width, height );
	
	uint xpos = position.x;
	uint ypos = position.y;
	int samples = 16;
	float radius = 1.5;
	float bias = 0.1	;

	float	depth 	 	=	DepthTexture.Load( int3(xpos,ypos,0) ).r;
	float3	wsNormal	=	normalize(NormalsTexture.Load( int3(xpos,ypos,0) ) * 2 - 1);
	float3	randDir		=	normalize(RandomTexture.Load( int3(xpos%8,ypos%8,0) ) * 2 - 1);
	
	float4	projPos		=	float4( position.x/(float)width*2-1, position.y/(float)height*(-2)+1, depth, 1 );
	float4	worldPos	=	mul( projPos, Params.InvViewProjection );
			worldPos	/=	worldPos.w;
			
	float	sceneDepth	=	LinearZ( depth, Params.ProjMatrix );
	
	//return frac(worldPos);
	//return frac(sceneDepth);
	
	float	occlusion	=	0;
	
	//return wsNormal.y * 0.5 + 0.5f;

	
	for (int i=0; i<samples; i++) {
		float3 newRandDir	=	RandomTexture.Load( int3((i*117)%37, (i*113)%39, 0) ) * 2 - 1;
		newRandDir	=	reflect( newRandDir, randDir );
		newRandDir	=	faceforward( newRandDir, newRandDir, -wsNormal );
		
		float4	samplePos	=	mul( worldPos + float4(newRandDir*radius,0), Params.ViewProjection );
				samplePos	=	samplePos / samplePos.w;
				samplePos.xy=	samplePos.xy * float2(0.5,-0.5) + float2(0.5f,0.5f);
				
		float	sampleDepth	=	LinearZ( DepthTexture.Sample( LinearSampler, samplePos.xy ).r, Params.ProjMatrix ) + bias;
		
		float	depthDelta	=	sceneDepth - sampleDepth;
		float	rho			=	saturate( (depthDelta - radius ) / depthDelta ) ;

		occlusion	+= rho;
	}//*/
	
	occlusion	/= samples;

	return pow(saturate(occlusion),2);
}

#endif



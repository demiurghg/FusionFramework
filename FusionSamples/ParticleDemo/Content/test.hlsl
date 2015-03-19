
#if 0
$ubershader INJECTION|SIMULATION|DRAW
#endif

#define BLOCK_SIZE	512

struct PARTICLE {
	float2	Position;
	float2	Velocity;
	float2	Acceleration;
	float4	Color0;
	float4	Color1;
	float	Size0;
	float	Size1;
	float	Angle0;
	float	Angle1;
	float	TotalLifeTime;
	float	LifeTime;
	float	FadeIn;
	float	FadeOut;
};

struct PARAMS {
	float4x4	View;
	float4x4	Projection;
	int			MaxParticles;
	float		DeltaTime;
};

cbuffer CB1 : register(b0) { 
	PARAMS Params; 
};

SamplerState						Sampler				: 	register(s0);
Texture2D							Texture 			: 	register(t0);
StructuredBuffer<PARTICLE>			particleBufferSrc	: 	register(t1);
AppendStructuredBuffer<PARTICLE>	particleBufferDst	: 	register(u0);

/*-----------------------------------------------------------------------------
	Simulation :
-----------------------------------------------------------------------------*/
#if (defined INJECTION) || (defined SIMULATION)
[numthreads( BLOCK_SIZE, 1, 1 )]
void CSMain( 
	uint3 groupID			: SV_GroupID,
	uint3 groupThreadID 	: SV_GroupThreadID, 
	uint3 dispatchThreadID 	: SV_DispatchThreadID,
	uint  groupIndex 		: SV_GroupIndex
)
{
	int id = dispatchThreadID.x;

#ifdef INJECTION
	if (id < Params.MaxParticles) {
		PARTICLE p = particleBufferSrc[ id ];
		
		if (p.LifeTime < p.TotalLifeTime) {
			particleBufferDst.Append( p );
		}
	}
#endif

#ifdef SIMULATION
	if (id < Params.MaxParticles) {
		PARTICLE p = particleBufferSrc[ id ];
		
		if (p.LifeTime < p.TotalLifeTime) {
			p.LifeTime += Params.DeltaTime;
			particleBufferDst.Append( p );
		}
	}
#endif
}
#endif


/*-----------------------------------------------------------------------------
	Rendering :
-----------------------------------------------------------------------------*/

struct VSOutput {
	int vertexID : TEXCOORD0;
};

struct GSOutput {
	float4	Position : SV_Position;
	float2	TexCoord : TEXCOORD0;
	float2	TexCoord1 : TEXCOORD1;
	float 	TexCoord2 : TEXCOORD2;
	float3	TexCoord3 : TEXCOORD3;
	float4	Color    : COLOR0;
	int4	Color1    : COLOR1;
};


#if DRAW
VSOutput VSMain( uint vertexID : SV_VertexID )
{
	VSOutput output;
	output.vertexID = vertexID;
	return output;
}


float Ramp(float f_in, float f_out, float t) 
{
	float y = 1;
	t = saturate(t);
	
	float k_in	=	1 / f_in;
	float k_out	=	-1 / (1-f_out);
	float b_out =	-k_out;	
	
	if (t<f_in)  y = t * k_in;
	if (t>f_out) y = t * k_out + b_out;
	
	return y;
}



[maxvertexcount(6)]
void GSMain( point VSOutput inputPoint[1], inout TriangleStream<GSOutput> outputStream )
{
	GSOutput p0, p1, p2, p3;
	
	p0.TexCoord1 = 0; p0.TexCoord2 = 0; p0.TexCoord3 = 0; p0.Color1 = 0;
	p1.TexCoord1 = 0; p1.TexCoord2 = 0; p1.TexCoord3 = 0; p1.Color1 = 0;
	p2.TexCoord1 = 0; p2.TexCoord2 = 0; p2.TexCoord3 = 0; p2.Color1 = 0;
	p3.TexCoord1 = 0; p3.TexCoord2 = 0; p3.TexCoord3 = 0; p3.Color1 = 0;
	
	PARTICLE prt = particleBufferSrc[ inputPoint[0].vertexID ];
	
	if (prt.LifeTime >= prt.TotalLifeTime ) {
		return;
	}
	
	float factor	=	saturate(prt.LifeTime / prt.TotalLifeTime);
	
	float  sz 		=   lerp( prt.Size0, prt.Size1, factor )/2;
	float  time		=	prt.LifeTime;
	float4 color	=	lerp( prt.Color0, prt.Color1, Ramp( prt.FadeIn, prt.FadeOut, factor ) );
	float2 position	=	prt.Position + prt.Velocity * time + prt.Acceleration * time * time / 2;
	float  a		=	lerp( prt.Angle0, prt.Angle1, factor );	

	float2x2	m	=	float2x2( cos(a), sin(a), -sin(a), cos(a) );
	
	p0.Position	= mul( float4( position + mul(float2( sz, sz), m), 0, 1 ), Params.Projection );
	p0.TexCoord	= float2(1,1);
	p0.Color 	= color;
	
	p1.Position	= mul( float4( position + mul(float2(-sz, sz), m), 0, 1 ), Params.Projection );
	p1.TexCoord	= float2(0,1);
	p1.Color 	= color;
	
	p2.Position	= mul( float4( position + mul(float2(-sz,-sz), m), 0, 1 ), Params.Projection );
	p2.TexCoord	= float2(0,0);
	p2.Color 	= color;
	
	p3.Position	= mul( float4( position + mul(float2( sz,-sz), m), 0, 1 ), Params.Projection );
	p3.TexCoord	= float2(1,0);
	p3.Color 	= color;

	outputStream.Append(p0);
	outputStream.Append(p1);
	outputStream.Append(p2);
	
	outputStream.RestartStrip();

	outputStream.Append(p0);
	outputStream.Append(p2);
	outputStream.Append(p3);

	outputStream.RestartStrip();
}



float4 PSMain( GSOutput input ) : SV_Target
{
	return Texture.Sample( Sampler, input.TexCoord ) * float4(input.Color.rgb,1);
}
#endif


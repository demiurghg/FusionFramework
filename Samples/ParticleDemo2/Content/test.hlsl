
#if 0
$ubershader INJECTION|SIMULATION|RENDER
#endif

struct IN_PARTICLE {
	float4	Position	 : POSITION;
	float4	Color0		 : COLOR0;
	float4	Color1		 : COLOR1;
	float4	VelAccel	 : TEXCOORD0;
	float4	SizeAngle	 : TEXCOORD1;
	float4	Timing		 : TEXCOORD2; // total, lifetime, fade-in, fade-out
};

struct OUT_PARTICLE {
	float4	Position	 : SV_POSITION;
	float4	Color0		 : COLOR0;
	float4	Color1		 : COLOR1;
	float4	VelAccel	 : TEXCOORD0;
	float4	SizeAngle	 : TEXCOORD1;
	float4	Timing		 : TEXCOORD2; // total, lifetime, fade-in, fade-out
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

SamplerState	Sampler				: 	register(s0);
Texture2D		Texture 			: 	register(t0);



/*-----------------------------------------------------------------------------
	Rendering :
-----------------------------------------------------------------------------*/

struct GSOutput {
	float4	Position  : SV_Position;
	float2	TexCoord  : TEXCOORD0;
	float4	Color     : COLOR0;
};

#if (defined SIMULATION) || (defined INJECTION) || (defined RENDER)
OUT_PARTICLE VSMain( IN_PARTICLE p, uint id : SV_VertexID )
{
	OUT_PARTICLE op;
	op.Position	 	=	float4(p.Position.xy,0,1);	 
	op.Color0		=	p.Color0		;  
	op.Color1		=	p.Color1		;  
	op.VelAccel	    =	p.VelAccel	    ;
	op.SizeAngle	=	p.SizeAngle	    ;
	op.Timing		=	p.Timing		;  
	
	return op;
}
#endif


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


#ifdef SIMULATION
[maxvertexcount(1)]
void GSMain( point OUT_PARTICLE inputPoint[1], inout PointStream<OUT_PARTICLE> outputStream )
{
	OUT_PARTICLE p = inputPoint[0];
	if (p.Timing.y < p.Timing.x) {
		p.Timing.y += Params.DeltaTime;
		outputStream.Append( p );
	}
}
#endif


#ifdef INJECTION
[maxvertexcount(1)]
void GSMain( point OUT_PARTICLE inputPoint[1], inout PointStream<OUT_PARTICLE> outputStream )
{
	OUT_PARTICLE p = inputPoint[0];
	//if (p.Timing.y < p.Timing.x) {
		outputStream.Append( p );
	//}
}
#endif



#ifdef RENDER
[maxvertexcount(6)]
void GSMain( point OUT_PARTICLE inputPoint[1], inout TriangleStream<GSOutput> outputStream )
{
	GSOutput p0, p1, p2, p3;
	
	OUT_PARTICLE prt = inputPoint[0];
	
	if (prt.Timing.y > prt.Timing.x) {
		//return;
	}
	
	float factor	=	saturate(prt.Timing.y / prt.Timing.x);
	
	float  sz 		=   lerp( prt.SizeAngle.x, prt.SizeAngle.y, factor )/2;
	float  time		=	prt.Timing.y;
	float4 color	=	lerp( prt.Color0, prt.Color1, Ramp( prt.Timing.z, prt.Timing.w, factor ) );
	float2 position	=	prt.Position.xy + prt.VelAccel.xy * time + prt.VelAccel.zw * time * time / 2;
	float  a		=	lerp( prt.SizeAngle.z, prt.SizeAngle.w, factor );	

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


/*-----------------------------------------------------------------------------
	CONSTANT Tables
-----------------------------------------------------------------------------*/
struct ConstData {
	float4x4	ViewProj	;
	float4		Zoom		;
	float4		Offset		;
	float4		ViewPosition;
};

struct VS_INPUT {	
	float2	Position	: POSITION;		//	Mercator position
	float4	Color		: COLOR0	;
	float4	WLHRot		: TEXCOORD0	;
};


struct VS_OUTPUT {
    float4 Position	: SV_POSITION;
	float4 Color	: COLOR0	;
	float4 WLHRot	: TEXCOORD0	;
};

struct GS_OUTPUT {
    float4 Position		: SV_POSITION	;
	float4 Color		: COLOR0		;
	float3 Normal		: TEXCOORD0		;
	float3 ViewVec		: TEXCOORD1		;
};


cbuffer CBStage : register(b0) 	{	ConstData	Stage	: 	packoffset( c0 );	}	


/*-------------------------------------------------------------------------------------------------
	Particle rendering :
-------------------------------------------------------------------------------------------------*/
#ifdef BUILDINGS_DRAW

VS_OUTPUT VSMain ( VS_INPUT v )
{
	VS_OUTPUT	output;
	
	// Calculate according to Zoom and Offset
	float2 pos = float2(v.Position.x, v.Position.y)*Stage.Zoom.x + Stage.Offset.xy;

	output.Position	=	float4(pos.x, 0.1f, pos.y, 1);//mul(float4(pos.x, 0.1f, pos.y, 1), Stage.ViewProj);
	output.Color	=	v.Color;
	output.WLHRot	=	v.WLHRot;

	return output;
}


[maxvertexcount(24)]
void GSMain ( point VS_OUTPUT inputArray[1], inout TriangleStream<GS_OUTPUT> stream )
{
	GS_OUTPUT	output;
	VS_OUTPUT	input	=	inputArray[0];

	float w = (input.WLHRot.x/2.0f) * Stage.Zoom.x * 0.0000001f;
	float l = (input.WLHRot.y/2.0f) * Stage.Zoom.x * 0.0000001f;
	float h = input.WLHRot.z * Stage.Zoom.x * 0.0000001f;

	float x = input.Position.x;
	float y = input.Position.y;
	float z = input.Position.z;

	

	// Top
	output.Position	=	mul(float4(x + w, y + h, z + l, 1), Stage.ViewProj);
	output.Color	=	input.Color;
	output.Normal	= float3(0, 1, 0);
	output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	stream.Append( output );
	
	output.Position	=	mul(float4(x - w, y + h, z + l, 1), Stage.ViewProj);
	output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	stream.Append( output );
	
	output.Position	=	mul(float4(x + w, y + h, z - l, 1), Stage.ViewProj);
	output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	stream.Append( output );
	
	output.Position	=	mul(float4(x - w, y + h, z - l, 1), Stage.ViewProj);
	output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	stream.Append( output );
    
	stream.RestartStrip();
	
	
	// Bottom
	output.Position	=	mul(float4(x - w, y - h, z - l, 1), Stage.ViewProj);
	output.Normal	= float3(0, -1, 0);
	output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	stream.Append( output );

	output.Position	=	mul(float4(x - w, y - h, z + l, 1), Stage.ViewProj);
	output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	stream.Append( output );
	
	output.Position	=	mul(float4(x + w, y - h, z - l, 1), Stage.ViewProj);
	output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	stream.Append( output );

	output.Position	=	mul(float4(x + w, y - h, z + l, 1), Stage.ViewProj);
	output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	stream.Append( output );
	
	stream.RestartStrip();

	
	// Right
	output.Position	=	mul(float4(x + w, y - h, z - l, 1), Stage.ViewProj);
	output.Normal	= float3(0, 0, 1);
	output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	stream.Append( output );

	output.Position	=	mul(float4(x + w, y - h, z + l, 1), Stage.ViewProj);
	output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	stream.Append( output );
	
	output.Position	=	mul(float4(x + w, y + h, z - l, 1), Stage.ViewProj);
	output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	stream.Append( output );

	output.Position	=	mul(float4(x + w, y + h, z + l, 1), Stage.ViewProj);
	output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	stream.Append( output );
	
	stream.RestartStrip();

	
	// Left
	output.Position	=	mul(float4(x - w, y + h, z - l, 1), Stage.ViewProj);
	output.Normal	= float3(0, 0, -1);
	output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	stream.Append( output );

	output.Position	=	mul(float4(x - w, y + h, z + l, 1), Stage.ViewProj);
	output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	stream.Append( output );

	output.Position	=	mul(float4(x - w, y - h, z - l, 1), Stage.ViewProj);
	output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	stream.Append( output );

	output.Position	=	mul(float4(x - w, y - h, z + l, 1), Stage.ViewProj);
	output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	stream.Append( output );
	
	stream.RestartStrip();
	
	
	// Front
	output.Position	=	mul(float4(x + w, y + h, z + l, 1), Stage.ViewProj);
	output.Normal	= float3(0, 0, 1);
	output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	stream.Append( output );

	output.Position	=	mul(float4(x + w, y - h, z + l, 1), Stage.ViewProj);
	output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	stream.Append( output );

	output.Position	=	mul(float4(x - w, y + h, z + l, 1), Stage.ViewProj);
	output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	stream.Append( output );

	output.Position	=	mul(float4(x - w, y - h, z + l, 1), Stage.ViewProj);
	output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	stream.Append( output );
	
	stream.RestartStrip();

	// Back
	output.Position	=	mul(float4(x - w, y + h, z - l, 1), Stage.ViewProj);
	output.Normal	= float3(0, 0, -1);
	output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	stream.Append( output );

	output.Position	=	mul(float4(x - w, y - h, z - l, 1), Stage.ViewProj);
	output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	stream.Append( output );

	output.Position	=	mul(float4(x + w, y + h, z - l, 1), Stage.ViewProj);
	output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	stream.Append( output );

	output.Position	=	mul(float4(x + w, y - h, z - l, 1), Stage.ViewProj);
	output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	stream.Append( output );
		
	stream.RestartStrip();
}



float4 PSMain ( GS_OUTPUT input ) : SV_Target
{
	float v = 0.5 * (1 + dot(input.ViewVec, input.Normal));
	
	float4 res = v * input.Color;
	res.a = 0.0f;
	return res;

	//float4 	color	=	input.Color;
	//return color;
}

#endif


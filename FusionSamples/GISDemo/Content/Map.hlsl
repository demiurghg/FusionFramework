/*-----------------------------------------------------------------------------
	CONSTANT Tables
-----------------------------------------------------------------------------*/
struct ConstData {
	float4x4	ViewProj	;
	float4		Zoom		;
	float4		Offset		;
	float4		TesAmount	;
    float4		Padding		;
};

struct VS_INPUT {	
	float4	Position	: POSITION	;		//	Mercator position
	float4	Tex			: TEXCOORD0	;
};

struct VS_OUTPUT {
    float3 Position	: POSITION	;
	float4 Color	: COLOR0	;
	float4 Tex		: TEXCOORD0	;
	float2 World	: TEXCOORD1	;
};

cbuffer CBStage : register(b0) 	{	ConstData	Stage	: 	packoffset( c0 );	}	


/*-------------------------------------------------------------------------------------------------
	Particle rendering :
-------------------------------------------------------------------------------------------------*/
#ifdef MAP_DRAW

VS_OUTPUT VSMain ( VS_INPUT v )
{
	VS_OUTPUT	output;
	
	// Calculate according to Zoom and Offset
	float2 pos		= float2(v.Position.x, v.Position.y);

	output.Position	=	float3(pos.x, 0.0f, pos.y); //mul(float4(pos.x, 0.0f, pos.y, 1), Stage.ViewProj);
	output.Color	=	float4(1.0f, 1.0f, 1.0f, 1.0f);
	output.Tex		=	v.Tex;
	output.World	=	v.Position.zw;

	return output;
}


////////////////////////////////////////////////
// Hull Shader Part
////////////////////////////////////////////////
struct PATCH_OUTPUT
{
    float edges[4]	: SV_TessFactor;
    float inside[2]	: SV_InsideTessFactor;
};

struct HULL_OUTPUT
{
    float3 Position : POSITION;
    float4 Color	: COLOR;
	float4 Tex		: TEXCOORD0	;
	float2 World	: TEXCOORD1	;
};

// Patch Constant Function
PATCH_OUTPUT ColorPatchConstantFunction(InputPatch<VS_OUTPUT, 4> inputPatch, uint patchId : SV_PrimitiveID)
{    
    PATCH_OUTPUT output;


    // Set the tessellation factors for the three edges of the triangle.
    output.edges[0] = Stage.TesAmount.x;
    output.edges[1] = Stage.TesAmount.x;
    output.edges[2] = Stage.TesAmount.x;
	output.edges[3] = Stage.TesAmount.x;

    // Set the tessellation factor for tessallating inside the triangle.
    output.inside[0] =  Stage.TesAmount.x;
	output.inside[1] = Stage.TesAmount.x;
    return output;
}

////////////////////////////////////////////////////////////////////////////////
// Hull Shader
[domain("quad")]
[partitioning("integer")]
[outputtopology("triangle_cw")]
[outputcontrolpoints(4)]
[patchconstantfunc("ColorPatchConstantFunction")]
HULL_OUTPUT HSMain(InputPatch<VS_OUTPUT, 4> patch, uint pointId : SV_OutputControlPointID, uint patchId : SV_PrimitiveID)
{
    HULL_OUTPUT output;

    // Set the position for this control point as the output position.
    output.Position	= patch[pointId].Position;
    // Set the input color as the output color.
    output.Color	= patch[pointId].Color;
	output.Tex		= patch[pointId].Tex;
	output.World	= patch[pointId].World;

    return output;
}


struct DOMAIN_OUTPUT
{
    float4	Position	: SV_POSITION;
    float4	Color		: COLOR;
	float2	Tex			: TEXCOORD0	;
	float4	World		: TEXCOORD1	;
};


float2 TileToWorldPos(float tile_x, float tile_y)
{
	float2	p = float2(0,0);
	float	n = 3.1415926f - ((2.0f * 3.1415926f * tile_y));

	p.x = (tile_x * 360.0f) - 180.0f;
	p.y = (180.0f / 3.1415926f) * atan(sinh(n));

	return p;
}

////////////////////////////////////////////////////////////////////////////////
// Domain Shader
Texture2D		ElevationMaps[4]	: register(t1);
SamplerState	Sampler				: register(s0);

[domain("quad")]
DOMAIN_OUTPUT DSMain(PATCH_OUTPUT input, float2 uvwCoord : SV_DomainLocation, const OutputPatch<HULL_OUTPUT, 4> patch)
{
    float3 vertexPosition;
    DOMAIN_OUTPUT output;
 

    // Determine the position of the new vertex.
    //vertexPosition = uvwCoord.x * patch[0].Position + uvwCoord.y * patch[1].Position + uvwCoord.z * patch[2].Position;
	float3	verticalPos1	= lerp(patch[0].Position, patch[1].Position, uvwCoord.y); 
	float3	verticalPos2	= lerp(patch[3].Position, patch[2].Position, uvwCoord.y); 
			vertexPosition	= lerp(verticalPos1, verticalPos2, uvwCoord.x); 

    float3	newPos		= vertexPosition;
			newPos.xz	= newPos.xz * Stage.Zoom.x + Stage.Offset.xy;

	output.World.xy		= vertexPosition.xz;
	output.World.zw		= patch[0].World;

	float2	pos = TileToWorldPos(output.World.x, output.World.y);

	float	elev = ElevationMaps[0].SampleLevel(Sampler, frac(float2(pos.x, 1.0f - pos.y)), 0).x;

	newPos.y = elev*2.5f;

    // Calculate the position of the new vertex against the world, view, and projection matrices.
    output.Position = mul(float4(newPos, 1.0f), Stage.ViewProj);

    // Send the input color into the pixel shader.
    output.Color = patch[0].Color;


	float2 verticalUV1	= lerp(patch[0].Tex.xy, patch[1].Tex.xy, uvwCoord.y); 
	float2 verticalUV2	= lerp(patch[3].Tex.xy, patch[2].Tex.xy, uvwCoord.y); 
	float2 tex			= lerp(verticalUV1, verticalUV2, uvwCoord.x);
	output.Tex			= tex;

    return output;
}

Texture2D		DifTexture	: register(t0);


float4 PSMain ( DOMAIN_OUTPUT input ) : SV_Target
{
	float4 color;

	float2 pos		= TileToWorldPos(input.World.x, input.World.y);
	float2 offset	= input.World.zw - input.World.xy;
	int x = offset.x;
	int y = offset.y;

	if(x == 0 && y == 0)
		color		= ElevationMaps[0].Sample(Sampler, frac(float2(pos.x, 1.0f - pos.y)));
	if(x == 1 && y == 0)
		color		= ElevationMaps[1].Sample(Sampler, frac(float2(pos.x, 1.0f - pos.y)));
	if(x == 0 && y == 1)
		color		= ElevationMaps[2].Sample(Sampler, frac(float2(pos.x, 1.0f - pos.y)));
	if(x == 1 && y == 1)
		color		= ElevationMaps[3].Sample(Sampler, frac(float2(pos.x, 1.0f - pos.y)));

	//color = float4(frac(float2(pos.x, 1.0f - pos.y)), 0.0f, 1.0f);
	//color		= ElevationMaps[0].Sample(Sampler, frac(float2(pos.x, 1.0f - pos.y)));
	color.gb	= color.rr;
	color.rgb	= color.rgb/30.0f;


	
	return color;
}


#endif


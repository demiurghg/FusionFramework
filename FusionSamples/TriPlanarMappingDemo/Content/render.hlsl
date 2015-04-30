
struct BATCH {
	float4x4	ViewProj;
	float4x4	World	;
	float4		ViewPos	;
	float4		LodDist; // x - min LOD, y - max LOD, z - min distance, w - max distance
	float4		ScaleSharp; // x - texture scale, y - Triplanar Blend Sharpness
};

struct VS_IN {
	float3 Position : POSITION;
	half3 Tangent	: TANGENT;
	half3 Normal	: NORMAL;
	half3 Binormal	: BINORMAL;
	float4 Color 	: COLOR;
	float2 Tex		: TEXCOORD0;
};

struct VS_OUT {
	float3 	Position 	: POSITION;
	float4 	Color 		: COLOR;
	float2 	Tex		 	: TEXCOORD0;
	float3 Tangent		: TANGENT;
	float3 Normal		: NORMAL;
	float3 Binormal		: BINORMAL;
};

cbuffer CBBatch 	: 	register(b0) { BATCH Batch : packoffset( c0 ); }	

#if 0
$ubershader Wireframe|None
#endif
 
/*-----------------------------------------------------------------------------
	Shader functions :
-----------------------------------------------------------------------------*/
VS_OUT VSMain( VS_IN input )
{
	VS_OUT output 	= (VS_OUT)0;
	
	output.Position = input.Position;
	output.Color 	= input.Color;
	output.Tex		= input.Tex;
	output.Tangent	= input.Tangent;
	output.Normal	= input.Normal;
	output.Binormal	= input.Binormal;
	
	output.Position = mul(float4(output.Position, 1.0f), Batch.World).xyz;
	
	output.Normal 	= normalize( mul(float4(output.Normal, 		0.0f), Batch.World).xyz);
	output.Tangent 	= normalize( mul(float4(output.Tangent, 	0.0f), Batch.World).xyz);
	output.Binormal = normalize( mul(float4(output.Binormal, 	0.0f), Batch.World).xyz);
	
	return output;
}

////////////////////////////////////////////////
// Hull Shader Part
////////////////////////////////////////////////
struct PATCH_OUTPUT
{
    float edges[3]	: SV_TessFactor;
    float inside	: SV_InsideTessFactor;
};

struct HULL_OUTPUT
{
    float3 Position : POSITION;
    float4 Color	: COLOR;
	float2 Tex		: TEXCOORD0;
	float3 Tangent	: TANGENT;
	float3 Normal	: NORMAL;
	float3 Binormal	: BINORMAL;
};

struct DOMAIN_OUTPUT
{
    float4	Position	: SV_POSITION;
    float4	Color		: COLOR;
	float2	Tex			: TEXCOORD0;
	float3 Tangent		: TANGENT;
	float3 Normal		: NORMAL;
	float3 Binormal		: BINORMAL;
	float3 ViewDir		: TEXCOORD1;
	float3 WorldPos 	: TEXCOORD2;
};


// Patch Constant Function
PATCH_OUTPUT HullShaderConstantFunction(InputPatch<VS_OUT, 3> inputPatch, uint patchId : SV_PrimitiveID)
{    
    PATCH_OUTPUT output;
	
	float minLOD = Batch.LodDist.x;
	float maxLOD = Batch.LodDist.y;
	
	float minDistance = Batch.LodDist.z;
	float maxDistance = Batch.LodDist.w;
	
	float3 cameraPosition = Batch.ViewPos.xyz;
	
	float distanceRange = maxDistance - minDistance;
	float vertex0 = lerp(minLOD, maxLOD, (1.0f - saturate((distance(cameraPosition, (inputPatch[0].Position + inputPatch[1].Position)*0.5f) - minDistance) / distanceRange)));
	float vertex1 = lerp(minLOD, maxLOD, (1.0f - saturate((distance(cameraPosition, (inputPatch[1].Position + inputPatch[2].Position)*0.5f) - minDistance) / distanceRange)));
	float vertex2 = lerp(minLOD, maxLOD, (1.0f - saturate((distance(cameraPosition, (inputPatch[2].Position + inputPatch[0].Position)*0.5f) - minDistance) / distanceRange)));
	
    // Set the tessellation factors for the four edges of the quad.
	
    output.edges[0] = vertex1;
    output.edges[1] = vertex2;
    output.edges[2] = vertex0;
	//output.edges[3] = Batch.Edges.w;

    // Set the tessellation factor for tessellating inside the quad.
    //output.inside = Batch.Inside.x;
	float minTess = min(output.edges[0], min(output.edges[1], output.edges[2]));
	output.inside = minTess;

    return output;
}



////////////////////////////////////////////////////////////////////////////////
// Hull Shader
[domain("tri")]
[partitioning("pow2")]
[outputtopology("triangle_cw")]
[outputcontrolpoints(3)]
[patchconstantfunc("HullShaderConstantFunction")]
HULL_OUTPUT HSMain(InputPatch<VS_OUT, 3> patch, uint pointId : SV_OutputControlPointID, uint patchId : SV_PrimitiveID)
{
    HULL_OUTPUT output;
    // Set the position for this control point as the output position.
    output.Position	= patch[pointId].Position;
    output.Color	= patch[pointId].Color;
	output.Tex		= patch[pointId].Tex;
	output.Normal	= patch[pointId].Normal;
	output.Tangent	= patch[pointId].Tangent;
	output.Binormal	= patch[pointId].Binormal;
	
    return output;
}



float3 project(float3 p, float3 c, float3 n)
{
    return p - dot(p - c, n) * n;
}

// Computes the position of a point in the Phong Tessellated triangle
float3 PhongGeometry(float u, float v, float w, float3 p0, float3 p1, float3 p2, float3 n0, float3 n1, float3 n2, float alpha)
{
    // Find local space point
    float3 p = u * p0 + v * p1 + w * p2;
    // Find projected vectors
    float3 c0 = project(p, p0, n0);
    float3 c1 = project(p, p1, n1);
    float3 c2 = project(p, p2, n2);
    // Interpolate
    float3 q = u * c0 + v * c1 + w * c2;
    // For blending between tessellated and untessellated model:
    q = lerp(p, q, alpha);
    return q;
}

// Computes the normal of a point in the Phong Tessellated triangle
float3 PhongNormal(float3 uvw, float3 n0, float3 n1, float3 n2)
{
    // Interpolate
    return normalize(uvw.x * n0 + uvw.y * n1 + uvw.z * n2);
}


Texture2D		TopMap			: register(t0);
Texture2D		TopNormalsMap	: register(t1);
Texture2D		SideMap			: register(t2);
Texture2D		SideNormalsMap	: register(t3);
SamplerState 	Sampler 		: register(s0);


[domain("tri")]
DOMAIN_OUTPUT DSMain(PATCH_OUTPUT input, float3 uvwCoord : SV_DomainLocation, const OutputPatch<HULL_OUTPUT, 3> patch)
{
    DOMAIN_OUTPUT output;
	    
	output.Color = patch[0].Color;

    // Determine the position of the new vertex.
    float3 vertexPosition = uvwCoord.x * patch[0].Position + uvwCoord.y * patch[1].Position + uvwCoord.z * patch[2].Position;

	output.Normal 	= PhongNormal(uvwCoord, patch[0].Normal, patch[1].Normal, patch[2].Normal);
	output.Tangent 	= PhongNormal(uvwCoord, patch[0].Tangent, patch[1].Tangent, patch[2].Tangent);
	output.Binormal = PhongNormal(uvwCoord, patch[0].Binormal, patch[1].Binormal, patch[2].Binormal);
	
//#ifdef PhongTes
	
	vertexPosition = PhongGeometry(
		uvwCoord.x, uvwCoord.y, uvwCoord.z, 
		patch[0].Position, patch[1].Position, patch[2].Position, 
		patch[0].Normal, patch[1].Normal, patch[2].Normal,
		0.6f);
	
//#endif
	
	output.Tex = uvwCoord.x * patch[0].Tex + uvwCoord.y * patch[1].Tex + uvwCoord.z * patch[2].Tex;
	
	output.ViewDir = normalize(Batch.ViewPos.xyz - vertexPosition);
	
	output.WorldPos = vertexPosition;

	////////// Displacement ///////////////////////////////////////////////////////////////
	float TextureScale 				= Batch.ScaleSharp.x;;
	float TriplanarBlendSharpness 	= Batch.ScaleSharp.y;;
	
	float2 yUV = vertexPosition.xz / TextureScale;
	float2 xUV = vertexPosition.zy / TextureScale;
	float2 zUV = vertexPosition.xy / TextureScale;
	
	float yDisp = TopNormalsMap.SampleLevel(Sampler, yUV  * float2(+1,+1), 0).w	* 2 - 1;
	float xDisp = SideNormalsMap.SampleLevel(Sampler, xUV * float2(-1,-1), 0).w	* 2 - 1;
	float zDisp = SideNormalsMap.SampleLevel(Sampler, zUV * float2(+1,-1), 0).w	* 2 - 1;
	
	float3 blendWeights = pow(abs(output.Normal), TriplanarBlendSharpness);
	float sum = blendWeights.x + blendWeights.y + blendWeights.z;
	blendWeights = blendWeights / float3(sum, sum, sum);
	
	float n = xDisp * blendWeights.x + yDisp * blendWeights.y + zDisp * blendWeights.z;
	
	vertexPosition = vertexPosition + output.Normal * n * 2.0;
	/////////////////////////////////////////////////////////////////////////////////////
	
	
	output.Position = mul(float4(vertexPosition, 1.0f), Batch.ViewProj);
	
    return output;
}


float4 PSMain( DOMAIN_OUTPUT input ) : SV_Target
{
	float TextureScale 				= Batch.ScaleSharp.x;
	float TriplanarBlendSharpness 	= Batch.ScaleSharp.y;
	
	float2 yUV = input.WorldPos.xz / TextureScale;
	float2 xUV = input.WorldPos.zy / TextureScale;
	float2 zUV = input.WorldPos.xy / TextureScale;
	
	float3 yDiff = TopMap.Sample(Sampler, yUV  * float2(+1,+1)).xyz;
	float3 xDiff = SideMap.Sample(Sampler, xUV * float2(-1,-1)).xyz;
	float3 zDiff = SideMap.Sample(Sampler, zUV * float2(+1,-1)).xyz;
	
	float3 yNorm = TopNormalsMap.Sample(Sampler, yUV  * float2(+1,+1)).xyz	* 2 - 1;
	float3 xNorm = SideNormalsMap.Sample(Sampler, xUV * float2(-1,-1)).xyz	* 2 - 1;
	float3 zNorm = SideNormalsMap.Sample(Sampler, zUV * float2(+1,-1)).xyz	* 2 - 1;
	
	
	float3 bump1 = float3( 0,-xNorm.x, -xNorm.y);  
    float3 bump2 = float3( yNorm.y, 0,  yNorm.x);  
    float3 bump3 = float3( zNorm.x, -zNorm.y, 0);  
	
	
	float3 blendWeights = pow(abs(input.Normal), TriplanarBlendSharpness);
	//blendWeights = normalize(max(blendWeights, 0.00001f));
	
	// Divide our blend mask by the sum of it's components, this will make x+y+z=1
	float sum = blendWeights.x + blendWeights.y + blendWeights.z;
	blendWeights = blendWeights / float3(sum, sum, sum);
	
	// Finally, blend together all three samples based on the blend mask.
	float3 c = xDiff * blendWeights.x + yDiff * blendWeights.y + zDiff * blendWeights.z;
	c = 0.5;
	float3 n = bump1 * blendWeights.x + bump2 * blendWeights.y + bump3 * blendWeights.z;
	
	n = normalize(n + input.Normal);
	
	//return float4(c, 1.0f);
	//return float4(n, 1.0f);
	
	float decay = (input.WorldPos.y+10)/30.0f;
	
	float3 amb = float3(50,70,100) / 256.0f / 2;
	float3 sun = float3(140,170,200) / 256.0f * 1 * decay;
	float3 light = (0.2 + 0.8*dot( n, normalize(float3(2,30,1)))) * sun + amb;
	return float4(light * c, 1);
}








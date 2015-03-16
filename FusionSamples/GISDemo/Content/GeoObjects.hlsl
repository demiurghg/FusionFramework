struct DrawParams {
	float4x4	viewProjMatrix;
	float4		Offset;
	float4		Zoom;
};

cbuffer CBSimulationParams : register(b0) { DrawParams drawParams : packoffset(c0); }
 
struct VS_OUTPUT 
{
	float4 Pos		: SV_POSITION;
	float4 color	: COLOR;
};
 
VS_OUTPUT VSMain(float3 Pos: POSITION, float4 color : COLOR)
{
	VS_OUTPUT Out = (VS_OUTPUT)0;
 
	//Out.Pos		= mul(float4(Pos,1), drawParams.viewProjMatrix);

	float2 pos = Pos.xy*drawParams.Zoom.x + drawParams.Offset.xy;
	

	//Out.Pos		= mul(float4(pos, 5, 1), drawParams.viewProjMatrix);
	Out.Pos		= mul(float4(pos.x, 0.1f, pos.y, 1), drawParams.viewProjMatrix);
	
	Out.color	= color;
 
	return Out;
}
 
 
float4 PSMain(VS_OUTPUT input) : SV_Target 
{
	return input.color;
}
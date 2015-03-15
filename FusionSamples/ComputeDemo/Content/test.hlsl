
#if 0
$ubershader
#endif

#define BLOCK_SIZE	256

struct RESULT {
	float	Sum;
	float	Mul;
	int		GroupID;
	int		GroupThreadID;
	int		DispatchThreadID;
	int		GroupIndex;
};

struct PARAMS {
	int Size;
};

cbuffer CBParams : register(b0) { PARAMS Params : packoffset( c0 ); }	

RWStructuredBuffer<float>	argA	: register(u0);
RWStructuredBuffer<float>	argB	: register(u1);
RWStructuredBuffer<RESULT>	result	: register(u2);


[numthreads( BLOCK_SIZE, 1, 1 )]
void CSMain( 
	uint3 groupID			: SV_GroupID,
	uint3 groupThreadID 	: SV_GroupThreadID, 
	uint3 dispatchThreadID 	: SV_DispatchThreadID,
	uint  groupIndex 		: SV_GroupIndex
)
{
	int id = dispatchThreadID.x;
	if (id < Params.Size) {
		result[id].Sum	=	argA[id] + argB[id];
		result[id].Mul	=	argA[id] * argB[id];
		result[id].GroupID			=	groupID.x;
		result[id].GroupThreadID	=	groupThreadID.x;
		result[id].DispatchThreadID	=	dispatchThreadID.x;
		result[id].GroupIndex		=	groupIndex.x;
	}
}


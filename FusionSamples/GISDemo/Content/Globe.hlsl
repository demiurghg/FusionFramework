/*-----------------------------------------------------------------------------
	CONSTANT Tables
-----------------------------------------------------------------------------*/
struct ConstData {
	float4x4	ViewProj;
	uint2		CameraX	;
	uint2		CameraY	;
	uint4		CameraZ	;
	float4		FRTT	; // Factor, Radius
	float4 ViewDir;
};


struct DataForDots {
	float4x4	View;
	float4x4	Proj;
	float4		TexWHST;
	float4		Colors[16];
};

struct VS_INPUT {	
	float3	Position		: POSITION	;	// World position
	float4	Color			: COLOR		;
	float4	Tex				: TEXCOORD0	;	// Texture Coordinates

	uint2 lon				: TEXCOORD1	;
	uint2 lat				: TEXCOORD2	;
};

struct VS_CART_INPUT {	
	float4	Color	: COLOR		;
	float4	Tex		: TEXCOORD0	;	// Texture Coordinates

	uint2 X			: TEXCOORD1	;
	uint2 Y			: TEXCOORD2	;
	uint2 Z			: TEXCOORD3	;
};

struct VS_OUTPUT {
    float4 Position		: SV_POSITION	;
	float4 Color		: COLOR			;
	float4 Tex			: TEXCOORD0		;
	float3 Normal		: TEXCOORD1		;
	float3 XAxis		: TEXCOORD2		;
};

struct GS_OUTPUT {
    float4 Position		: SV_POSITION	;
	float4 Color		: COLOR			;
	float2 Tex			: TEXCOORD0		;
	float3 Normal		: TEXCOORD1		;
};


struct GS_ARC_OUTPUT {
    float4 Position		: SV_POSITION	;
	float4 Color		: COLOR			;
};


cbuffer CBStage		: register(b0) 	{	ConstData	Stage		: 	packoffset( c0 );	}
cbuffer CBDotsStage : register(b1) 	{	DataForDots	DotsData	;	}

Texture2D		DiffuseMap		: register(t0);
Texture2D		FrameMap		: register(t1);
Texture2D		AtmosNextMap	: register(t2);
Texture2D		Arrow			: register(t3);
SamplerState	Sampler			: register(s0);
SamplerState	PointSampler	: register(s1);


/*-------------------------------------------------------------------------------------------------
	Globe rendering :
-------------------------------------------------------------------------------------------------*/

double dDiv(double a, double b) // 
{
	double r = double(1.0f/float(b));

	r = r * (2.0 - b*r);
	r = r * (2.0 - b*r);

	return a*r;
}


double sine_limited(double x) {
  double r = x, mxx = -x*x;
  // Change dDiv to multiply to constants 
  r += (x *= dDiv(mxx, 6.0	)); // i=3
  r += (x *= dDiv(mxx, 20.0	)); // i=5
  r += (x *= dDiv(mxx, 42.0	)); // i=7
  r += (x *= dDiv(mxx, 72.0	)); // i=9
  r += (x *= dDiv(mxx, 110.0)); // i=11

  return r;
}

// works properly only for x >= 0
double sine_positive(double x) {
	double PI		=	3.141592653589793;
	double PI2		=	2.0*PI;
	double PI_HALF	=	0.5*PI;
	
	
	if (x <= PI_HALF) {
	  return sine_limited(x);
	} else if (x <= PI) {
	  return sine_limited(PI - x);
	} else if (x <= PI2) {
	  return -sine_limited(x - PI);
	} else {
	  return sine_limited(x - PI2*floor(float(dDiv(x,PI2))));
	}
}

double sine(double x) {
	return x < 0.0 ? -sine_positive(-x) : sine_positive(x);
}

double cosine(double x) {
	double PI=3.141592653589793;
	double PI_HALF=0.5*PI;
	return sine(PI_HALF - x);
}


double3 SphericalToDecart(double2 pos, double r)
{
	double3 res = double3(0,0,0);

	double sinX = sine(pos.x);
	double cosX = cosine(pos.x);
	double sinY = sine(pos.y);
	double cosY = cosine(pos.y);

	res.z = r*cosY*cosX;
	res.x = r*cosY*sinX;
	res.y = r*sinY;

	//res.z = r*cosine(pos.y)*cosine(pos.x);
	//res.x = r*cosine(pos.y)*sine(pos.x);
	//res.y = r*sine(pos.y);

	return res;
}

#if 0
$ubershader VERTEX_SHADER USE_GEOCOORDS DRAW_POLY DRAW_VERTEX_POLY DRAW_TEXTURED +SHOW_FRAMES
$ubershader VERTEX_SHADER USE_GEOCOORDS DRAW_POLY DRAW_VERTEX_POLY DRAW_COLOR
$ubershader VERTEX_SHADER USE_GEOCOORDS DRAW_POLY DRAW_VERTEX_POLY DRAW_PALETTE
$ubershader DRAW_VERTEX_LINES USE_GEOCOORDS VERTEX_SHADER DRAW_SEGMENTED_LINES

$ubershader DRAW_POLY DRAW_VERTEX_POLY USE_CARTCOORDS VERTEX_SHADER DRAW_COLOR
$ubershader DRAW_VERTEX_DOTS DRAW_DOTS USE_GEOCOORDS VERTEX_SHADER DOTS_SCREENSPACE
$ubershader DRAW_VERTEX_LINES VERTEX_SHADER USE_GEOCOORDS DRAW_CHARTS
$ubershader DRAW_VERTEX_LINES USE_GEOCOORDS VERTEX_SHADER DRAW_DOTS DOTS_WORLDSPACE
$ubershader DRAW_POLY DRAW_VERTEX_POLY USE_GEOCOORDS VERTEX_SHADER DRAW_HEAT
$ubershader DRAW_VERTEX_POLY USE_GEOCOORDS VERTEX_SHADER DRAW_ATMOSPHERE
$ubershader DRAW_VERTEX_LINES USE_GEOCOORDS VERTEX_SHADER DRAW_ARCS

$ubershader DRAW_VERTEX_LINES USE_GEOCOORDS VERTEX_SHADER DRAW_LINES

#endif

#ifdef VERTEX_SHADER
VS_OUTPUT VSMain ( 
	#ifdef USE_GEOCOORDS
		VS_INPUT v
	#endif
	
	#ifdef USE_CARTCOORDS
		VS_CART_INPUT v
	#endif
)
{
	VS_OUTPUT	output = (VS_OUTPUT)0;
	
	double3 cameraPos =  double3(asdouble(Stage.CameraX[0], Stage.CameraX[1]), asdouble(Stage.CameraY[0], Stage.CameraY[1]), asdouble(Stage.CameraZ[0], Stage.CameraZ[1]));

	float angle = 0;

#ifdef USE_GEOCOORDS
	double lon		= asdouble(v.lon.x, v.lon.y);
	double lat		= asdouble(v.lat.x, v.lat.y);
	double3 cPos	= SphericalToDecart(double2(lon, lat), 6378.137);

	angle = float(lon);
#endif

#ifdef USE_CARTCOORDS
	double3 cPos = double3(asdouble(v.X.x, v.X.y), asdouble(v.Y.x, v.Y.y), asdouble(v.Z.x, v.Z.y));
#endif
	
	double3 normPos = cPos*0.000156785594;
	float3	normal	= normalize(float3(normPos));
	//
	//cPos = cPos + double3(normal) * double(v.Position.x);

	double posX = cPos.x - cameraPos.x;
	double posY = cPos.y - cameraPos.y;
	double posZ = cPos.z - cameraPos.z;

#ifdef DRAW_VERTEX_DOTS
	output.Position	= mul(float4(posX, posY, posZ, 1), DotsData.View);
	output.Normal	= normal;
#endif
#ifdef DRAW_VERTEX_LINES
	//double3 normPos = cPos*0.000156785594;
	//float3	normal	= normalize(float3(normPos));

	output.Position	= float4(posX, posY, posZ, 1);
	output.Normal	= normal.xyz;

	float4x4 rotMat = float4x4( cos(angle), 0, -sin(angle), 0, 0,1,0,0, sin(angle),0,cos(angle),0, 0,0,0,1 );
	
	output.XAxis = normalize(mul(float4(1, 0, 0, 0), rotMat).xyz);

#endif
#ifdef DRAW_VERTEX_POLY
	output.Position	=	mul(float4(posX, posY, posZ, 1), Stage.ViewProj);
	output.Normal	=	normal;
#endif
	output.Color	=	v.Color;
	output.Tex		=	v.Tex;
	

	return output;
}
#endif



#ifdef DRAW_CHARTS

[maxvertexcount(20)]
void GSMain ( point VS_OUTPUT inputArray[1], inout TriangleStream<GS_OUTPUT> stream )
{
	GS_OUTPUT	output;// = (GS_OUTPUT)0;
	VS_OUTPUT	input	=	inputArray[0];

	float w = input.Tex.x;
	float h = input.Tex.y;
	float l = input.Tex.z;

	float offset = input.Tex.w;

	float3 pos = input.Position.xyz;

	float3	color		= input.Color.xyz;

	float3 xAxis = input.XAxis;
	float3 zAxis = normalize(cross(xAxis,input.Normal));

	float alpha = 1.0f;
	if(h == 0.0f) {
		alpha = 0.0f;
		offset = -1000.0f;
	}


	// Top
	output.Color	=	float4(color*0.8f, alpha);
	output.Tex		=	float2(0.0f, 0.0f);
	output.Normal	=	input.Normal;
	output.Position	=	mul(float4(pos + w*xAxis + h*input.Normal + l*zAxis + offset*input.Normal, 1), Stage.ViewProj);
	stream.Append( output );
	
	output.Position	=	mul(float4(pos - w*xAxis + h*input.Normal + l*zAxis + offset*input.Normal, 1), Stage.ViewProj);
	stream.Append( output );
	
	output.Position	=	mul(float4(pos + w*xAxis + h*input.Normal - l*zAxis + offset*input.Normal, 1), Stage.ViewProj);
	stream.Append( output );
	
	output.Position	=	mul(float4(pos - w*xAxis + h*input.Normal - l*zAxis + offset*input.Normal, 1), Stage.ViewProj);
	stream.Append( output );
    
	stream.RestartStrip();
	
	
	//// Bottom
	//output.Position	=	mul(float4(x - w, y - h, z - l, 1), Stage.ViewProj);
	//output.Normal	= float3(0, -1, 0);
	//output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	//stream.Append( output );
	//
	//output.Position	=	mul(float4(x - w, y - h, z + l, 1), Stage.ViewProj);
	//output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	//stream.Append( output );
	//
	//output.Position	=	mul(float4(x + w, y - h, z - l, 1), Stage.ViewProj);
	//output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	//stream.Append( output );
	//
	//output.Position	=	mul(float4(x + w, y - h, z + l, 1), Stage.ViewProj);
	//output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	//stream.Append( output );
	//
	//stream.RestartStrip();
	//
	//
	
	//// Right
	output.Color	=	float4(color*0.5f, alpha);
	output.Position	=	mul(float4(pos + w*xAxis - l*zAxis + offset*input.Normal, 1), Stage.ViewProj);
	stream.Append( output );
	
	output.Position	=	mul(float4(pos + w*xAxis + l*zAxis + offset*input.Normal, 1), Stage.ViewProj);
	stream.Append( output );
	
	output.Position	=	mul(float4(pos + w*xAxis + h*input.Normal - l*zAxis + offset*input.Normal, 1), Stage.ViewProj);
	stream.Append( output );
	
	output.Position	=	mul(float4(pos + w*xAxis + h*input.Normal + l*zAxis + offset*input.Normal, 1), Stage.ViewProj);
	stream.Append( output );
    
	stream.RestartStrip();
	

	// Left
	output.Color	=	float4(color*0.7f, alpha);
	output.Position	=	mul(float4(pos - w*xAxis + h*input.Normal - l*zAxis + offset*input.Normal, 1), Stage.ViewProj);
	stream.Append( output );
	
	output.Position	=	mul(float4(pos - w*xAxis + h*input.Normal + l*zAxis + offset*input.Normal, 1), Stage.ViewProj);
	stream.Append( output );
	
	output.Position	=	mul(float4(pos - w*xAxis - l*zAxis + offset*input.Normal, 1), Stage.ViewProj);
	stream.Append( output );
	
	output.Position	=	mul(float4(pos - w*xAxis + l*zAxis + offset*input.Normal, 1), Stage.ViewProj);
	stream.Append( output );
    
	stream.RestartStrip();
	
	
	//// Front
	output.Color	=	float4(color*0.4f, alpha);
	output.Position	=	mul(float4(pos + w*xAxis + h*input.Normal + l*zAxis + offset*input.Normal, 1), Stage.ViewProj);
	stream.Append( output );
	
	output.Position	=	mul(float4(pos + w*xAxis + l*zAxis + offset*input.Normal, 1), Stage.ViewProj);
	stream.Append( output );
	
	output.Position	=	mul(float4(pos - w*xAxis + h*input.Normal + l*zAxis + offset*input.Normal, 1), Stage.ViewProj);
	stream.Append( output );
	
	output.Position	=	mul(float4(pos - w*xAxis + l*zAxis + offset*input.Normal, 1), Stage.ViewProj);
	stream.Append( output );
    
	stream.RestartStrip();
	
	
	//// Back
	output.Color	=	float4(color*0.9f, alpha);
	output.Position	=	mul(float4(pos - w*xAxis + h*input.Normal - l*zAxis + offset*input.Normal, 1), Stage.ViewProj);
	stream.Append( output );
	
	output.Position	=	mul(float4(pos - w*xAxis - l*zAxis + offset*input.Normal, 1), Stage.ViewProj);
	stream.Append( output );
	
	output.Position	=	mul(float4(pos + w*xAxis + h*input.Normal - l*zAxis + offset*input.Normal, 1), Stage.ViewProj);
	stream.Append( output );
	
	output.Position	=	mul(float4(pos + w*xAxis - l*zAxis + offset*input.Normal, 1), Stage.ViewProj);
	stream.Append( output );
    
	stream.RestartStrip();
}



float4 PSMain ( GS_OUTPUT input ) : SV_Target
{
	return input.Color;
}
#endif



#ifdef DRAW_SIMPLE_BUILDINGS

[maxvertexcount(20)]
void GSMain ( point VS_OUTPUT inputArray[1], inout TriangleStream<GS_OUTPUT> stream )
{
	GS_OUTPUT	output;// = (GS_OUTPUT)0;
	VS_OUTPUT	input	=	inputArray[0];

	float w = input.Tex.x;
	float h = w*2.0f;
	float l = input.Tex.y;

	float x = input.Position.x;
	float y = input.Position.y;
	float z = input.Position.z;

	float4	color		= input.Color;


	// Top
	output.Color	=	float4(0.8f, 0.8f, 0.8f, 1.0f);
	output.Tex		=	float2(0.0f, 0.0f);
	output.Normal	=	input.Normal;
	output.Position	=	mul(float4(x + w, y + l, z + h, 1), DotsData.Proj);
	stream.Append( output );
	
	output.Position	=	mul(float4(x - w, y + l, z + h, 1), DotsData.Proj);
	stream.Append( output );
	
	output.Position	=	mul(float4(x + w, y - l, z + h, 1), DotsData.Proj);
	stream.Append( output );
	
	output.Position	=	mul(float4(x - w, y - l, z + h, 1), DotsData.Proj);
	stream.Append( output );
    
	stream.RestartStrip();
	
	
	//// Bottom
	//output.Position	=	mul(float4(x - w, y - h, z - l, 1), Stage.ViewProj);
	//output.Normal	= float3(0, -1, 0);
	//output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	//stream.Append( output );
	//
	//output.Position	=	mul(float4(x - w, y - h, z + l, 1), Stage.ViewProj);
	//output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	//stream.Append( output );
	//
	//output.Position	=	mul(float4(x + w, y - h, z - l, 1), Stage.ViewProj);
	//output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	//stream.Append( output );
	//
	//output.Position	=	mul(float4(x + w, y - h, z + l, 1), Stage.ViewProj);
	//output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	//stream.Append( output );
	//
	//stream.RestartStrip();
	//
	//
	//// Right
	output.Color	=	float4(0.5f, 0.5f, 0.5f, 1.0f);
	output.Position	=	mul(float4(x + w, y - l, z - h, 1), DotsData.Proj);
	stream.Append( output );
	
	output.Position	=	mul(float4(x + w, y + l, z - h, 1), DotsData.Proj);
	stream.Append( output );
	
	output.Position	=	mul(float4(x + w, y - l, z + h, 1), DotsData.Proj);
	stream.Append( output );
	
	output.Position	=	mul(float4(x + w, y + l, z + h, 1), DotsData.Proj);
	stream.Append( output );
    
	stream.RestartStrip();
	

	//// Left
	output.Color	=	float4(0.7f, 0.7f, 0.7f, 1.0f);
	output.Position	=	mul(float4(x - w, y - l, z + h, 1), DotsData.Proj);
	stream.Append( output );
	
	output.Position	=	mul(float4(x - w, y + l, z + h, 1), DotsData.Proj);
	stream.Append( output );
	
	output.Position	=	mul(float4(x - w, y - l, z - h, 1), DotsData.Proj);
	stream.Append( output );
	
	output.Position	=	mul(float4(x - w, y + l, z - h, 1), DotsData.Proj);
	stream.Append( output );
    
	stream.RestartStrip();
	

	//// Front
	output.Color	=	float4(0.4f, 0.4f, 0.4f, 1.0f);
	output.Position	=	mul(float4(x + w, y + l, z + h, 1), DotsData.Proj);
	stream.Append( output );
	
	output.Position	=	mul(float4(x + w, y + l, z - h, 1), DotsData.Proj);
	stream.Append( output );
	
	output.Position	=	mul(float4(x - w, y + l, z + h, 1), DotsData.Proj);
	stream.Append( output );
	
	output.Position	=	mul(float4(x - w, y + l, z - h, 1), DotsData.Proj);
	stream.Append( output );
    
	stream.RestartStrip();
	

	//// Back
	output.Color	=	float4(0.9f, 0.9f, 0.9f, 1.0f);
	output.Position	=	mul(float4(x - w, y - l, z + h, 1), DotsData.Proj);
	stream.Append( output );
	
	output.Position	=	mul(float4(x - w, y - l, z - h, 1), DotsData.Proj);
	stream.Append( output );
	
	output.Position	=	mul(float4(x + w, y - l, z + h, 1), DotsData.Proj);
	stream.Append( output );
	
	output.Position	=	mul(float4(x + w, y - l, z - h, 1), DotsData.Proj);
	stream.Append( output );
    
	stream.RestartStrip();
	//output.Position	=	mul(float4(x - w, y + h, z - l, 1), Stage.ViewProj);
	//output.Normal	= float3(0, 0, -1);
	//output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	//stream.Append( output );
	//
	//output.Position	=	mul(float4(x - w, y - h, z - l, 1), Stage.ViewProj);
	//output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	//stream.Append( output );
	//
	//output.Position	=	mul(float4(x + w, y + h, z - l, 1), Stage.ViewProj);
	//output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	//stream.Append( output );
	//
	//output.Position	=	mul(float4(x + w, y - h, z - l, 1), Stage.ViewProj);
	//output.ViewVec	= normalize((Stage.ViewPosition - output.Position).xyz);
	//stream.Append( output );
	//	
	//stream.RestartStrip();

}



float4 PSMain ( GS_OUTPUT input ) : SV_Target
{
	return input.Color;
}
#endif


#ifdef DRAW_ARCS

float3 Slerp(float3 n0, float3 n1, float t)
{
	float3 N0 = normalize(n0);
	float3 N1 = normalize(n1);

	float phi = acos(dot(N0, N1));

	float sinPhi = sin(phi);

	float3 ret = (sin((1.0f-t)*phi)/sinPhi)*N0 + (sin(t*phi)/sinPhi)*N1;
	
	return phi;
}


[maxvertexcount(100)]
void GSMain ( line VS_OUTPUT inputArray[2], inout TriangleStream<GS_ARC_OUTPUT> stream )
{
	GS_ARC_OUTPUT	output = (GS_ARC_OUTPUT)0;
	VS_OUTPUT	p0	=	inputArray[0];
	VS_OUTPUT	p1	=	inputArray[1];

	float halfWidth0 = p0.Tex.x;
	float halfWidth1 = p1.Tex.x;

	float3 dir = p0.Position.xyz - p1.Position.xyz;
	
	float3 sideVec0 = normalize(cross(p0.Normal, dir));
	float3 sideVec1 = normalize(cross(p1.Normal, dir));

	float3 sideOffset0 = sideVec0*halfWidth0;
	float3 sideOffset1 = sideVec1*halfWidth1;


	float slicesCount	= 50;
	float radius		= length(dir)/2.0f;

	float PI = 3.141592f;

	[unroll]
	for(float i = 0; i < slicesCount; i = i + 1) {

		float f = i / (slicesCount-1);

		float3 pos			= lerp(p0.Position.xyz, p1.Position.xyz, f);
		float3 sideOffset	= lerp(sideOffset0, sideOffset1, f);
		float3 normal		= normalize(lerp(p0.Normal, p1.Normal, f));

		float height = sin(PI * f) * radius;

		// Determine color
		float4 color = lerp(p0.Color, p1.Color, f);
		output.Color = color;


		output.Position	= mul(float4(pos.xyz + sideOffset + normal*height, 1), Stage.ViewProj);	
		stream.Append( output );

		output.Position	= mul(float4(pos.xyz - sideOffset + normal*height, 1), Stage.ViewProj);	
		stream.Append( output );

	}

}



float4 PSMain ( GS_ARC_OUTPUT input ) : SV_Target
{
	return input.Color;
}
#endif


#ifdef DRAW_SEGMENTED_LINES

[maxvertexcount(10)]
void GSMain ( line VS_OUTPUT inputArray[2], inout TriangleStream<GS_OUTPUT> stream )
{
	GS_OUTPUT	output;// = (GS_OUTPUT)0;
	VS_OUTPUT	p0	=	inputArray[0];
	VS_OUTPUT	p1	=	inputArray[1];

	float halfWidth0 = p0.Tex.x;
	float halfWidth1 = p1.Tex.x;

	float3 dir = p0.Position.xyz - p1.Position.xyz;
	
	float3 sideVec0 = normalize(cross(p0.Normal, dir));
	float3 sideVec1 = normalize(cross(p1.Normal, dir));

	float3 sideOffset0 = sideVec0*halfWidth0;
	float3 sideOffset1 = sideVec1*halfWidth1;


	float slicesCount = 5;

	//float texMaxX = p0.Tex.y;
	float texMaxX = length(dir) * 3.0f;
	texMaxX = texMaxX - frac(texMaxX);

	[unroll]
	for(float i = 0; i < slicesCount; i = i + 1) {

		float f = i / (slicesCount-1);

		float3 pos			= lerp(p0.Position.xyz, p1.Position.xyz, f);
		float3 sideOffset	= lerp(sideOffset0, sideOffset1, f);
		float3 normal		= normalize(lerp(p0.Normal, p1.Normal, f));

		float texX = texMaxX*f;

		// Determine color
		//float4 color = lerp(p0.Color, p1.Color, f);
		if(i == slicesCount-1) {
			output.Color = p1.Color;
		} else {
			output.Color = p0.Color;
		}

		output.Normal = normal;
		
		output.Tex		= float2(texX, 0.0f);
		output.Position	= mul(float4(pos.xyz + sideOffset, 1), Stage.ViewProj);	
		stream.Append( output );

		output.Tex		= float2(texX, 1.0f);
		output.Position	= mul(float4(pos.xyz - sideOffset, 1), Stage.ViewProj);	
		stream.Append( output );

	}
}



float4 PSMain ( GS_OUTPUT input ) : SV_Target
{
	float4 color = DiffuseMap.Sample(Sampler, input.Tex);

	return color * input.Color.a;
}
#endif


#ifdef DRAW_LINES

[maxvertexcount(4)]
void GSMain ( line VS_OUTPUT inputArray[2], inout TriangleStream<GS_OUTPUT> stream )
{
	GS_OUTPUT	output;// = (GS_OUTPUT)0;
	VS_OUTPUT	p0	=	inputArray[0];
	VS_OUTPUT	p1	=	inputArray[1];

	float halfWidth0 = p0.Tex.x;
	float halfWidth1 = p1.Tex.x;

	float3 dir = p0.Position.xyz - p1.Position.xyz;
	
	float3 sideVec0 = normalize(cross(p0.Normal, dir));
	float3 sideVec1 = normalize(cross(p1.Normal, dir));

	float3 sideOffset0 = sideVec0*halfWidth0;
	float3 sideOffset1 = sideVec1*halfWidth1;


	// Plane
	output.Position	= mul(float4(p0.Position.xyz + sideOffset0, 1), Stage.ViewProj);
	output.Tex		= float2(0.0f, 0.0f);
	output.Color	= p0.Color;
	output.Normal	= p0.Normal;
	stream.Append( output );
	
	output.Position	= mul(float4(p0.Position.xyz - sideOffset0, 1), Stage.ViewProj);
	output.Color	= p0.Color;
	stream.Append( output );
	
	output.Position	= mul(float4(p1.Position.xyz + sideOffset1, 1), Stage.ViewProj);
	output.Color	= p1.Color;
	stream.Append( output );
	
	output.Position	= mul(float4(p1.Position.xyz - sideOffset1, 1), Stage.ViewProj);
	output.Color	= p1.Color;
	stream.Append( output );
}



float4 PSMain ( GS_OUTPUT input ) : SV_Target
{
	return input.Color;
}
#endif


#ifdef DRAW_DOTS

[maxvertexcount(4)]
void GSMain ( point VS_OUTPUT inputArray[1], inout TriangleStream<GS_OUTPUT> stream )
{
	GS_OUTPUT	output;// = (GS_OUTPUT)0;
	VS_OUTPUT	input	=	inputArray[0];

	float halfWidth = input.Tex.z;

	float x = input.Position.x;
	float y = input.Position.y;
	float z = input.Position.z;

	float3 pos = input.Position.xyz;
	float3 xAxis = input.XAxis;
	float3 zAxis = normalize(cross(xAxis,input.Normal)); 


	float texRight	= (input.Tex.x * DotsData.TexWHST.y)/DotsData.TexWHST.x;
	float texLeft	= ((input.Tex.x-1) * DotsData.TexWHST.y)/DotsData.TexWHST.x;

	float4	color		= input.Color;
	int		colorInd	= int(input.Tex.y);
	if(colorInd != 0) color = DotsData.Colors[colorInd];

	//float ang = dot(input.Normal, Stage.ViewDir.xyz);
	//color.a = ang;


	float4x4 mat;
#ifdef DOTS_SCREENSPACE
	// Plane
	output.Position	=	mul(float4(x + halfWidth, y + halfWidth, z, 1), DotsData.Proj);
	output.Tex		=	float2(texRight, 0.0f);
	output.Color	=	color;
	output.Normal	=	input.Normal;
	stream.Append( output );
	
	output.Position	= mul(float4(x - halfWidth, y + halfWidth, z, 1), DotsData.Proj);
	output.Tex		= float2(texLeft, 0.0f);
	stream.Append( output );
	
	output.Position	= mul(float4(x + halfWidth, y - halfWidth, z, 1), DotsData.Proj);
	output.Tex		= float2(texRight, 1.0f);
	stream.Append( output );
	
	output.Position	= mul(float4(x - halfWidth, y - halfWidth, z, 1), DotsData.Proj);
	output.Tex		= float2(texLeft, 1.0f);
	stream.Append( output );
#endif
#ifdef DOTS_WORLDSPACE
	// Plane
	output.Color	=	color;
	output.Normal	=	input.Normal;

	output.Position	= mul(float4(pos + halfWidth*xAxis - halfWidth*zAxis, 1), Stage.ViewProj);
	output.Tex		= float2(texRight, 0.0f);
	stream.Append( output );
	
	output.Position	= mul(float4(pos - halfWidth*xAxis - halfWidth*zAxis, 1), Stage.ViewProj);
	output.Tex		= float2(texLeft, 0.0f);
	stream.Append( output );
	
	output.Position	= mul(float4(pos + halfWidth*xAxis + halfWidth*zAxis, 1), Stage.ViewProj);
	output.Tex		= float2(texRight, 1.0f);
	stream.Append( output );
	
	output.Position	= mul(float4(pos - halfWidth*xAxis + halfWidth*zAxis, 1), Stage.ViewProj);
	output.Tex		= float2(texLeft, 1.0f);
	stream.Append( output );
#endif

	
}



float4 PSMain ( GS_OUTPUT input ) : SV_Target
{
	float4 color = DiffuseMap.Sample(Sampler, input.Tex);

	color.rgb *= input.Color.rgb;

	color.a *= input.Color.a;

	return color;
}
#endif


////////////////////////// Draw map tiles and polygons
#ifdef DRAW_POLY
float4 PSMain ( VS_OUTPUT input ) : SV_Target
{
	#ifdef DRAW_COLOR
		return float4(input.Color.rgb, Stage.FRTT.x);
	#endif
	#ifdef DRAW_PALETTE
		float4 color = DiffuseMap.Sample(Sampler, float2(Stage.FRTT.x, 0.0f)); // Sample palette
		return float4(color.rgb, Stage.FRTT.y);
	#endif
	#ifdef DRAW_HEAT
		float step = 0.5f/Stage.FRTT.z;

		float val = 0.0f;

		float valX = DiffuseMap.Sample(PointSampler, float2(input.Tex.x, 1.0f - input.Tex.y)).x; // Center
		val	+= valX;
		// Corners
		float val0 = DiffuseMap.Sample(PointSampler, float2(input.Tex.x - step, 1.0f - input.Tex.y - step)).x; // left top;
		float val1 = DiffuseMap.Sample(PointSampler, float2(input.Tex.x + step, 1.0f - input.Tex.y - step)).x; // left top;
		float val2 = DiffuseMap.Sample(PointSampler, float2(input.Tex.x - step, 1.0f - input.Tex.y + step)).x; // left top;
		float val3 = DiffuseMap.Sample(PointSampler, float2(input.Tex.x + step, 1.0f - input.Tex.y + step)).x; // left top;
		val += val0; // left top
		val += val1; // right top
		val += val2; // left bottom
		val += val3; // right bottom

		//val += DiffuseMap.Sample(PointSampler, float2(input.Tex.x - step, 1.0f - input.Tex.y)).x; // left
		//val += DiffuseMap.Sample(PointSampler, float2(input.Tex.x + step, 1.0f - input.Tex.y)).x; // right
		//val += DiffuseMap.Sample(PointSampler, float2(input.Tex.x, 1.0f - input.Tex.y - step)).x; // top
		//val += DiffuseMap.Sample(PointSampler, float2(input.Tex.x, 1.0f - input.Tex.y + step)).x; // bottom


		val = val / 5.0f;

		val = val / Stage.FRTT.x;

		val = clamp(val, 0.0f, 1.0f);

		float4	color	= FrameMap.Sample(Sampler, float2(val, 0.0f));

			// float iso = 10;
			// if ( (val0<0.5 && val1>=0.5) || (val0>0.5 && val1<=0.5) ) color = float4(1,1,1,1);
			// if ( (val2<0.5 && val3>=0.5) || (val2>0.5 && val1<=0.5) ) color = float4(1,1,1,1);
			//if (
		/*	float dx = ddx(val);
			float dy = ddy(val);
			float z1 = val + dx;
			float z2 = val + dy;
			
			float z0 = frac(val/20);
			z1 = frac(z1/20);
			z2 = frac(z2/20);
			if((z0<0.5 && z1>0.5) || (z0>0.5 && z1<0.5)) color = float4(1,1,1,1);
			if((z0<0.5 && z2>0.5) || (z0>0.5 && z2<0.5)) color = float4(1,1,1,1);*/
		
		return float4(color.rgba);
	#endif
	#ifdef DRAW_TEXTURED
		float4 color	= DiffuseMap.Sample(Sampler, input.Tex.xy);
		float3 ret		= color.rgb;
		
		#ifdef SHOW_FRAMES
			float4	frame	= FrameMap.Sample(Sampler, input.Tex.xy);
					ret		= color.rgb * (1.0 - frame.a) + frame.rgb*frame.a;
		#endif
		
		return float4(ret, color.a);
	#endif
}
#endif


#ifdef DRAW_ATMOSPHERE

float4 PSMain ( VS_OUTPUT input ) : SV_Target
{
	float3 values		= DiffuseMap.Sample(	PointSampler, input.Tex	).xyz;
	float3 nextVals		= AtmosNextMap.Sample(	PointSampler, input.Tex	).xyz;

	float value = lerp(values.x, nextVals.x, Stage.FRTT.x);
	value = (value - 6.0f)/(36.0f - 6.0f);

	float4 color = FrameMap.Sample(Sampler, float2(value, 0.0f));

	///////////////////////////////////////////////// Arrows ////////////////////////////////////////
	float2 tex = input.Tex;// + float2(0.5f/25.0f, -0.5f/25.0f);
	tex *= Stage.FRTT.y; // arrowsScale
		
	float2	temp	= frac(tex);

	float2	velPos	= (floor(tex) + float2(0.5, 0.5)) / Stage.FRTT.y;

	float2 currentSpeed = DiffuseMap.Sample(PointSampler, velPos).yz;
	
	float		angle		= currentSpeed.y;
	float2x2	rotationMat	= {cos(angle), -sin(angle), sin(angle), cos(angle)};

	float	normLen		= currentSpeed.x/7.0f;
	float	velScale	= lerp( 1.0, 2.5, 1.0f - normLen);
			velScale	= clamp( velScale, 1.0, 2.5f);

	float2	texCoords = mul(temp - float2(0.5, 0.5), rotationMat)*velScale + float2(0.5, 0.5);
	
	color.rgb *= Arrow.Sample(Sampler, texCoords).rgb;
	////////////////////////////////////////////////////////////////////////////////////////////////

	return color;
}

#endif

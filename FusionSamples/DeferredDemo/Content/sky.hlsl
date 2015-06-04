

#if 0
$ubershader  PROCEDURAL_SKY|FOG|CLOUDS SRGB|CIERGB
#endif

cbuffer Constants : register(b0)
{
  float4x4 MatrixWVP;
  float3	 SunPosition;
  float4	 SunColor;
  float		 Turbidity;
  float3	 Temperature;
  float		 SkyIntensity;
  float3	Ambient;
  float		Time;
};

Texture2D CloudTexture : register(t0);
Texture2D CirrusTexture : register(t1);
SamplerState SamplerLinear : register(s0);

struct VS_INPUT {
	float3 position		: POSITION0;
};
	

struct VS_OUTPUT {
	float4 position		: SV_POSITION;
	float3 worldPos		: TEXCOORD1;
	float3 skyColor		: COLOR0;
};

#define PS_INPUT VS_OUTPUT

/*-------------------------------------------------------------------------------------------------
	Perez sky model
-------------------------------------------------------------------------------------------------*/

float3 perezZenith ( float t, float thetaSun )
{
	const float		pi = 3.1415926;
	const float4	cx1 = float4 ( 0,       0.00209, -0.00375, 0.00165  );
	const float4	cx2 = float4 ( 0.00394, -0.03202, 0.06377, -0.02903 );
	const float4	cx3 = float4 ( 0.25886, 0.06052, -0.21196, 0.11693  );
	const float4	cy1 = float4 ( 0.0,     0.00317, -0.00610, 0.00275  );
	const float4	cy2 = float4 ( 0.00516, -0.04153, 0.08970, -0.04214 );
	const float4	cy3 = float4 ( 0.26688, 0.06670, -0.26756, 0.15346  );

	float	t2    = t*t;
	float	chi   = (4.0 / 9.0 - t / 120.0 ) * (pi - 2.0 * thetaSun );
	float4	theta = float4 ( 1, thetaSun, thetaSun*thetaSun, thetaSun*thetaSun*thetaSun );

	float	Y = (4.0453 * t - 4.9710) * tan ( chi ) - 0.2155 * t + 2.4192;
	float	x = t2 * dot ( cx1, theta ) + t * dot ( cx2, theta ) + dot ( cx3, theta );
	float	y = t2 * dot ( cy1, theta ) + t * dot ( cy2, theta ) + dot ( cy3, theta );

	return float3 ( Y, x, y );//*/
}

float ACOS(float x) {
	return 3.1415/2 - x - x*x*x/6.0 - 3.0/40*(x*x*x*x*x) - 5.0/122 * (x*x*x*x * x*x*x) - 35.0/1152*(x*x*x * x*x*x * x*x*x);
}

float3  perezFunc ( float t, float cosTheta, float cosGamma )
{
	//return 1;
	float  gamma      = ACOS ( cosGamma );
    float  cosGammaSq = cosGamma * cosGamma;
    float  aY =  0.17872 * t - 1.46303;	      float  bY = -0.35540 * t + 0.42749;
    float  cY = -0.02266 * t + 5.32505;	      float  dY =  0.12064 * t - 2.57705;
    float  eY = -0.06696 * t + 0.37027;	      float  ax = -0.01925 * t - 0.25922;
    float  bx = -0.06651 * t + 0.00081;	      float  cx = -0.00041 * t + 0.21247;
    float  dx = -0.06409 * t - 0.89887;	      float  ex = -0.00325 * t + 0.04517;
    float  ay = -0.01669 * t - 0.26078;	      float  by = -0.09495 * t + 0.00921;
    float  cy = -0.00792 * t + 0.21023;	      float  dy = -0.04405 * t - 1.65369;
    float  ey = -0.01092 * t + 0.05291;	  
	return float3 ( (1.0 + aY * exp(bY/cosTheta)) * (1.0 + cY * exp(dY * gamma) + eY*cosGammaSq),
                    (1.0 + ax * exp(bx/cosTheta)) * (1.0 + cx * exp(dx * gamma) + ex*cosGammaSq),
                    (1.0 + ay * exp(by/cosTheta)) * (1.0 + cy * exp(dy * gamma) + ey*cosGammaSq) );
}


float3  perezSky ( float t, float cosTheta, float cosGamma, float cosThetaSun )
{
    float	thetaSun = ACOS ( cosThetaSun );
    float3  zenith   = perezZenith ( t, thetaSun );
    float3  clrYxy   = zenith * perezFunc ( t, cosTheta, cosGamma ) / perezFunc ( t, 1.0, cosThetaSun );

    return clrYxy;
}


float3 perezSun ( float t, float cosThetaSun, float boost )
{
	return perezSky( t, cosThetaSun, 1, cosThetaSun ) * float3(boost,1,1);
}

float3 YxyToRGB ( float3 Yxy )
{
    float3  clrYxy = Yxy;
    float3  XYZ;
    //clrYxy.x = 1.0 - exp ( -clrYxy.x / Exposure );

    XYZ.x = clrYxy.x * clrYxy.y / clrYxy.z;     
    XYZ.y = clrYxy.x;              
    XYZ.z = (1 - clrYxy.y - clrYxy.z) * clrYxy.x / clrYxy.z; 

	#ifdef SRGB
    const float3 rCoeffs = float3 ( 3.2404542f, -1.5371385f, -0.4985314f);
    const float3 gCoeffs = float3 (-0.9692660f,  1.8760108f,  0.0415560f);
    const float3 bCoeffs = float3 ( 0.0556434f, -0.2040259f,  1.0572252f);
	#endif

	#ifdef CIERGB
    const float3 rCoeffs = float3 ( 2.3706743f, -0.9000405f, -0.4706338f);
    const float3 gCoeffs = float3 (-0.5138850f,  1.4253036f,  0.0885814f);
    const float3 bCoeffs = float3 ( 0.0052982f, -0.0146949f,  1.0093968f);
	#endif

	// old values :
    // const float3 rCoeffs = float3 ( 2.0413690f, -0.5649464f, -0.3446944f);
    // const float3 gCoeffs = float3 (-0.9692660f,  1.8760108f,  0.0415560f);
    // const float3 bCoeffs = float3 ( 0.0134474f, -0.1183897f,  1.0154096f);
	
	return float3 ( dot ( rCoeffs, XYZ ), dot ( gCoeffs, XYZ ), dot ( bCoeffs, XYZ ) );
}



/*-------------------------------------------------------------------------------------------------
	Shaders :
-------------------------------------------------------------------------------------------------*/

VS_OUTPUT VSMain( VS_INPUT input )
{
	VS_OUTPUT output;

	output.position = mul( float4( input.position, 1.0f ), MatrixWVP);
	output.worldPos = input.position;

	float3 v = normalize(output.worldPos);
	float3 l = normalize(SunPosition); 
	output.skyColor		= perezSky( Turbidity, max ( v.y, 0.0 ) + 0.05, dot ( l, v ), l.y );

	return output;
}


float screen(float a, float b) 
{ 
	return 1 - (1-a) * (1-b); 
}


float3 screen(float3 a, float3 b) 
{ 
	return 1 - (1-a) * (1-b); 
}


float overlay(float a, float b) 
{ 
	float r;
	if (a<0.5) {
		r = 2 * b * a;
	} else {
		r = 1 - 2 * (1-a) * (1-b);
	}
	return r;
}


float4 PSMain( PS_INPUT input ) : SV_TARGET0
{
	float3 view = normalize(input.worldPos);
	float3 sky = 0.0f;

	#if defined(PROCEDURAL_SKY) || defined(FOG)
		sky = YxyToRGB( input.skyColor ).xyz;// * Temperature * 1;
		sky *= SkyIntensity;

		#ifdef PROCEDURAL_SKY
			float  ldv = dot ( normalize(SunPosition), view );
			float sunFactor = smoothstep( 0.9999f, 0.99991f, ldv );
			sky.rgb += SunColor * sunFactor;
		#endif
		
		return float4( sky, 1.0f);
	#endif
	
	#ifdef CLOUDS
		float4 cirrus	=	CirrusTexture.Sample( SamplerLinear, input.worldPos.xz * float2(2,-2) + float2(0.5,0.5f) );
		cirrus.a *= 0.1f;
		
		//return cirrus * float4(200,100,50,1);
		
		float4 cloudTex	=	CloudTexture.Sample( SamplerLinear, input.worldPos.xz * float2(3,-3)*2 + float2(0.5,0.5f) + float2(Time,Time)*0.004 );
		float4 clouds  = 0;
		float dist		=	length(input.worldPos.xz*2);
		float fog		=	pow(1-saturate( dist ), 0.5);
		
		clouds.rgb	=	saturate(cloudTex.r) * 1 * pow(cloudTex.g,1);
		clouds.a	=	cloudTex.a  * fog;

		clouds = lerp(cirrus * fog, clouds, clouds.a );
		
		#if 1
		float4 cloudTex2	=	CloudTexture.Sample( SamplerLinear, input.worldPos.xz * float2(2.7,-2)*2+ float2(1.7,2.5f) + float2(Time,Time)*0.003 );
		float4 clouds2		= 	0;
		
		clouds2.rgb	=	saturate(cloudTex2.r) * 1 * pow(cloudTex2.g,1);
		clouds2.a	=	cloudTex2.a  * fog;
		
		clouds = lerp(clouds, clouds2, clouds2.a );


		float4 cloudTex3	=	CloudTexture.Sample( SamplerLinear, input.worldPos.xz * float2(1.7,-1.2)*2 + float2(0.35,0.75f) + float2(Time,Time)*0.002 );
		float4 clouds3		= 	0;
		
		clouds3.rgb	=	saturate(cloudTex3.r) * 1 * pow(cloudTex3.g,1);
		clouds3.a	=	cloudTex3.a  * fog;
		
		clouds = lerp(clouds, clouds3, clouds3.a );
		


		float4 cloudTex4	=	CloudTexture.Sample( SamplerLinear, input.worldPos.xz * float2(0.7,-1.1)*2 + float2(0.65,0.25f) + float2(Time,Time)*0.001 );
		float4 clouds4		= 	0;
		
		clouds4.rgb	=	saturate(cloudTex4.r) * 1 * pow(cloudTex4.g,1);
		clouds4.a	=	cloudTex4.a  * fog;
		
		clouds = lerp(clouds, clouds4, clouds4.a );
		#endif
		



		//clouds.a = pow(clouds.a,0.5);
		clouds.rgb = lerp(Ambient.rgb, SunColor, pow(clouds.r,2)); 

		return clouds;
	#endif

}






























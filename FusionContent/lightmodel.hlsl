
/*-----------------------------------------------------------------------------
	Lamber and Blinn lighting models
-----------------------------------------------------------------------------*/

static const float PI = 3.141592f;

float	sqr(float a) {
	return a * a;
}


float3	Lambert( float3 normal, float3 light_dir, float3 intensity, float3 diff_color, float bias = 0 )
{
	light_dir	=	normalize(light_dir);
	return intensity * diff_color * max( 0, dot(light_dir, normal) + bias ) / (1+bias);
}


float3 	Blinn		( float3 N, float3 V, float3 L, float3 I, float3 F, float m )
{
	float	power	=	50/sqrt(m);
			L		=	normalize( L );
			V		=	normalize( V );
	float3	H		=	normalize( V + L );
	return	I * F * pow( max(0, dot(H, N)), power) / sqr(m);
}

float3 	BlinnP		( float3 N, float3 V, float3 L, float3 I, float3 F, float power )
{
			L		=	normalize( L );
			V		=	normalize( V );
	float3	H		=	normalize( V + L );
	
	float	d		=	saturate(dot ( N, H ));
	float	t		=	0.04;
	float	ca		=	pow( t, 1 / power );
	float	na		=	(d - 1)/(ca - 1);
	
	return	I * F * pow( max(0, dot(H, N)), abs(power)+0.01) * (power+1)/3.14;
}


float	LinearFalloff( float dist, float max_range )
{
	float fade = 0;
	fade = saturate(1 - (dist / max_range));
	fade *= fade;
	
	return fade;
}


/*-----------------------------------------------------------------------------
	Cook-Torrance lighting model
-----------------------------------------------------------------------------*/

float	Fresnel( float c, float Fn )
{
	return Fn + (1-Fn) * pow(2, (-5.55473 * c - 6.98316)*c );
	//return Fn + (1-Fn) * pow(1-c, 5);
}


float3	CookTorrance( float3 N, float3 V, float3 L, float3 I, float3 F, float m )
{
			L	=	normalize(L);
			V	=	normalize(V);
	float3	H	=	normalize(V+L);
	
	
	// UE4 :
	#ifdef USE_UE4
		// float 	k	=	sqr(m+1)/8;
		// float	g1	=	dot(N,L) / ( dot(N,L) * (1-k) + k );
		// float	g2	=	dot(N,V) / ( dot(N,V) * (1-k) + k );
		// float	G	=	g1 * g2;

		float G = 0;
		float	g1	=	2 * dot(N,H) * dot(N,V) / dot(V,H);
		float	g2	=	2 * dot(N,H) * dot(N,L) / dot(V,H);
		G	=	min(1, min(g1, g2));
		
		float	a	=	m*m;
		float	D	=	a * a / PI / sqr(sqr(dot(N,H)) * (a*a-1)+1);

		F.r  = Fresnel(dot(V,H), F.r);
		F.g  = Fresnel(dot(V,H), F.g);
		F.b  = Fresnel(dot(V,H), F.b);
							  
		return max(0, I * F * D * G / (4*dot(N,L)*dot(V,N)));
	#else

		float G = 0;
		float	g1	=	2 * dot(N,H) * dot(N,V) / dot(V,H);
		float	g2	=	2 * dot(N,H) * dot(N,L) / dot(V,H);
		G	=	min(1, min(g1, g2));
		
		float	cos_a	=	dot(N,H);
		float	sin_a	=	sqrt(abs(1 - cos_a * cos_a)); // ABS to avoid negative values
		
		float	D	=	exp( -(sin_a*sin_a) / (cos_a*cos_a) / (m*m) ) / (4 * m*m * cos_a * cos_a * cos_a * cos_a );

		F.r  = Fresnel(dot(V,H), F.r);
		F.g  = Fresnel(dot(V,H), F.g);
		F.b  = Fresnel(dot(V,H), F.b);//*/
							  
		return max(0, I * F * D * G / (4*dot(N,L)*dot(V,N)));
	#endif
}



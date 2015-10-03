using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpDX;
using Fusion.Core.Mathematics;

namespace Fusion.Pipeline.Utils {
	internal partial class Image {

		static Random rand = new Random();


		/// <summary>
		/// Fills image with 
		/// </summary>
		/// <param name="seed"></param>
		/// <param name="monochrome"></param>
		public void Fill ( Color4 color )
		{
			PerpixelProcessing( p => color );
		}


		/// <summary>
		/// Fills image with 
		/// </summary>
		/// <param name="seed"></param>
		/// <param name="monochrome"></param>
		public void FillUniformNoise ( bool monochrome )
		{
			if ( monochrome ) {
				float v = rand.NextFloat(0,1);
				PerpixelProcessing( p => { v = rand.NextFloat(0,1); return new Color4(v,v,v,1); } );
			} else {
				PerpixelProcessing( p => rand.NextColor4() );
			}
		}



		/// <summary>
		/// Generate perline noise
		/// </summary>
		/// <param name="kernelSize"></param>
		/// <param name="numHarmonics"></param>
		/// <param name="persistence"></param>
		/// <returns></returns>
		static public Image GeneratePerlinNoise ( int kernelSize, int numHarmonics, float persistence = 0.5f )
		{
			var kernel = new Image[numHarmonics];

			for ( int i=0; i<numHarmonics; i++) {
				kernel[i] = new Image( kernelSize << i, kernelSize << i );
				kernel[i].FillUniformNoise( true );
				kernel[i].Normalize();
			}


			int size = kernelSize << numHarmonics;

			Image noise = new Image( size, size );

			for ( int x = 0; x<size; x++ ) {
				for ( int y = 0; y<size; y++ ) {

					Color4 value = Color4.Zero;
					float   freq  = 1.0f;
					float	amp   = 0.5f;
					float	fx    = x / (float)size;
					float	fy    = y / (float)size;

					for (int i=0; i<numHarmonics; i++) {
						value += kernel[i].Sample( fx * freq, fy * freq, true, true ) * amp;

						amp  *= persistence;
						freq *= 2;
					}
					
					noise.Write( x, y, value, true );
				}
			}

			noise.Normalize();

			return noise;
		}



		/// <summary>
		/// ComputeGradient
		/// </summary>
		public Image ComputeGradient ()
		{
			Image image = new Image( Width, Height ); 

			for ( int i=0; i<Width; i++ ) {
				for ( int j=0; j<Height; j++) {

					float center = Luminance( Sample( i, j ) );

					float nx = 0.5f * ( center - Luminance( Sample( i+1, j ) ) )
							 + 0.5f * ( Luminance( Sample( i-1, j ) ) - center );
					
					float ny = 0.5f * ( center - Luminance( Sample( i, j+1 ) ) )
							 + 0.5f * ( Luminance( Sample( i, j-1 ) ) - center );

					float nz = (float)Math.Sqrt( MathUtil.Clamp( 1 - nx*nx + ny*ny, 0, 1 ) );

					image.Write( i, j, new Color4( nx, ny, nz, center ) );
				}
			}

			return image;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="amount"></param>
		/// <returns></returns>
		public Image GradientDistortion ( float amount )
		{
			Image image		=	new Image( Width, Height ); 
			Image dudvImage	=	this.ComputeGradient();

			for ( int i=0; i<Width; i++ ) {
				for ( int j=0; j<Height; j++) {

					float x = i/(float)Width;
					float y = j/(float)Height;

					Color4 dudv	=	dudvImage.Sample( x, y );
					Color4 v		=	Sample( x + dudv.Red * amount, y + dudv.Green * amount );

					image.Write( i, j, v );
				}
			}

			return image;
		}


		/// <summary>
		/// http://www.lighthouse3d.com/opengl/terrain/index.php3?particle
		/// </summary>
		/// <param name="iterations"></param>
		/// <param name="displace"></param>
		/// <returns></returns>
		public static Image ParticleDeposition ( int sizeX, int sizeY, int iterations, float displace )
		{
			var rand	= new Random();
			var image	= new Image( sizeX, sizeY );

			int x = rand.Next() % sizeX;
			int y = rand.Next() % sizeY;

			var disp = new Color4( displace, displace, displace, displace );

			for (int i=0; i<iterations; i++) {
				int dir = rand.Next(4);
				if (dir==0) x++;
				if (dir==1) x--;
				if (dir==2) y++;
				if (dir==3) y--;

				var value = image.Sample(x,y);
				image.Write( x,y, value + disp );
			}

			return image;
		}
	}
}

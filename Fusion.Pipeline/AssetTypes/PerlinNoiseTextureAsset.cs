using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion;
using Fusion.Graphics;


namespace Fusion.Content {
	
	[Asset("Content", "Perlin Noise Texture")]
	public class PerlinNoiseTextureAsset : Asset {


		public override string[] Dependencies
		{
			get { return new string[]{}; }
		}


		public int		Seed		{ get; set; }
		public int		Octaves		{ get; set; }
		public float	Amplitude	{ get; set; }
		public float	Persistence { get; set; }
		public float	Frequency	{ get; set; }
			
		public int		Width		{ get; set; }
		public int		Height		{ get; set; }

		public Color	Color0		{ get; set; }
		public Color	Color1		{ get; set; }
	
		public PerlinNoiseTextureAsset ()
		{
			var r		=	new Random();
			Seed		=	r.Next(Int32.MaxValue);
			Octaves		=	32;
			Amplitude	=	0.5f;
			Persistence =	0.1f;
			Frequency	=	0.01f;
			Width		=	256;
			Height		=	256;

			Color0		=	Color.Black;
			Color1		=	Color.White;
		}


		public override void Build ( BuildContext buildContext )
		{
			Log.Warning("PerlinNoiseTextureAsset is an experimental asset.");

			var noise	=	new Image( Width, Height );

			noise.PerpixelProcessing( (x,y,c) => {
				var n = Noise(x,y);
				return new Color4( n,n,n,1 );
			});

			noise.Normalize();

			Image.SaveTga( noise, buildContext.TargetPath( this ) );
		}



		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Noise stuff :
		 * 
		-----------------------------------------------------------------------------------------*/

        public float Noise(int x, int y)
        {
            //returns -1 to 1
            float total = 0.0f;
            float freq = Frequency, amp = Amplitude;

            for (int i = 0; i < Octaves; ++i)
            {
                total = total + Smooth(x * freq, y * freq) * amp;
                freq *= 2;
                amp *= Persistence;
            }
            /*if (total < -2.4f) total = -2.4f;
            else if (total > 2.4f) total = 2.4f;*/
            //total = total * 1.5;
            return total;
        }


        private float NoiseGeneration(int x, int y)
        {
            int n = x + y * 57;
            n = (n << 13) ^ n;

            return (1.0f - ((n * (n * n * 15731 + 789221) + Seed) & 0x7fffffff) / 1073741824.0f);
        }


        private float Interpolate(float x, float y, float a)
        {
            float value = (1 - (float)Math.Cos(a * Math.PI)) * 0.5f;
            return x * (1 - value) + y * value;
        }


        private float Smooth(float x, float y)
        {
            float n1 = NoiseGeneration((int)x, (int)y);
            float n2 = NoiseGeneration((int)x + 1, (int)y);
            float n3 = NoiseGeneration((int)x, (int)y + 1);
            float n4 = NoiseGeneration((int)x + 1, (int)y + 1);

            float i1 = Interpolate(n1, n2, x - (int)x);
            float i2 = Interpolate(n3, n4, x - (int)x);

            return Interpolate(i1, i2, y - (int)y);
        }
	} 
}

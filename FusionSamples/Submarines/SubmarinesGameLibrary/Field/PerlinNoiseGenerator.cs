using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary.Field
{
    static class PerlinNoiseGenerator
    {
        public static int Seed { get; set; }

        public static int Octaves { get; set; }

        public static double Amplitude { get; set; }

        public static double Persistence { get; set; }

        public static double Frequency { get; set; }

        static PerlinNoiseGenerator()
        {
            Random r = new Random();
            //LOOOL
            PerlinNoiseGenerator.Seed = r.Next(Int32.MaxValue);
            PerlinNoiseGenerator.Octaves = 32;
            PerlinNoiseGenerator.Amplitude = 1;
            PerlinNoiseGenerator.Frequency = 0.1;
            PerlinNoiseGenerator.Persistence = 0.7;
        }

        public static double Noise(int x, int y)
        {
            //returns -1 to 1
            //Console.WriteLine(PerlinNoiseGenerator.Seed);
            double total = 0.0;
            double freq = PerlinNoiseGenerator.Frequency, amp = PerlinNoiseGenerator.Amplitude;
            for (int i = 0; i < PerlinNoiseGenerator.Octaves; ++i)
            {
                total = total + PerlinNoiseGenerator.Smooth(x * freq, y * freq) * amp;
                freq *= 2;
                amp *= PerlinNoiseGenerator.Persistence;
            }
            if (total < -2.4) total = -2.4;
            else if (total > 2.4) total = 2.4;
            //Console.WriteLine(total);
            return (total * 100);
        }

        public static double NoiseGeneration(int x, int y)
        {
            int n = x + y * 57;
            n = (n << 13) ^ n;
            return (1.0 - ((n * (n * n * 15731 + 789221) + PerlinNoiseGenerator.Seed) & 0x7fffffff) / 1073741824.0);
        }

        private static double Interpolate(double x, double y, double a)
        {
            double value = (1 - Math.Cos(a * Math.PI)) * 0.5;
            return x * (1 - value) + y * value;
        }

        private static double Smooth(double x, double y)
        {
            double n1 = NoiseGeneration((int)x, (int)y);
            double n2 = NoiseGeneration((int)x + 1, (int)y);
            double n3 = NoiseGeneration((int)x, (int)y + 1);
            double n4 = NoiseGeneration((int)x + 1, (int)y + 1);

            double i1 = Interpolate(n1, n2, x - (int)x);
            double i2 = Interpolate(n3, n4, x - (int)x);

            return Interpolate(i1, i2, y - (int)y);
        }
    }
}

// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// -----------------------------------------------------------------------------
// Original code from SlimMath project. http://code.google.com/p/slimmath/
// Greetings to SlimDX Group. Original code published with the following license:
// -----------------------------------------------------------------------------
/*
* Copyright (c) 2007-2011 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/
using System;
using System.Diagnostics;

namespace Fusion.Core.Mathematics
{
    public static class MathUtil
    {
        /// <summary>
        /// The value for which all absolute numbers smaller than are considered equal to zero.
        /// </summary>
        public const float ZeroTolerance = 1e-6f; // Value a 8x higher than 1.19209290E-07F

        /// <summary>
        /// A value specifying the approximation of π which is 180 degrees.
        /// </summary>
        public const float Pi = (float)Math.PI;

        /// <summary>
        /// A value specifying the approximation of 2π which is 360 degrees.
        /// </summary>
        public const float TwoPi = (float)(2 * Math.PI);

        /// <summary>
        /// A value specifying the approximation of π/2 which is 90 degrees.
        /// </summary>
        public const float PiOverTwo = (float)(Math.PI / 2);

        /// <summary>
        /// A value specifying the approximation of π/4 which is 45 degrees.
        /// </summary>
        public const float PiOverFour = (float)(Math.PI / 4);

        /// <summary>
        /// Checks if a and b are almost equals, taking into account the magnitude of floating point numbers (unlike <see cref="WithinEpsilon"/> method). See Remarks.
        /// See remarks.
        /// </summary>
        /// <param name="a">The left value to compare.</param>
        /// <param name="b">The right value to compare.</param>
        /// <returns><c>true</c> if a almost equal to b, <c>false</c> otherwise</returns>
        /// <remarks>
        /// The code is using the technique described by Bruce Dawson in 
        /// <a href="http://randomascii.wordpress.com/2012/02/25/comparing-floating-point-numbers-2012-edition/">Comparing Floating point numbers 2012 edition</a>. 
        /// </remarks>
        public unsafe static bool NearEqual(float a, float b)
        {
            // Check if the numbers are really close -- needed
            // when comparing numbers near zero.
            if (IsZero(a - b))
                return true;

            // Original from Bruce Dawson: http://randomascii.wordpress.com/2012/02/25/comparing-floating-point-numbers-2012-edition/
            int aInt = *(int*)&a;
            int bInt = *(int*)&b;

            // Different signs means they do not match.
            if((aInt < 0) != (bInt < 0))
                return false;

            // Find the difference in ULPs.
            int ulp = Math.Abs(aInt - bInt);

            // Choose of maxUlp = 4
            // according to http://code.google.com/p/googletest/source/browse/trunk/include/gtest/internal/gtest-internal.h
            const int maxUlp = 4;
            return (ulp <= maxUlp);
        }

        /// <summary>
        /// Determines whether the specified value is close to zero (0.0f).
        /// </summary>
        /// <param name="a">The floating value.</param>
        /// <returns><c>true</c> if the specified value is close to zero (0.0f); otherwise, <c>false</c>.</returns>
        public static bool IsZero(float a)
        {
            return Math.Abs(a) < ZeroTolerance;
        }

        /// <summary>
        /// Determines whether the specified value is close to one (1.0f).
        /// </summary>
        /// <param name="a">The floating value.</param>
        /// <returns><c>true</c> if the specified value is close to one (1.0f); otherwise, <c>false</c>.</returns>
        public static bool IsOne(float a)
        {
            return IsZero(a - 1.0f);
        }

        /// <summary>
        /// Checks if a - b are almost equals within a float epsilon.
        /// </summary>
        /// <param name="a">The left value to compare.</param>
        /// <param name="b">The right value to compare.</param>
        /// <param name="epsilon">Epsilon value</param>
        /// <returns><c>true</c> if a almost equal to b within a float epsilon, <c>false</c> otherwise</returns>
        public static bool WithinEpsilon(float a, float b, float epsilon)
        {
            float num = a - b;
            return ((-epsilon <= num) && (num <= epsilon));
        }

        /// <summary>
        /// Converts revolutions to degrees.
        /// </summary>
        /// <param name="revolution">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static float RevolutionsToDegrees(float revolution)
        {
            return revolution * 360.0f;
        }

        /// <summary>
        /// Converts revolutions to radians.
        /// </summary>
        /// <param name="revolution">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static float RevolutionsToRadians(float revolution)
        {
            return revolution * TwoPi;
        }

        /// <summary>
        /// Converts revolutions to gradians.
        /// </summary>
        /// <param name="revolution">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static float RevolutionsToGradians(float revolution)
        {
            return revolution * 400.0f;
        }

        /// <summary>
        /// Converts degrees to revolutions.
        /// </summary>
        /// <param name="degree">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static float DegreesToRevolutions(float degree)
        {
            return degree / 360.0f;
        }

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        /// <param name="degree">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static float DegreesToRadians(float degree)
        {
            return degree * (Pi / 180.0f);
        }

        /// <summary>
        /// Converts radians to revolutions.
        /// </summary>
        /// <param name="radian">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static float RadiansToRevolutions(float radian)
        {
            return radian / TwoPi;
        }

        /// <summary>
        /// Converts radians to gradians.
        /// </summary>
        /// <param name="radian">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static float RadiansToGradians(float radian)
        {
            return radian * (200.0f / Pi);
        }

        /// <summary>
        /// Converts gradians to revolutions.
        /// </summary>
        /// <param name="gradian">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static float GradiansToRevolutions(float gradian)
        {
            return gradian / 400.0f;
        }

        /// <summary>
        /// Converts gradians to degrees.
        /// </summary>
        /// <param name="gradian">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static float GradiansToDegrees(float gradian)
        {
            return gradian * (9.0f / 10.0f);
        }

        /// <summary>
        /// Converts gradians to radians.
        /// </summary>
        /// <param name="gradian">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static float GradiansToRadians(float gradian)
        {
            return gradian * (Pi / 200.0f);
        }

        /// <summary>
        /// Converts radians to degrees.
        /// </summary>
        /// <param name="radian">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static float RadiansToDegrees(float radian)
        {
            return radian * (180.0f / Pi);
        }

        /// <summary>
        /// Clamps the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        /// <returns>The result of clamping a value between min and max</returns>
        public static float Clamp(float value, float min, float max)
        {
            return value < min ? min : value > max ? max : value;
        }

        /// <summary>
        /// Clamps the specified value. Inclusivly.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        /// <returns>The result of clamping a value between min and max</returns>
        public static int Clamp(int value, int min, int max)
        {
            return value < min ? min : value > max ? max : value;
        }


		///// <summary>
		///// Wraps value between min and max.
		///// </summary>
		///// <param name="x">The value.</param>
		///// <param name="a">The min. Inclusive.</param>
		///// <param name="b">The max. Exclusive.</param>
		///// <returns></returns>
		//public static int Wrap(int x, int a, int b)
		//{
		//	var r = (x-a)%(b-a);
		//	return (r < 0) ? r + a + b-a : r + a;
		//}
    

		///// <summary>
		///// Wraps value between min and max.
		///// </summary>
		///// <param name="x">The value.</param>
		///// <param name="a">The min. Inclusive.</param>
		///// <param name="b">The max. Exclusive.</param>
		///// <returns></returns>
		//public static float Wrap(float x, float a, float b)
		//{
		//	var r = (x-a)%(b-a);
		//	return (r < 0) ? r + a + b-a : r + a;
		//}
    

        /// <summary>
        /// Interpolates between two values using a linear function by a given amount.
        /// </summary>
        /// <remarks>
        /// See http://www.encyclopediaofmath.org/index.php/Linear_interpolation and
        /// http://fgiesen.wordpress.com/2012/08/15/linear-interpolation-past-present-and-future/
        /// </remarks>
        /// <param name="from">Value to interpolate from.</param>
        /// <param name="to">Value to interpolate to.</param>
        /// <param name="amount">Interpolation amount.</param>
        /// <returns>The result of linear interpolation of values based on the amount.</returns>
        public static double Lerp(double from, double to, double amount)
        {
            return (1 - amount) * from + amount * to;
        }

        /// <summary>
        /// Interpolates between two values using a linear function by a given amount.
        /// </summary>
        /// <remarks>
        /// See http://www.encyclopediaofmath.org/index.php/Linear_interpolation and
        /// http://fgiesen.wordpress.com/2012/08/15/linear-interpolation-past-present-and-future/
        /// </remarks>
        /// <param name="from">Value to interpolate from.</param>
        /// <param name="to">Value to interpolate to.</param>
        /// <param name="amount">Interpolation amount.</param>
        /// <returns>The result of linear interpolation of values based on the amount.</returns>
        public static float Lerp(float from, float to, float amount)
        {
            return (1 - amount) * from + amount * to;
        }

        /// <summary>
        /// Interpolates between two values using a linear function by a given amount.
        /// </summary>
        /// <remarks>
        /// See http://www.encyclopediaofmath.org/index.php/Linear_interpolation and
        /// http://fgiesen.wordpress.com/2012/08/15/linear-interpolation-past-present-and-future/
        /// </remarks>
        /// <param name="from">Value to interpolate from.</param>
        /// <param name="to">Value to interpolate to.</param>
        /// <param name="amount">Interpolation amount.</param>
        /// <returns>The result of linear interpolation of values based on the amount.</returns>
        public static byte Lerp(byte from, byte to, float amount)
        {
            return (byte)Lerp((float)from, (float)to, amount);
        }

        /// <summary>
        /// Performs smooth (cubic Hermite) interpolation between 0 and 1.
        /// </summary>
        /// <remarks>
        /// See https://en.wikipedia.org/wiki/Smoothstep
        /// </remarks>
        /// <param name="amount">Value between 0 and 1 indicating interpolation amount.</param>
        public static float SmoothStep(float amount)
        {
            return (amount <= 0) ? 0
                : (amount >= 1) ? 1
                : amount * amount * (3 - (2 * amount));
        }

        /// <summary>
        /// Performs a smooth(er) interpolation between 0 and 1 with 1st and 2nd order derivatives of zero at endpoints.
        /// </summary>
        /// <remarks>
        /// See https://en.wikipedia.org/wiki/Smoothstep
        /// </remarks>
        /// <param name="amount">Value between 0 and 1 indicating interpolation amount.</param>
        public static float SmootherStep(float amount)
        {
            return (amount <= 0) ? 0
                : (amount >= 1) ? 1
                : amount * amount * amount * (amount * ((amount * 6) - 15) + 10);
        }

        /// <summary>
        /// Calculates the modulo of the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="modulo">The modulo.</param>
        /// <returns>The result of the modulo applied to value</returns>
        public static float Mod(float value, float modulo)
        {
            if (modulo == 0.0f)
            {
                return value;
            }

            return value % modulo;
        }

        /// <summary>
        /// Calculates the modulo 2*PI of the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the modulo applied to value</returns>
        public static float Mod2PI(float value)
        {
            return Mod(value, TwoPi);
        }

        /// <summary>
        /// Wraps the specified value into a range [min, max]
        /// </summary>
        /// <param name="value">The value to wrap.</param>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        /// <returns>Result of the wrapping.</returns>
        /// <exception cref="ArgumentException">Is thrown when <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
        public static int Wrap(int value, int min, int max)
        {
            if (min > max)
                throw new ArgumentException(string.Format("min {0} should be less than or equal to max {1}", min, max), "min");

            // Code from http://stackoverflow.com/a/707426/1356325
            int range_size = max - min + 1;

            if (value < min)
                value += range_size * ((min - value) / range_size + 1);

            return min + (value - min) % range_size;
        }

        /// <summary>
        /// Wraps the specified value into a range [min, max[
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        /// <returns>Result of the wrapping.</returns>
        /// <exception cref="ArgumentException">Is thrown when <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
        public static float Wrap(float value, float min, float max)
        {
            if (NearEqual(min, max)) return min;

            double mind = min;
            double maxd = max;
            double valued = value;

            if (mind > maxd)
                throw new ArgumentException(string.Format("min {0} should be less than or equal to max {1}", min, max), "min");

            var range_size = maxd - mind;
            return (float)(mind + (valued - mind) - (range_size * Math.Floor((valued - mind) / range_size)));
        }

        /// <summary>
        /// Gauss function.
        /// </summary>
        /// <param name="amplitude">Curve amplitude.</param>
        /// <param name="x">Position X.</param>
        /// <param name="y">Position Y</param>
        /// <param name="radX">Radius X.</param>
        /// <param name="radY">Radius Y.</param>
        /// <param name="sigmaX">Curve sigma X.</param>
        /// <param name="sigmaY">Curve sigma Y.</param>
        /// <returns>The result of Gaussian function.</returns>
        public static float Gauss(float amplitude, float x, float y, float radX, float radY, float sigmaX, float sigmaY)
        {
            return (float)Gauss((double)amplitude, x, y, radX, radY, sigmaX, sigmaY);
        }

        /// <summary>
        /// Gauss function.
        /// </summary>
        /// <param name="amplitude">Curve amplitude.</param>
        /// <param name="x">Position X.</param>
        /// <param name="y">Position Y</param>
        /// <param name="radX">Radius X.</param>
        /// <param name="radY">Radius Y.</param>
        /// <param name="sigmaX">Curve sigma X.</param>
        /// <param name="sigmaY">Curve sigma Y.</param>
        /// <returns>The result of Gaussian function.</returns>
        public static double Gauss(double amplitude, double x, double y, double radX, double radY, double sigmaX, double sigmaY)
        {
            return (amplitude * Math.E) -
                (
                    Math.Pow(x - (radX / 2), 2) / (2 * Math.Pow(sigmaX, 2)) +
                    Math.Pow(y - (radY / 2), 2) / (2 * Math.Pow(sigmaY, 2))
                );
        }


		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Fusion Math
		 * 
		-----------------------------------------------------------------------------------------*/

		static public Half4		ToHalf4( Color4 c )				{ return new Half4( c.Red, c.Green, c.Blue, c.Alpha ); }
		static public Half4		ToHalf4( Vector3 v, float w )	{ return new Half4( v.X, v.Y, v.Z, w ); }
		static public Half2		ToHalf2( Vector2 v )			{ return new Half2( v.X, v.Y ); }
		

		/// <summary>
		/// Converts degrees to radians
		/// </summary>
		/// <param name="deg"></param>
		/// <returns></returns>
		static public float Rad ( float deg ) { return MathUtil.DegreesToRadians( deg ); }

		/// <summary>
		/// Converts radians to degrees
		/// </summary>
		/// <param name="rad"></param>
		/// <returns></returns>
		static public float Deg ( float rad ) { return MathUtil.RadiansToDegrees( rad ); }

		/// <summary>
		/// Square
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		static public float	Square ( float value ) { return value * value; }


		/// <summary>
		/// Gets the center of bounding box
		/// </summary>
		/// <param name="self"></param>
		/// <returns></returns>
		static public Vector3 Center ( this BoundingBox self ) { return 0.5f * (self.Minimum + self.Maximum); }

		/// <summary>
		/// Gets the size of bounding box
		/// </summary>
		/// <param name="self"></param>
		/// <returns></returns>
		static public Vector3 Size ( this BoundingBox self ) { return self.Maximum - self.Minimum; }

		/// <summary>
		/// Divide a by b rounding result up
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		static public int IntDivUp ( int a, int b ) { return (a % b != 0) ? (a / b + 1) : (a / b); }

		/// <summary>
		/// Whether x is power of two
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		static public bool IsPowerOfTwo ( int x ) {  return ((x & (x - 1)) == 0) && (x!=0); }

		/// <summary>
		/// Finds integer log base 2 of an integer (aka the position of the highest bit set)
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		static public int LogBase2 ( int x ) {  
			int v = x;	// 32-bit word to find the log base 2 of
			int r = 0;			// r will be lg(v)

			while (v!=0) {
				v = v >> 1;
				r++;
			}
			return r;
		}

		/// <summary>
		/// Lerps a to b
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		static public int Lerp ( int from, int to, float amount ) { return (int)(((float)from) * (1.0f-amount) + ((float)to) * amount); }

		/// <summary>
		/// Saturates value
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		static public double Saturate		( double x ) { return Math.Min( 1.0, Math.Max( 0.0, x ) ); }

		/// <summary>
		/// Divide a by b rounding result up
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		static public int IntDivRoundUp	( int a, int b ) { return ( a + b - 1 ) / b; }
							 
		/// <summary>
		/// Checks whether value is odd
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		static public bool IsOdd ( int a ) { return ( ( a & 1 ) == 1 ); }

		/// <summary>
		/// Checks whether value is even
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		static public bool IsEven ( int a ) { return ( ( a & 1 ) == 0 ); }

		/// <summary>
		/// Decompose matrix to yaw, pitch, roll
		/// </summary>
		/// <param name="mat"></param>
		/// <param name="yaw"></param>
		/// <param name="pitch"></param>
		/// <param name="roll"></param>
		public static void ToAngles ( this Matrix mat, out float yaw, out float pitch, out float roll )
		{
			pitch		=	0;
			roll		=	0;
			yaw			=	(float)Math.Atan2( mat.Backward.X, mat.Backward.Z );	
			
			pitch		=	(float)Math.Asin( mat.Forward.Y );

			var	xp		=	Vector3.Cross( mat.Forward, Vector3.UnitY );
			var	xp2		=	Vector3.Cross( mat.Forward, Vector3.UnitY );
			xp.Normalize();

			var dotY	=	MathUtil.Clamp( Vector3.Dot( mat.Up, xp ), -1, 1 );
			var dotX	=	MathUtil.Clamp( Vector3.Dot( mat.Right, xp ), -1, 1 );

			roll		=	- (float)Math.Atan2( dotY, dotX );

			Debug.Assert( !float.IsNaN(yaw) );
			Debug.Assert( !float.IsNaN(pitch) );
			Debug.Assert( !float.IsNaN(roll) );

			return;
		}	


		/// <summary>
		/// 
		/// </summary>
		/// <param name="mat"></param>
		/// <param name="yaw"></param>
		/// <param name="pitch"></param>
		/// <param name="roll"></param>
		public static void ToAnglesDeg ( this Matrix mat, out float yaw, out float pitch, out float roll )
		{
			ToAngles( mat, out yaw, out pitch, out roll );
			yaw		=	MathUtil.RadiansToDegrees( yaw	 );
			pitch	=	MathUtil.RadiansToDegrees( pitch );
			roll	=	MathUtil.RadiansToDegrees( roll	 );
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="right"></param>
		/// <param name="up"></param>
		/// <param name="backward"></param>
		/// <returns></returns>
		public static Matrix Transformation( Vector3 right, Vector3 up, Vector3 backward ) 
		{
			var mat = Matrix.Identity;
			mat.Right = right;
			mat.Up = up;
			mat.Backward = backward;
			return mat;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="znear"></param>
		/// <param name="zfar"></param>
		/// <returns></returns>
		public static Matrix InversePerspectiveRH( float width, float height, float znear, float zfar ) 
		{
			float halfWidth = 0.5f * width;
			float halfHeight = 0.5f* height;
			return InversePerspectiveOffCenterRH( -halfWidth, halfWidth, -halfHeight, halfHeight, znear, zfar );
		}

		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <param name="bottom"></param>
		/// <param name="top"></param>
		/// <param name="znear"></param>
		/// <param name="zfar"></param>
		/// <returns></returns>
		public static Matrix InversePerspectiveOffCenterRH( float left, float right, float bottom, float top, float znear, float zfar )
        {
			float k = 0.5f / znear;
		
			return new Matrix
			(
				k * ( right - left ),					0.0f,	 0.0f,							  0.0f,
								0.0f,	k * ( top - bottom ),	 0.0f,							  0.0f,
								0.0f,					0.0f,	 0.0f,	( 1.0f / zfar - 1.0f / znear ),
				k * ( right + left ),	k * ( top + bottom ),	-1.0f,					   1.0f/ znear 
			);
        }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool IsRectInsideRect ( Rectangle r1, Rectangle r2 )
		{
			int X1	=	r1.X;
			int Y1	=	r1.Y;
			int X2	=	r2.X;
			int Y2	=	r2.Y;

			int W1	=	r1.Width;
			int H1	=	r1.Height;
			int W2	=	r2.Width;
			int H2	=	r2.Height;

			if (X1+W1<X2 || X2+W2<X1 || Y1+H1<Y2 || Y2+H2<Y1) {
				return false;
			} else {
				return true;
			}
		}


        /// <summary>
        /// Does something with arrays.
        /// </summary>
        /// <typeparam name="T">Most likely the type of elements in the array.</typeparam>
        /// <param name="value">Who knows what this is for.</param>
        /// <param name="count">Probably the length of the array.</param>
        /// <returns>An array of who knows what.</returns>
        public static T[] Array<T>(T value, int count)
        {
            T[] result = new T[count];
            for (int i = 0; i < count; i++)
                result[i] = value;

            return result;
        }



		/// <summary>
		/// 
		/// </summary>
		/// <param name="b"></param>
		/// <param name="pos"></param>
		/// <returns></returns>
		public static bool IsBitSet(byte b, int pos)
		{
		   return (b & (1 << pos)) != 0;
		}



		/// <summary>
		/// http://www.dotnetperls.com/bitcount
		/// This is a simple and fast algorithm that walks through all the bits that are set to one. 
		/// It is static. It does not rely on saving state.
		/// Tip: It always has accurate results, but it performs the fastest when most bits are set to zero.
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public static int SparseBitcount(int n)
		{
			int count = 0;
			while (n != 0) {
				count++;
				n &= (n - 1);
			}
			return count;
		}


		/// <summary>
		/// http://www.dotnetperls.com/bitcount
		/// This bitcount is slow simple and reliable. 
		/// It is also static because it doesn't rely on state. 
		/// It should only be considered when simplicity is more important than anything else.
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public static int IteratedBitcount(int n)
		{
			int test = n;
			int count = 0;

			while (test != 0) {
				if ((test & 1) == 1) {
					count++;
				}
				test >>= 1;
			}
			return count;
		}


		/// <summary>
		/// Computes view matricies for each cubemap face in right-handed coordinate system.: Result = initialTransform * LookAt() * postTransform.
		/// </summary>
		/// <param name="origin">Viewpoint origin.</param>
		/// <param name="initialTransform">Initial cubemap transform.</param>
		/// <param name="postTransform">Post transform matrix.</param>
		/// <returns></returns>
		public static Matrix[] ComputeCubemapViewMatriciesRH ( Vector3 origin, Matrix initialTransform, Matrix postTransform )
		{
			return new[] {
				initialTransform * Matrix.LookAtRH( origin, -Vector3.UnitX + origin, Vector3.UnitY ) * postTransform,
				initialTransform * Matrix.LookAtRH( origin,  Vector3.UnitX + origin, Vector3.UnitY ) * postTransform,
				initialTransform * Matrix.LookAtRH( origin, -Vector3.UnitY + origin,-Vector3.UnitZ ) * postTransform,
				initialTransform * Matrix.LookAtRH( origin,  Vector3.UnitY + origin, Vector3.UnitZ ) * postTransform,
				initialTransform * Matrix.LookAtRH( origin, -Vector3.UnitZ + origin, Vector3.UnitY ) * postTransform,
				initialTransform * Matrix.LookAtRH( origin,  Vector3.UnitZ + origin, Vector3.UnitY ) * postTransform
			};
		}


		/// <summary>
		/// Computes view matricies for each cubemap face in left-handed coordinate system.: Result = initialTransform * LookAt() * postTransform.
		/// </summary>
		/// <param name="origin">Viewpoint origin.</param>
		/// <param name="initialTransform">Initial cubemap transform.</param>
		/// <param name="postTransform">Post transform matrix.</param>
		/// <param name="rh">Is right-handed coodrinate system.</param>
		/// <returns></returns>
		public static Matrix[] ComputeCubemapViewMatriciesLH ( Vector3 origin, Matrix initialTransform, Matrix postTransform )
		{
			return new[] {
				initialTransform * Matrix.LookAtLH( origin,  Vector3.UnitX + origin, Vector3.UnitY ) * postTransform,
				initialTransform * Matrix.LookAtLH( origin, -Vector3.UnitX + origin, Vector3.UnitY ) * postTransform,
				initialTransform * Matrix.LookAtLH( origin,  Vector3.UnitY + origin,-Vector3.UnitZ ) * postTransform,
				initialTransform * Matrix.LookAtLH( origin, -Vector3.UnitY + origin, Vector3.UnitZ ) * postTransform,
				initialTransform * Matrix.LookAtLH( origin,  Vector3.UnitZ + origin, Vector3.UnitY ) * postTransform,
				initialTransform * Matrix.LookAtLH( origin, -Vector3.UnitZ + origin, Vector3.UnitY ) * postTransform
			};
		}


		/// <summary>
		/// Computes projection matrix for cubemap face in right-handed coordinate system.
		/// </summary>
		/// <param name="near">Near plane</param>
		/// <param name="far">Far plane</param>
		/// <returns></returns>
		public static Matrix ComputeCubemapProjectionMatrixRH ( float near, float far, bool rh = false )
		{
			return Matrix.PerspectiveFovRH( MathUtil.PiOverTwo, 1, near, far );
		}


		/// <summary>
		/// Computes projection matrix for cubemap face in left-handed coordinate system.
		/// </summary>
		/// <param name="near">Near plane</param>
		/// <param name="far">Far plane</param>
		/// <returns></returns>
		public static Matrix ComputeCubemapProjectionMatrixLH ( float near, float far, bool rh = false )
		{
			return Matrix.PerspectiveFovLH( MathUtil.PiOverTwo, 1, near, far );
		}
    }
}

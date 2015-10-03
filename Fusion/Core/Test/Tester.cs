using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Fusion.Core.Test {
	public static class Tester {

		/// <summary>
		/// 
		/// </summary>
		/// <param name="expected"></param>
		/// <param name="actual"></param>
		/// <param name="message"></param>
		public static void AreEqual ( object expected, object actual )
		{
			AreEqual( expected, actual, "" );
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="expected"></param>
		/// <param name="actual"></param>
		/// <param name="message"></param>
		public static void AreEqual ( object expected, object actual, string message, params object[] args )
		{
			if (!expected.Equals(actual)) {
				throw new TestException( "Equality failed: " + string.Format( message, args ) );
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="expected"></param>
		/// <param name="actual"></param>
		/// <param name="message"></param>
		public static void AreNotEqual ( object expected, object actual )
		{
			AreNotEqual( expected, actual, "" );
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="expected"></param>
		/// <param name="actual"></param>
		/// <param name="message"></param>
		public static void AreNotEqual ( object expected, object actual, string message, params object[] args )
		{
			if (expected!=actual) {
			} else {
				throw new TestException( string.Format( message, args ) );
			}
		}



		/// <summary>
		/// Runs system self test
		/// </summary>
		public static void Run ()
		{
			Log.Message("Performing self-test:");

			List<MethodInfo> methods = new List<MethodInfo>();

			foreach ( var asmb in AppDomain.CurrentDomain.GetAssemblies() ) {
				
				foreach ( var type in asmb.DefinedTypes ) {
					//Log.Message("  {0}", type.ToString() );


					foreach ( var method in type.GetMethods( BindingFlags.Static|BindingFlags.NonPublic|BindingFlags.Public ) ) {

						if ( method.GetCustomAttribute<TestAttribute>(true) != null ) {
							methods.Add( method );
						}
					}
				}

			}

			int count = 0;

			foreach ( var method in methods ) {
				try {
					method.Invoke(null,null);	
					Log.Message("  Passed: {0}", method.Name );
					count++;
				} catch ( TargetInvocationException e ) {
					Log.Error("  Failed: {0} {1}", method.Name, e.InnerException.Message );
				}
			}

			Log.Message("Passed: {0}/{1}", count, methods.Count );

		}

	}
}

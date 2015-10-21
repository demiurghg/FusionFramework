using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Fusion.Core.Configuration;

namespace Fusion.Core.Shell.Commands {
	
	[Command("set")]
	public class Set : Command {

		[CommandLineParser.Required]
		public string Variable { get; set; }

		[CommandLineParser.Required]
		public string Value { get; set; }


		string oldValue;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		public Set ( Invoker invoker ) : base(invoker)
		{
		}


		/// <summary>
		/// Force game to exit.
		/// </summary>
		public override void Execute ()
		{
			ConfigVariable variable;

			if (!Invoker.Variables.TryGetValue( Variable, out variable )) {
				throw new Exception(string.Format("Variable '{0}' does not exist", Variable) );
			}

			oldValue	= variable.Get();
			variable.Set( Value );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="type"></param>
		/// <returns></returns>
        static object ChangeType(string value, Type type)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(type);

            return converter.ConvertFromInvariantString(value);
        }



		/// <summary>
		/// No rollback.
		/// </summary>
		public override void Rollback ()
		{
			ConfigVariable variable;

			if (!Invoker.Variables.TryGetValue( Variable, out variable )) {
				throw new Exception(string.Format("Variable '{0}' does not exist", Variable) );
			}

			variable.Set( oldValue );
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Fusion.Core.Shell.Commands {
	
	[Command("set")]
	public class Set : Command {

		[CommandLineParser.Required]
		public string Service { get; set; }

		[CommandLineParser.Required]
		public string Variable { get; set; }

		[CommandLineParser.Required]
		public string Value { get; set; }


		object oldValue;


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
			var cfg = Game.GetConfigObjectByServiceName( Service );
			
			var prop = cfg.GetType().GetProperty( Variable );

			if (prop==null) {
				throw new Exception(string.Format("Service '{0}' does not have config variable '{1}'", Service, Variable));
			}

			oldValue	=	prop.GetValue( cfg );

			prop.SetValue( cfg, ChangeType( Value, prop.PropertyType ) );

			Result = prop.GetValue( cfg );
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
			var cfg = Game.GetConfigObjectByServiceName( Service );
			var prop = cfg.GetType().GetProperty( Variable );

			prop.SetValue( cfg, oldValue );
		}
	}
}

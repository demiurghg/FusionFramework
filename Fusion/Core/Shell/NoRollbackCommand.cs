using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Fusion.Engine.Common;

namespace Fusion.Core.Shell {

	/// <summary>
	///	Base class for 'no-rollback-commands'.
	/// </summary>
	public abstract class NoRollbackCommand : Command {

		/// <summary>
		/// 
		/// </summary>
		/// <param name="invoker"></param>
		public NoRollbackCommand ( Invoker invoker ) : base(invoker)
		{
		}

		/// <summary>
		/// Rollback action
		/// </summary>
		public override void Rollback () {}


		/// <summary>
		/// Execution delay in milliseconds.
		/// </summary>
		[CommandLineParser.Ignore()]
		public override bool NoRollback { 
			get { return true; }
			set { }
		}

	}
}

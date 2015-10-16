using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.ComponentModel;

namespace Fusion.Core.Shell
{
    /// <summary>
    /// http://blogs.msdn.com/b/shawnhar/archive/2012/04/20/a-reusable-reflection-based-command-line-parser.aspx
    /// </summary>
    public class CommandLineParserConfiguration
    {
		public char OptionLeadingChar { get; set; }
		public bool ThrowExceptionOnShowError { get; set; }

		public CommandLineParserConfiguration ()
		{
			OptionLeadingChar = '/';
		}
    }
}

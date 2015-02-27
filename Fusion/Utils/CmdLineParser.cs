using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.ComponentModel;

namespace Fusion
{
    // Reusable, reflection based helper for parsing commandline options.
    public class CmdLineParser
    {
        object optionsObject;

        Queue<PropertyInfo> requiredOptions = new Queue<PropertyInfo>();
        Dictionary<string, PropertyInfo> optionalOptions = new Dictionary<string, PropertyInfo>();

        List<string> requiredUsageHelp = new List<string>();
        List<string> optionalUsageHelp = new List<string>();

		bool throwException = false;

        // Constructor.
        public CmdLineParser(object optionsObject, bool throwException = false )
        {
            this.optionsObject	= optionsObject;
			this.throwException	= throwException;

            // Reflect to find what commandline options are available.
            foreach (PropertyInfo field in optionsObject.GetType().GetProperties())
            {
				if ( GetAttribute<IgnoreAttribute>(field) != null ) {
					continue;
				}

                string fieldName = GetOptionName(field);

                if (GetAttribute<RequiredAttribute>(field) != null)
                {
                    // Record a required option.
                    requiredOptions.Enqueue(field);

                    requiredUsageHelp.Add(string.Format("<{0}>", fieldName));
                }
                else
                {
                    // Record an optional option.
                    optionalOptions.Add(fieldName.ToLowerInvariant(), field);

                    if (field.PropertyType == typeof(bool))
                    {
                        optionalUsageHelp.Add(string.Format("/{0}", fieldName));
                    }
                    else
                    {
                        optionalUsageHelp.Add(string.Format("/{0}:value", fieldName));
                    }
                }
            }
        }


		//public HashSet<PropertyInfo> AffectedProperties { get; private set; }


        public bool ParseCommandLine(string[] args)
        {
			//AffectedProperties	=	new HashSet<PropertyInfo>();

            // Parse each argument in turn.
            foreach (string arg in args)
            {
                if (!ParseArgument(arg.Trim()))
                {
                    return false;
                }
            }

            // Make sure we got all the required options.
            PropertyInfo missingRequiredOption = requiredOptions.FirstOrDefault(field => !IsList(field) || GetList(field).Count == 0);

            if (missingRequiredOption != null)
            {
                ShowError("Missing argument '{0}'", GetOptionName(missingRequiredOption));
                return false;
            }

            return true;
        }


        bool ParseArgument(string arg)
        {
            if (arg.StartsWith("/"))
            {
                // Parse an optional argument.
                char[] separators = { ':' };

                string[] split = arg.Substring(1).Split(separators, 2, StringSplitOptions.None);

                string name = split[0];
                string value = (split.Length > 1) ? split[1] : "true";

                PropertyInfo field;

                if (!optionalOptions.TryGetValue(name.ToLowerInvariant(), out field))
                {
                    ShowError("Unknown option '{0}'", name);
                    return false;
                }

                return SetOption(field, value);
            }
            else
            {
                // Parse a required argument.
                if (requiredOptions.Count == 0)
                {
                    ShowError("Too many arguments");
                    return false;
                }

                PropertyInfo field = requiredOptions.Peek();

                if (!IsList(field))
                {
                    requiredOptions.Dequeue();
                }

                return SetOption(field, arg);
            }
        }


		/*public class EventArgs : EventArgs {
			public PropertyInfo AffectedProperty;
		}

		EventHandler<EventArgs> */


        bool SetOption(PropertyInfo field, string value)
        {
            try
            {
                if (IsList(field))
                {
                    // Append this value to a list of options.
                    GetList(field).Add(ChangeType(value, ListElementType(field)));
                }
                else
                {
                    // Set the value of a single option.
                    field.SetValue(optionsObject, ChangeType(value, field.PropertyType));
                }

                return true;
            }
            catch
            {
                ShowError("Invalid value '{0}' for option '{1}'", value, GetOptionName(field));
                return false;
            }
        }


        static object ChangeType(string value, Type type)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(type);

            return converter.ConvertFromInvariantString(value);
        }


        static bool IsList(PropertyInfo field)
        {
            return typeof(IList).IsAssignableFrom(field.PropertyType);
        }


        IList GetList(PropertyInfo field)
        {
            return (IList)field.GetValue(optionsObject);
        }


        static Type ListElementType(PropertyInfo field)
        {
            var interfaces = from i in field.PropertyType.GetInterfaces()
                             where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                             select i;

            return interfaces.First().GetGenericArguments()[0];
        }


        static string GetOptionName(PropertyInfo field)
        {
            var nameAttribute = GetAttribute<NameAttribute>(field);

            if (nameAttribute != null)
            {
                return nameAttribute.Name;
            }
            else
            {
                return field.Name;
            }
        }


        void ShowError(string message, params object[] args)
        {
            string name = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().ProcessName);

            Console.Error.WriteLine(message, args);
            Console.Error.WriteLine();
            Console.Error.WriteLine("Usage: {0} {1}", name, string.Join(" ", requiredUsageHelp));

            if (optionalUsageHelp.Count > 0)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine("Options:");

                foreach (string optional in optionalUsageHelp)
                {
                    Console.Error.WriteLine("    {0}", optional);
                }
            }

			if (throwException) {
				throw new Exception("Failed to parse arguments, see console for details.");
			}
        }


        static T GetAttribute<T>(ICustomAttributeProvider provider) where T : Attribute
        {
            return provider.GetCustomAttributes(typeof(T), false).OfType<T>().FirstOrDefault();
        }


        // Used on optionsObject fields to indicate which options are required.
        [AttributeUsage(AttributeTargets.Property)]
        public sealed class IgnoreAttribute : Attribute
        { 
        }


        // Used on optionsObject fields to indicate which options are required.
        [AttributeUsage(AttributeTargets.Property)]
        public sealed class RequiredAttribute : Attribute
        { 
        }


        // Used on an optionsObject field to rename the corresponding commandline option.
        [AttributeUsage(AttributeTargets.Property)]
        public sealed class NameAttribute : Attribute
        {
            public NameAttribute(string name)
            {
                this.Name = name;
            }

            public string Name { get; private set; }
        }
    }
}

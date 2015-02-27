using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Fusion.Utils;


namespace Fusion.Content {

	public abstract class ContentProcessor {
		
		/// <summary>
		/// Does content processing.
		/// </summary>
		public abstract void	Process ( ContentItem item );

		static Type[] contentProcessorTypes = null;


		/// <summary>
		/// Gathers all content processors in all loaded 
		/// assemblies within current app domain.
		/// </summary>
		/// <returns></returns>
		public static Type[] GatherContentProcessors ()
		{
			if (contentProcessorTypes!=null) {
				return contentProcessorTypes.ToArray();
			}


			contentProcessorTypes = Misc.GetAllSubclassedOf( typeof(ContentProcessor) )
				.Where( t => t.HasAttribute<ContentProcessorAttribute>() )
				.ToArray();

			return contentProcessorTypes;
		}


		/// <summary>
		/// Gets content processor name
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public static string GetName ( Type contentProcessorType ) 
		{
			return	contentProcessorType.GetCustomAttributes( typeof(ContentProcessorAttribute), true )	
										.Select( na => (na as ContentProcessorAttribute).Name )
										.First();
		}



		/// <summary>
		/// 
		/// </summary>
		internal static Type GetProcessorTypeByName ( string name )
		{
			return GatherContentProcessors()
				.Where( cp => GetName( cp ) == name )
				.First();
		}


		/// <summary>
		/// Gets content processor name
		/// </summary>
		/// <returns></returns>
		public string GetName ()
		{
			return GetName(GetType());
		}


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public ContentItem CreateContentItem ( string path )
		{
			return GetType().GetCustomAttribute<ContentProcessorAttribute>().CreateContentItem( path );
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <param name="contentProcessor"></param>
		/// <returns></returns>
		public bool AcceptFileType ( string path )
		{
			return GetType().GetCustomAttribute<ContentProcessorAttribute>().MatchExt( path );
		}
	}



	/// <summary>
	/// Name attribute
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ContentProcessorAttribute : Attribute {

		public readonly string Name;
		public readonly string Extensions;
		public readonly Type ContentItemType;


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name"></param>
		/// <param name="extensions"></param>
		/// <param name="contentItemType"></param>
		public ContentProcessorAttribute ( string name, string extensions, Type contentItemType ) 
		{
			Name = name;
			Extensions = extensions;
			ContentItemType = contentItemType;

			if ( contentItemType.IsSubclassOf( typeof( ContentItem ) )  || contentItemType == typeof(ContentItem) ) {
			} else {
				throw new ContentException(string.Format("Content item type for '{0}' must be a subclass of ContentItem", name));
			}
		}


		public bool MatchExt ( string path )
		{
			var extList = Extensions.Split( new[]{';'}, StringSplitOptions.RemoveEmptyEntries );
						
			foreach ( var ext in extList ) {
				if (Wildcard.IsMatch(ext, path, false)) {
					return true;
				}
			}
			return false;
		}


		public ContentItem CreateContentItem ( string path ) 
		{
			var item = (ContentItem)Activator.CreateInstance( ContentItemType );
			item.Path				=	path;
			item.ContentProcessor	=	Name;
			return item;
		}
	}
}

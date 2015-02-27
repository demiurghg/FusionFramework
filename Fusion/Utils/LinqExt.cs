using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fusion {

	public static class MinMaxSelectorEnumerableExtension {
		
		public static T SelectMaxOrDefault<T>(this IEnumerable<T> list, Func<T, float> selector)
		{
			if (!list.Any()) return default(T);
			return list.Aggregate((acc, next) => (selector(acc) > selector(next)) ? acc : next);
		}


		public static T SelectMinOrDefault<T>(this IEnumerable<T> list, Func<T, float> selector)
		{
			if (!list.Any()) return default(T);
			return list.Aggregate((acc, next) => (selector(acc) < selector(next)) ? acc : next);
		}


		public static IEnumerable<TSource> DistinctBy<TSource, TKey> (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			HashSet<TKey> seenKeys = new HashSet<TKey>();
			foreach (TSource element in source)
			{
				if (seenKeys.Add(keySelector(element)))
				{
					yield return element;
				}
			}
		}
	}
}

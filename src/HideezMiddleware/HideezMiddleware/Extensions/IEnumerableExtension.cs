using System.Collections.Generic;
using System.Linq;

namespace HideezMiddleware
{
    static class IEnumerableExtension
    {
        /// <summary>
        /// Splits collection into a range of sets with specified amout of items per set
        /// </summary>
        /// <param name="itemsPerSet">Maximum amount of items per set</param>
        /// <returns>Returns a new collection of sets with items from source collection</returns>
        public static IEnumerable<IEnumerable<T>> ToSets<T>(this IEnumerable<T> source, int itemsPerSet)
        {
            var sourceList = source as List<T> ?? source.ToList();
            for (var index = 0; index < sourceList.Count; index += itemsPerSet)
            {
                yield return sourceList.Skip(index).Take(itemsPerSet);
            }
        }
    }
}

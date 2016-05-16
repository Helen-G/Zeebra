using System.Collections.Generic;

namespace AFT.RegoV2.Core.Game.Extensions
{
    public static class ListExtensions
    {
        public static T AddAndPass<T>(this IList<T> list, T item)
        {
            list.Add(item);
            return item;
        }
    }
}
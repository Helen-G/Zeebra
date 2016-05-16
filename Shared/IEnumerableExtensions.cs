using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AFT.RegoV2.Shared
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Mimicks 'foreach' statement in a functional manner
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void ForEach<T>(
            this IEnumerable<T> source,
            Action<T> action)
        {
            foreach (T element in source)
                action(element);
        }

        public static Dictionary<TKey, TValue> ToDictionary<T, TKey, TValue>(this IEnumerable<T> list, Func<T, KeyValuePair<TKey, TValue>> map)
        {
            var to = new Dictionary<TKey, TValue>();
            foreach (var item in list)
            {
                var entry = map(item);
                to[entry.Key] = entry.Value;
            }
            return to;
        }

        public static bool AllElementsAreUnique<T>(this IEnumerable<T> list)
        {
            var hs = new HashSet<T>();
            return list.All(hs.Add);
        }

        /// <summary>
        /// Caches data from lazy sequence, so IEnumerable is not getting evaluated multiple times
        /// </summary>
        public static IEnumerable<T> Memoize<T>(this IEnumerable<T> sequence)
        {
            return new MemoizedSequence<T>(sequence);
        }
    }

    internal class MemoizedSequence<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> _source;
        private readonly List<T> _cache;
        private int _lastCachedIndex;
        private IEnumerator<T> _inner;
        private bool _disposed;

        public MemoizedSequence(IEnumerable<T> source)
        {
            _source = source;
            _cache = new List<T>();
            _lastCachedIndex = -1;
        }

        #region Implementation of IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            if (_inner == null)
                _inner = _source.GetEnumerator();

            for (int i = 0; i <= _lastCachedIndex; i++)
            {
                yield return _cache[i];
            }

            while (!_disposed && _inner.MoveNext())
            {
                _cache.Add(_inner.Current);
                _lastCachedIndex++;
                yield return _inner.Current;
            }
            _inner.Dispose();
            _disposed = true;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}

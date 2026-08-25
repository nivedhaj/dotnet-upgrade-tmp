using ct._Core.Middleware;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ct._Core.Util
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class Extensions
    {
        public static bool In<T>(this T value, IEnumerable<T> values)
        {
            if (values == null) return false;
            return values.Contains(value);
        }

        /// <summary>
        /// Auth Middleware Extension
        /// </summary>
        /// <param name="builder"></param>
        public static void Use_AuthMiddleware(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<AuthMiddleware>();
        }
        /// <summary>
        /// Exception Middleare Extension
        /// </summary>
        /// <param name="builder"></param>
        public static void Use_ExceptionHandlerMiddleware(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<ExceptionHandlerMiddleware>();
        }
        /// <summary>
        /// https://stackoverflow.com/questions/9314172/getting-all-messages-from-innerexceptions
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="nextItem"></param>
        /// <param name="canContinue"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> FromHierarchy<TSource>(this TSource source, Func<TSource, TSource> nextItem, Func<TSource, bool> canContinue)
        {
            for (var current = source; canContinue(current); current = nextItem(current))
            {
                yield return current;
            }
        }
        /// <summary>
        /// Inner Exception Hierarchy
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="nextItem"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> FromHierarchy<TSource>(this TSource source, Func<TSource, TSource> nextItem) where TSource : class
        {
            return FromHierarchy(source, nextItem, s => s != null);
        }
        /// <summary>
        /// Build Url extension
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="relativeURL"></param>
        /// <returns></returns>
        public static string Combine(this Uri uri, string relativeURL)
        {
            char[] charsToTrim = { '\\', '/' };
            var url1 = uri.ToString().TrimEnd(charsToTrim);
            var url2 = string.IsNullOrEmpty(relativeURL) ? string.Empty : relativeURL.TrimStart(charsToTrim);
            return string.Format("{0}/{1}", url1, url2);
        }

        /// <summary>
        /// Break a list into chunks of a given size
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static IEnumerable<T[]> Chunk<T>(this IEnumerable<T> input, int size)
        {
            if (input == null) throw new ArgumentNullException("input");
            if (size < 1) throw new ArgumentOutOfRangeException("size");
            using (var iter = input.GetEnumerator())
            {
                var pos = 0;
                while (iter.MoveNext())
                {
                    var remaining = input.Count() - pos;
                    var cSize = remaining <= size ? remaining : size;
                    var chunk = new T[cSize];
                    chunk[0] = iter.Current;
                    for (var i = 1; i < cSize && iter.MoveNext(); i++)
                    {
                        chunk[i] = iter.Current;
                    }
                    pos += chunk.Length;
                    yield return chunk;
                }
            }
        }
        /// <summary>
        /// Distinct By
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
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
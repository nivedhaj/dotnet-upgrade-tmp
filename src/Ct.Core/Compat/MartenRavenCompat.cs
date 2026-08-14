using Marten;
using Marten.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Marten.Compat;

namespace Marten.Compat
{
    /// <summary>
    /// Small compatibility helpers so legacy Raven-style call sites compile against Marten.
    /// Prefer rewriting to plain LINQ + ToList() over time.
    /// </summary>
    public static class SessionExtensions
    {
        /// <summary>No-op identity for old Raven ProjectInto.</summary>
        public static IQueryable<T> ProjectInto<T>(this IQueryable<T> source) => source;

        /// <summary>Approximate Raven Advanced.Stream by materializing the query.</summary>
        public static StreamEnumerator<T> Stream<T>(this IDocumentSession session, IQueryable<T> query)
            => new StreamEnumerator<T>(query.ToList());
    }

    public sealed class StreamEnumerator<T> : IDisposable
    {
        private readonly IEnumerator<StreamResult<T>> _inner;

        public StreamEnumerator(IEnumerable<T> items)
        {
            _inner = items.Select(i => new StreamResult<T> { Document = i }).GetEnumerator();
        }

        public StreamResult<T> Current => _inner.Current;
        public bool MoveNext() => _inner.MoveNext();
        public void Dispose() => _inner.Dispose();
    }

    public sealed class StreamResult<T>
    {
        public T Document { get; set; } = default!;
    }
}

namespace Raven.Client.Documents.Indexes
{
    /// <summary>Stub so leftover index base references compile; not executed.</summary>
    public abstract class AbstractIndexCreationTask { }
    public abstract class AbstractIndexCreationTask<T> : AbstractIndexCreationTask { }
    public abstract class AbstractIndexCreationTask<T, TReduce> : AbstractIndexCreationTask { }
}

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace FlashTeams.Test.Helpers.Stubs;

#pragma warning disable SA1402
public static class AsyncQueryable
{
    public static IQueryable<TEntity> AsAsyncQueryable<TEntity>(this IEnumerable<TEntity> source)
    {
        return new AsyncQueryable<TEntity>(source ?? throw new ArgumentNullException(nameof(source)));
    }
}

public class AsyncQueryable<TEntity> : EnumerableQuery<TEntity>, IAsyncEnumerable<TEntity>, IQueryable<TEntity>
{
    public AsyncQueryable(IEnumerable<TEntity> enumerable)
        : base(enumerable)
    {
    }

    public AsyncQueryable(Expression expression)
        : base(expression)
    {
    }

    IQueryProvider IQueryable.Provider => new AsyncQueryProvider(this);

    public IAsyncEnumerator<TEntity> GetEnumerator()
    {
        return new AsyncEnumerator(this.AsEnumerable().GetEnumerator());
    }

    public IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new AsyncEnumerator(this.AsEnumerable().GetEnumerator());
    }

    internal class AsyncEnumerator(IEnumerator<TEntity> inner) : IAsyncEnumerator<TEntity>
    {
        public TEntity Current => inner.Current;

        public void Dispose() => inner.Dispose();

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(inner.MoveNext());
        }

        public ValueTask DisposeAsync()
        {
            inner.Dispose();
            return ValueTask.CompletedTask;
        }
    }

    internal class AsyncQueryProvider(IQueryProvider inner) : IAsyncQueryProvider
    {
        public IQueryable CreateQuery(Expression expression)
        {
            return new AsyncQueryable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new AsyncQueryable<TElement>(expression);
        }

        public object Execute(Expression expression)
        {
            return inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return inner.Execute<TResult>(expression);
        }

        public static IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
        {
            return new AsyncQueryable<TResult>(expression);
        }

        TResult IAsyncQueryProvider.ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            return Execute<TResult>(expression);
        }
    }
}
namespace SynchroFeed.Library
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client;
    using System.Threading.Tasks;

    /// <summary>
    /// The DataServiceQueryExtensions class is an extensions class that contains async methods that make working with the DataServiceContext and DataServiceQuery&lt;T&gt; easier.
    /// This was copied from https://gist.github.com/tpetrina/2277212.
    /// </summary>
    public static class DataServiceQueryExtensions
    {
        public static Task<IEnumerable<T>> ExecuteAsync<T>(this DataServiceQuery<T> query, object state)
        {
            return Task.Factory.FromAsync<IEnumerable<T>>(query.BeginExecute, query.EndExecute, state);
        }
    }

    public static class DataServiceContextExtensions
    {
        public static Task<IEnumerable<T>> ExecuteAsync<T>(this DataServiceContext context, DataServiceQueryContinuation<T> continuation, object state)
        {
            return Task.Factory.FromAsync<DataServiceQueryContinuation<T>, IEnumerable<T>>(context.BeginExecute<T>, context.EndExecute<T>, continuation, state);
        }

        public static Task<IEnumerable<T>> ExecuteAsync<T>(this DataServiceContext context, Uri requestUri, object state)
        {
            return Task.Factory.FromAsync<Uri, IEnumerable<T>>(context.BeginExecute<T>, context.EndExecute<T>, requestUri, state);
        }

        public static Task<DataServiceResponse> ExecuteBatchAsync(this DataServiceContext context, object state, params DataServiceRequest[] queries)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            return Task.Factory.FromAsync<DataServiceResponse>(context.BeginExecuteBatch(null, state, queries), context.EndExecuteBatch);
        }

        public static Task<DataServiceStreamResponse> GetReadStreamAsync(this DataServiceContext context, object entity, DataServiceRequestArgs args, object state)
        {
            return Task.Factory.FromAsync<object, DataServiceRequestArgs, DataServiceStreamResponse>(context.BeginGetReadStream, context.EndGetReadStream, entity, args, state);
        }

        public static Task<QueryOperationResponse> LoadPropertyAsync(this DataServiceContext context, object entity, string propertyName, object state)
        {
            return Task.Factory.FromAsync<object, string, QueryOperationResponse>(context.BeginLoadProperty, context.EndLoadProperty, entity, propertyName, state);
        }

        public static Task<QueryOperationResponse> LoadPropertyAsync(this DataServiceContext context, object entity, string propertyName, DataServiceQueryContinuation continuation, object state)
        {
            return Task.Factory.FromAsync<object, string, DataServiceQueryContinuation, QueryOperationResponse>(context.BeginLoadProperty, context.EndLoadProperty, entity, propertyName, continuation, state);
        }

        public static Task<QueryOperationResponse> LoadPropertyAsync(this DataServiceContext context, object entity, string propertyName, Uri nextLinkUri, object state)
        {
            return Task.Factory.FromAsync<object, string, Uri, QueryOperationResponse>(context.BeginLoadProperty, context.EndLoadProperty, entity, propertyName, nextLinkUri, state);
        }

        public static Task<DataServiceResponse> SaveChangesAsync(this DataServiceContext context, object state)
        {
            return Task.Factory.FromAsync<DataServiceResponse>(context.BeginSaveChanges, context.EndSaveChanges, state);
        }

        public static Task<DataServiceResponse> SaveChangesAsync(this DataServiceContext context, SaveChangesOptions options, object state)
        {
            return Task.Factory.FromAsync<SaveChangesOptions, DataServiceResponse>(context.BeginSaveChanges, context.EndSaveChanges, options, state);
        }
    }
}

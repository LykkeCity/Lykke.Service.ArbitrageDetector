using System;
using System.Linq;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace Lykke.Service.ArbitrageDetector.Aspects.Cache
{
    public class CacheAsyncInterceptor : IAsyncInterceptor
    {
        private readonly ICacheProvider _cache;

        public CacheAsyncInterceptor(ICacheProvider cache)
        {
            _cache = cache;
        }

        public void InterceptSynchronous(IInvocation invocation)
        {
            // Check that method returns value
            if (invocation.Method.ReturnType == null)
            {
                invocation.Proceed();
                return;
            }

            var cacheAttr = GetCacheResultAttribute(invocation);

            // If no [Cache] attribute on current method
            if (cacheAttr == null)
            {
                invocation.Proceed();
                return;
            }

            var key = GetInvocationSignature(invocation);

            // If exists in cache - return result
            if (_cache.Contains(key))
            {
                invocation.ReturnValue = _cache.Get(key);
                return;
            }

            // Execution
            invocation.Proceed();
            var result = invocation.ReturnValue;

            // Save to cache
            if (result != null)
            {
                _cache.Put(key, result, cacheAttr.Duration);
            }
        }

        public void InterceptAsynchronous(IInvocation invocation)
        {
            throw new InvalidOperationException($"Method {invocation.Method.Name} mark as [Cache] must return Task<T>, not just Task.");
        }

        public void InterceptAsynchronous<TResult>(IInvocation invocation)
        {
            invocation.ReturnValue = InternalInterceptAsynchronous<TResult>(invocation);
        }

        private async Task<TResult> InternalInterceptAsynchronous<TResult>(IInvocation invocation)
        {
            var cacheAttr = GetCacheResultAttribute(invocation);

            // If no [Cache] attribute on current method
            if (cacheAttr == null)
            {
                invocation.Proceed();
                return await (Task<TResult>)invocation.ReturnValue;
            }

            var key = GetInvocationSignature(invocation);

            // If exists in cache - return result
            if (_cache.Contains(key))
            {
                invocation.ReturnValue = _cache.Get(key);
                return await (Task<TResult>)invocation.ReturnValue;
            }

            // Execution
            invocation.Proceed();
            var task = (Task<TResult>)invocation.ReturnValue;
            var result = await task;

            // Save to cache
            if (result != null)
            {
                _cache.Put(key, result, cacheAttr.Duration);
            }

            return result;
        }

        public CacheAttribute GetCacheResultAttribute(IInvocation invocation)
        {
            return Attribute.GetCustomAttribute(
                    invocation.MethodInvocationTarget,
                    typeof(CacheAttribute)
                )
                as CacheAttribute;
        }

        public string GetInvocationSignature(IInvocation invocation)
        {
            return string.Format("{0}-{1}-{2}",
                invocation.TargetType.FullName,
                invocation.Method.Name,
                string.Join("-", invocation.Arguments.Select(a => (a ?? "").ToString()).ToArray())
            );
        }
    }
}

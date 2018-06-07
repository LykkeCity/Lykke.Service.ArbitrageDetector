using System;
using System.Linq;
using Castle.DynamicProxy;

namespace Lykke.Service.ArbitrageDetector.Aspects.Cache
{
    public class CacheInterceptor : IInterceptor
    {
        private readonly ICacheProvider _cache;

        public CacheInterceptor(ICacheProvider cache)
        {
            _cache = cache;
        }

        public void Intercept(IInvocation invocation)
        {
            var cacheAttr = GetCacheResultAttribute(invocation);

            if (cacheAttr == null)
            {
                invocation.Proceed();
                return;
            }

            var key = GetInvocationSignature(invocation);

            if (_cache.Contains(key))
            {
                invocation.ReturnValue = _cache.Get(key);
                return;
            }

            invocation.Proceed();
            var result = invocation.ReturnValue;

            if (result != null)
            {
                _cache.Put(key, result, cacheAttr.Duration);
            }
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

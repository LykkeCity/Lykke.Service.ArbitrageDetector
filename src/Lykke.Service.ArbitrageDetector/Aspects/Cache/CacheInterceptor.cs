using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace Lykke.Service.ArbitrageDetector.Aspects.Cache
{
    /// <summary>
    /// If [Cache] applied on class then intercept every method wich has a result.
    /// </summary>
    public class CacheInterceptor : IInterceptor
    {
        private readonly ICacheProvider _cache;

        public CacheInterceptor(ICacheProvider cache)
        {
            _cache = cache;
        }

        public void Intercept(IInvocation invocation)
        {
            var cacheMethodAttr = GetCacheMethodAttribute(invocation);
            var cacheClassAttr = GetCacheClassAttribute(invocation);
            // Method attribute has more priority than class attribute
            var cacheAttr = cacheMethodAttr ?? cacheClassAttr;

            if (cacheAttr == null)
            {
                invocation.Proceed();
                return;
            }

            if (invocation.Method.ReturnType == typeof(void) || invocation.Method.ReturnType == typeof(Task))
            {
                // If attribute is on a method that doesn't have result value than throw exception
                if (cacheMethodAttr != null)
                    throw new InvalidOperationException($"Method \"{invocation.Method.Name}\" with attribute [{nameof(CacheAttribute)}] must return a value, not 'void' or 'Task'.");

                // Otherwise it's on the class - do not intercept
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
            var method = invocation.MethodInvocationTarget;
            var isAsync = method.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null;
            if (isAsync && typeof(Task).IsAssignableFrom(method.ReturnType))
            {
                invocation.ReturnValue = InterceptAsync((dynamic)invocation.ReturnValue);
                OnFinish();
            }
            else
            {
                OnFinish();
            }

            void OnFinish()
            {
                var result = invocation.ReturnValue;

                if (result != null)
                {
                    _cache.Put(key, result, cacheAttr.Duration);
                }
            }
        }

        private static async Task InterceptAsync(Task task)
        {
            await task.ConfigureAwait(false);
        }

        private static async Task<T> InterceptAsync<T>(Task<T> task)
        {
            var result = await task.ConfigureAwait(false);
            
            return result;
        }

        private static CacheAttribute GetCacheMethodAttribute(IInvocation invocation)
        {
            var classAttr = Attribute.GetCustomAttribute(invocation.MethodInvocationTarget, typeof(CacheAttribute))
                as CacheAttribute;
            return classAttr;
        }

        private static CacheAttribute GetCacheClassAttribute(IInvocation invocation)
        {
            var classAttr = Attribute.GetCustomAttribute(invocation.TargetType, typeof(CacheAttribute))
                as CacheAttribute;
            return classAttr;
        }

        private static string GetInvocationSignature(IInvocation invocation)
        {
            return string.Format("{0}-{1}-{2}",
                invocation.TargetType.FullName,
                invocation.Method.Name,
                string.Join("-", invocation.Arguments.Select(a => (a ?? "").ToString()).ToArray())
            );
        }
    }
}

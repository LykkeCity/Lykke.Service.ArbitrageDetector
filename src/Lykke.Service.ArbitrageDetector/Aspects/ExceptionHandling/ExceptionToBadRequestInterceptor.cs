using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Common.Log;
using Lykke.Service.ArbitrageDetector.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.ArbitrageDetector.Aspects.ExceptionHandling
{
    public class ExceptionToBadRequestInterceptor : IInterceptor
    {
        private readonly ILog _log;

        public ExceptionToBadRequestInterceptor(ILog log)
        {
            _log = log;
        }

        public async void Intercept(IInvocation invocation)
        {
            var cacheMethodAttr = GetExceptionLoggingMethodAttribute(invocation);
            var cacheClassAttr = GetExceptionLoggingClassAttribute(invocation);
            // Method attribute has more priority than class attribute
            var exceptionLoggingAttr = cacheMethodAttr ?? cacheClassAttr;

            if (exceptionLoggingAttr == null)
            {
                invocation.Proceed();
                return;
            }

            var method = invocation.MethodInvocationTarget;

            // Only if method returns IActionResult or Task<IActionResult>
            if (method.ReturnType != typeof(IActionResult) && method.ReturnType != typeof(Task<IActionResult>))
            {
                // If attribute is on a method that doesn't have IActionResult as a result than throw exception
                if (cacheMethodAttr != null)
                    throw new InvalidOperationException($"Method \"{invocation.Method.Name}\" with attribute [{nameof(ExceptionToBadRequestAttribute)}] must return IActionResult or Task<IActionResult>.");

                // Otherwise it's on the class - do not intercept
                invocation.Proceed();
                return;
            }

            try
            {
                invocation.Proceed();
                
                var isAsync = method.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null;
                if (isAsync && typeof(Task).IsAssignableFrom(method.ReturnType))
                {
                    invocation.ReturnValue = InterceptAsync((dynamic)invocation.ReturnValue, invocation);
                }
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(invocation.TargetType.ToString(), invocation.Method.Name, exception);
                invocation.ReturnValue = new BadRequestObjectResult(ErrorResponse.Create(exception.Message));
            }
        }

        private async Task<T> InterceptAsync<T>(Task<T> task, IInvocation invocation) where T: IActionResult
        {
            T result;

            try
            {
                result = await task.ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(invocation.TargetType.ToString(), invocation.Method.Name, exception);
                return (T)(object)new BadRequestObjectResult(ErrorResponse.Create(exception.Message));
            }

            return result;
        }

        private static ExceptionToBadRequestAttribute GetExceptionLoggingMethodAttribute(IInvocation invocation)
        {
            var classAttr = Attribute.GetCustomAttribute(invocation.MethodInvocationTarget, typeof(ExceptionToBadRequestAttribute))
                as ExceptionToBadRequestAttribute;
            return classAttr;
        }

        private static ExceptionToBadRequestAttribute GetExceptionLoggingClassAttribute(IInvocation invocation)
        {
            var classAttr = Attribute.GetCustomAttribute(invocation.TargetType, typeof(ExceptionToBadRequestAttribute))
                as ExceptionToBadRequestAttribute;
            return classAttr;
        }
    }
}

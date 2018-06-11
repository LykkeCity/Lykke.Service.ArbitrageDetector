using System;

namespace Lykke.Service.ArbitrageDetector.Aspects.ExceptionHandling
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class ExceptionToBadRequestAttribute : Attribute
    {
    }
}

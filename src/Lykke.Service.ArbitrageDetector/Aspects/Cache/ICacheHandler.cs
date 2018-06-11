using System;
using System.Threading.Tasks;

namespace Lykke.Service.ArbitrageDetector.Aspects.Cache
{
    public interface ICacheHandler
    {
        Task Handle(Action action);

        Task Handle(Func<Task> function);

        Task<T> Handle<T>(Func<Task<T>> function);
    }
}

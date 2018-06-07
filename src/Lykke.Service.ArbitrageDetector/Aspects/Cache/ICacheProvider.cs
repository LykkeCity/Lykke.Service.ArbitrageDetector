namespace Lykke.Service.ArbitrageDetector.Aspects.Cache
{
    public interface ICacheProvider
    {
        object Get(string key);

        void Put(string key, object value, int duration);

        bool Contains(string key);
    }
}

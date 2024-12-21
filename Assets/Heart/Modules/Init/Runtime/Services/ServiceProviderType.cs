namespace Sisus.Init
{
    internal enum ServiceProviderType
    {
        None = 0,
        ServiceInitializer,
        ServiceInitializerAsync,
        Wrapper,
        Initializer,
        IValueProviderT,
        IValueProviderAsyncT,
        IValueByTypeProvider,
        IValueByTypeProviderAsync,
        IValueProvider,
        IValueProviderAsync
    }
}
using System;
using Zenject;

namespace SST.Configurators.Samples.ZenjectForConfigurators
{
    /// <summary>
    /// <see cref="IHandlerFactory"/> implementation that creates configurator handlers through Zenject's
    /// <see cref="IInstantiator"/>. Because handlers are built by the container, each one can <c>[Inject]</c>
    /// its own dependencies instead of resolving them manually.
    /// </summary>
    public class ZenjectHandlerFactory : IHandlerFactory
    {
        [Inject] private readonly IInstantiator _instantiator;

        public object Create(Type handlerType) => _instantiator.Instantiate(handlerType);
    }
}
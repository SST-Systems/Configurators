using System;
using Zenject;

namespace SST.Configurators.Samples.Zenject
{
    public class ZenjectHandlerFactory : IHandlerFactory
    {
        [Inject] private readonly IInstantiator _instantiator;

        public object Create(Type handlerType) => _instantiator.Instantiate(handlerType);
    }
}
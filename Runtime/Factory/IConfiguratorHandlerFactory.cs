using System;

namespace SST.Configurators
{
    /// <summary>
    /// Constructs handler instances for the managers. Supply a custom implementation to route handler creation
    /// through a dependency-injection container instead of the default reflection-based factory.
    /// </summary>
    public interface IHandlerFactory
    {
        /// <summary>Creates a handler instance of the requested type.</summary>
        /// <param name="handlerType">The concrete handler type to instantiate.</param>
        /// <returns>A new handler instance.</returns>
        object Create(Type handlerType);
    }
}
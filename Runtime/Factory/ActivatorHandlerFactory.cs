using System;

namespace SST.Configurators
{
    /// <summary>
    /// Default <see cref="IHandlerFactory"/> that instantiates handlers via <see cref="Activator.CreateInstance(Type)"/>.
    /// Used when no factory is supplied to a manager.
    /// </summary>
    public class ActivatorHandlerFactory : IHandlerFactory
    {
        /// <summary>Creates a handler using its parameterless constructor.</summary>
        /// <param name="handlerType">The concrete handler type to instantiate.</param>
        /// <returns>A new handler instance.</returns>
        public object Create(Type handlerType) => Activator.CreateInstance(handlerType);
    }
}
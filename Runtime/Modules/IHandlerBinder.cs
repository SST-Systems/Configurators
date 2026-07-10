using System;

namespace SST.Configurators
{
    /// <summary>
    /// Implemented by serialized data objects that delegate to a pooled handler. The manager uses this contract
    /// to attach and detach the handler during resolve and dispose.
    /// </summary>
    public interface IHandlerBinder
    {
        /// <summary>Concrete handler type to obtain from the pool for this data.</summary>
        Type HandlerType { get; }

        /// <summary>Attaches the resolved handler and injects this data into it.</summary>
        /// <param name="handler">The pooled handler instance.</param>
        void BindHandler(object handler);

        /// <summary>Returns the currently bound handler, or <c>null</c> if none.</summary>
        object GetHandler();

        /// <summary>Detaches the current handler and clears its injected data so it can return to the pool.</summary>
        void UnbindHandler();
    }
}
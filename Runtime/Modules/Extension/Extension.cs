using System;

namespace SST.Configurators
{
    /// <summary>
    /// Base class for a synchronous, inline extension that produces a value of type <typeparamref name="T"/>.
    /// Subclass this and implement <see cref="GetValue"/>; for handler/DI-based extensions use
    /// <see cref="ExtensionData{TValue, THandler}"/>.
    /// </summary>
    /// <typeparam name="T">The value type produced by the extension.</typeparam>
    [Serializable]
    public abstract class Extension<T> : IExtension
    {
        /// <summary>Produces the extension's value.</summary>
        /// <returns>The current value.</returns>
        public abstract T GetValue();

        /// <summary>Implicitly converts the extension to its value by calling <see cref="GetValue"/>.</summary>
        /// <param name="extension">The extension to convert.</param>
        public static implicit operator T(Extension<T> extension) => extension.GetValue();
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SST.Configurators.Samples.ConditionsForVisibility
{
    /// <summary>
    /// Tiny static type-keyed registry used instead of a full DI container. <see cref="Bootstrap"/> registers
    /// services here and the rest of the sample retrieves them via <see cref="Get{T}"/>.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();

        // Clears stale registrations before each play session, so repeated Play with Domain Reload disabled
        // (Enter Play Mode Options) doesn't hit "already registered".
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnLoad() => _services.Clear();

        public static void Register<T>(T service)
        {
            Type type = typeof(T);

            if (!_services.TryAdd(type, service))
            {
                throw new InvalidOperationException($"[ServiceLocator] Service type {type.Name} already registered!");
            }
        }

        public static T Get<T>()
        {
            Type type = typeof(T);

            if (!_services.TryGetValue(type, out object service))
            {
                throw new KeyNotFoundException($"[ServiceLocator] Service type {type.Name } not found in the registry!");
            }

            return (T)service;
        }

        public static void Unregister<T>() => _services.Remove(typeof(T));

        public static void Clear() => _services.Clear();
    }
}

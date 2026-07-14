using System;
using System.Collections.Generic;
using UnityEngine;

namespace SST.Configurators.Samples.ModificationsForObject
{
    /// <summary>
    /// Tiny static registry (in place of full DI): maps a type to a single shared service instance.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();
        
        // Clear stale services on play so domain-reload-off runs start fresh.
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
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SST.Configurators.Samples.InstructionsForButton
{
    /// <summary>
    /// Tiny static service registry the samples use instead of a full DI container:
    /// register a service by type, then Get it from anywhere.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();

        // Clear stale services when entering Play Mode (survives domain-reload-disabled editor setups).
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
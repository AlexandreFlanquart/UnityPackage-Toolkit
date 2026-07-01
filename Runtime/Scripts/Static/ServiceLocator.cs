using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    public static class ServiceLocator
    {
        // Cache services by their concrete type so we only perform the expensive scene lookup once per service.
        private static readonly Dictionary<Type, Component> _serviceContainer = new Dictionary<Type, Component>();

        /// <summary>
        /// Find a service/script in current scene and return reference of it. Note: this will also locate inactive services.
        /// </summary>
        /// <typeparam name="T">Type of service to find</typeparam>
        public static T GetService<T>(bool createObjectIfNotFound = false) where T : Component
        {
            return FindService<T>(createObjectIfNotFound);
        }

        /// <summary>
        /// Add a service/script
        /// </summary>
        /// <typeparam name="T">Type of service to add</typeparam>
        /// <param name="go">GameObject of service to add</param>
        /// <param name="replaceExisting">Allows replacing an existing registered service.</param>
        /// <exception cref="InvalidOperationException">A service of this type is already registered and <paramref name="replaceExisting"/> is false.</exception>
        public static void AddService<T>(GameObject go, bool replaceExisting = false) where T : Component
        {
            if (go == null)
            {
                throw new ArgumentNullException(nameof(go));
            }

            var serviceType = typeof(T);
            var component = go.GetComponent<T>();
            if (component == null)
            {
                var message = $"ServiceLocator cannot register type {serviceType.Name}: the provided GameObject '{go.name}' has no such component.";
                MUPLogger.Error(message, go);
                throw new InvalidOperationException(message);
            }

            if (_serviceContainer.TryGetValue(serviceType, out var existing) && existing != null)
            {
                if (!replaceExisting)
                {
                    var message = $"ServiceLocator already contains a reference for type {serviceType.Name}. Pass replaceExisting=true to overwrite.";
                    MUPLogger.Error(message, existing);
                    throw new InvalidOperationException(message);
                }

                MUPLogger.Warning($"ServiceLocator replaced existing service of type {serviceType.Name}.", existing);
            }

            _serviceContainer[serviceType] = component;
        }

        /// <summary>
        /// Removes a registered service when it is no longer available.
        /// </summary>
        /// <typeparam name="T">Type of service to remove</typeparam>
        /// <param name="component">Component instance that should be removed from the locator.</param>
        public static void RemoveService<T>(Component component) where T : Component
        {
            if (component == null)
            {
                return;
            }

            var serviceType = typeof(T);
            if (_serviceContainer.TryGetValue(serviceType, out var existing) && existing == component)
            {
                _serviceContainer.Remove(serviceType);
            }
        }

        /// <summary>
        /// Check if a service is already referenced
        /// </summary>
        /// <returns>If a service is already referenced</returns>
        public static bool Exists<T>() where T : Component
        {
            var serviceType = typeof(T);
            if (!_serviceContainer.TryGetValue(serviceType, out var cached))
            {
                return false;
            }

            if (cached != null)
            {
                return true;
            }

            _serviceContainer.Remove(serviceType);
            MUPLogger.Warning($"ServiceLocator removed stale reference while checking existence for type {serviceType.Name}.");
            return false;
        }

        /// <summary>
        /// Look for a game object with type required. Checks the cache first (clearing a stale/destroyed
        /// entry if found), then falls back to a scene-wide search, optionally auto-creating the service.
        /// </summary>
        /// <typeparam name="T">Type to look for</typeparam>
        /// <param name="createObjectIfNotFound">Either create a gameobject with type if not exist</param>
        private static T FindService<T>(bool createObjectIfNotFound = false) where T : Component
        {
            var serviceType = typeof(T);

            // Check cache - if entry exists but object is invalid, clean it up
            if (_serviceContainer.TryGetValue(serviceType, out var cached))
            {
                if (cached != null)
                {
                    return (T)cached;
                }
                // Invalid entry found, remove it before searching hierarchy
                _serviceContainer.Remove(serviceType);
                MUPLogger.Info($"ServiceLocator removed stale reference of type {serviceType.Name}.");
            }

            // Always search hierarchy if not in cache or cache was invalid
            var located = GameObject.FindAnyObjectByType<T>(FindObjectsInactive.Include);
            if (located != null)
            {
                _serviceContainer[serviceType] = located;
                return located;
            }

            if (!createObjectIfNotFound)
            {
                return null;
            }

            if (serviceType.IsAbstract)
            {
                var message = $"ServiceLocator cannot auto-create abstract service type {serviceType.Name}.";
                MUPLogger.Error(message);
                throw new InvalidOperationException(message);
            }

            var go = new GameObject(serviceType.Name);
            try
            {
                var component = go.AddComponent<T>();
                _serviceContainer[serviceType] = component;
                MUPLogger.Warning($"ServiceLocator auto-created missing service of type {serviceType.Name}.", component);
                return component;
            }
            catch (Exception exception)
            {
                MUPLogger.Error($"ServiceLocator failed to auto-create service of type {serviceType.Name}: {exception.Message}");
                UnityEngine.Object.Destroy(go);
                throw;
            }
        }

        /// <summary>
        /// Clears every cached service reference. Call this on scene unload to prevent stale
        /// references from carrying over into a new scene — this is not done automatically.
        /// </summary>
        public static void Clear()
        {
            if (_serviceContainer.Count == 0)
            {
                return;
            }

            _serviceContainer.Clear();
            MUPLogger.Warning("ServiceLocator cache cleared.");
        }
    }
}

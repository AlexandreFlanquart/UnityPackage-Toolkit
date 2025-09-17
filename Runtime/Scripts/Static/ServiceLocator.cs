using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    public static class ServiceLocator
    {
        // Cache services by their concrete type so we only perform the expensive scene lookup once per service.
        static readonly Dictionary<Type, Component> servicecontainer = new Dictionary<Type, Component>();

        /// <summary>
        /// Find a service/script in current scene and return reference of it. Note: this will also locate inactive services.
        /// </summary>
        /// <typeparam name="T">Type of service to find</typeparam>
        public static T GetService<T>(bool createObjectIfNotFound = false) where T : Component
        {
            var serviceType = typeof(T);

            if (servicecontainer.TryGetValue(serviceType, out var cached))
            {
                if (cached != null)
                {
                    return (T)cached;
                }

                servicecontainer.Remove(serviceType);
                MUPLogger.Info($"ServiceLocator removed stale reference of type {serviceType.Name}.");
            }

            var located = FindService<T>(createObjectIfNotFound);
            if (located != null)
            {
                return located;
            }

            var message = $"ServiceLocator couldn't find service of type {serviceType.Name} (auto-create set to {createObjectIfNotFound}).";
            Debug.LogError(message);
            throw new InvalidOperationException(message);
        }

        /// <summary>
        /// Add a service/script
        /// </summary>
        /// <typeparam name="T">Type of service to add</typeparam>
        /// <param name="go">GameObject of service to add</param>
        /// <param name="replaceExisting">Allows replacing an existing registered service.</param>
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
                Debug.LogError(message, go);
                throw new InvalidOperationException(message);
            }

            if (servicecontainer.TryGetValue(serviceType, out var existing) && existing != null && !replaceExisting)
            {
                var message = $"ServiceLocator already contains a reference for type {serviceType.Name}. Pass replaceExisting=true to overwrite.";
                Debug.LogError(message, existing);
                throw new InvalidOperationException(message);
            }

            if (existing != null && replaceExisting)
            {
                Debug.LogWarning($"ServiceLocator replaced existing service of type {serviceType.Name}.", existing);
            }

            servicecontainer[serviceType] = component;
        }

        /// <summary>
        /// Check if a service is already referenced
        /// </summary>
        /// <returns>If a service is already referenced</returns>
        public static bool Exists<T>() where T : Component
        {
            var serviceType = typeof(T);
            if (!servicecontainer.TryGetValue(serviceType, out var cached))
            {
                return false;
            }

            if (cached != null)
            {
                return true;
            }

            servicecontainer.Remove(serviceType);
            Debug.LogWarning($"ServiceLocator removed stale reference while checking existence for type {serviceType.Name}.");
            return false;
        }

        /// <summary>
        /// Look for a game object with type required
        /// </summary>
        /// <typeparam name="T">Type to look for</typeparam>
        /// <param name="createObjectIfNotFound">Either create a gameobject with type if not exist</param>
        static T FindService<T>(bool createObjectIfNotFound = false) where T : Component
        {
            var serviceType = typeof(T);

            if (servicecontainer.TryGetValue(serviceType, out var cached) && cached != null)
            {
                return (T)cached;
            }

            var located = GameObject.FindAnyObjectByType<T>(FindObjectsInactive.Include);
            if (located != null)
            {
                servicecontainer[serviceType] = located;
                return located;
            }

            if (!createObjectIfNotFound)
            {
                return null;
            }

            if (serviceType.IsAbstract)
            {
                var message = $"ServiceLocator cannot auto-create abstract service type {serviceType.Name}.";
                Debug.LogError(message);
                throw new InvalidOperationException(message);
            }

            var go = new GameObject(serviceType.Name);
            try
            {
                var component = go.AddComponent<T>();
                servicecontainer[serviceType] = component;
                Debug.LogWarning($"ServiceLocator auto-created missing service of type {serviceType.Name}.", component);
                return component;
            }
            catch (Exception exception)
            {
                Debug.LogError($"ServiceLocator failed to auto-create service of type {serviceType.Name}: {exception.Message}");
                UnityEngine.Object.Destroy(go);
                throw;
            }
        }

        public static void Clear()
        {
            if (servicecontainer.Count == 0)
            {
                return;
            }

            servicecontainer.Clear();
            Debug.LogWarning("ServiceLocator cache cleared.");
        }
    }
}

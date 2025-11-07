using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    public static class MUP_ObjectPoolLocator
    {

        private static Dictionary<Type,MUP_ObjectPool> poolDictionnary = new Dictionary<Type,MUP_ObjectPool>();

        public static MUP_ObjectPool Get<T>(/*bool createObjectIfNotFound = false*/) where T: Component
        {
            var serviceType = typeof(T);

            if (poolDictionnary.TryGetValue(serviceType, out var cached))
            {
                if (cached != null)
                {
                    return /*(T)*/cached;
                }

                poolDictionnary.Remove(serviceType);
                MUPLogger.Info($"ServiceLocator removed stale reference of type {serviceType.Name}.");
            }

          /*  var located = FindService<T>(createObjectIfNotFound);
            if (located != null)
            {
                return located;
            }
            */
            var message = $"ServiceLocator couldn't find service of type {serviceType.Name}).";
            MUPLogger.Error(message);
            throw new InvalidOperationException(message);
        }
        public static void Add<T>(/*GameObject go*/MUP_ObjectPool objectPool, bool replaceExisting = false) where T : Component
        {
            if (objectPool == null)
            {
                throw new ArgumentNullException(nameof(objectPool));
            }

            var serviceType = typeof(T);
            //var component = objectPool.GetComponent<T>();
            /*if (component == null)
            {
                var message = $"ServiceLocator cannot register type {serviceType.Name}: the provided GameObject '{go.name}' has no such component.";
                MUPLogger.Error(message, go);
                throw new InvalidOperationException(message);
            }*/
            MUP_ObjectPool existing;
            if (poolDictionnary.TryGetValue(serviceType, out existing) && existing != null && !replaceExisting)
            {
                var message = $"ServiceLocator already contains a reference for type {serviceType.Name}. Pass replaceExisting=true to overwrite.";
               // MUPLogger.Error(message, existing);
                throw new InvalidOperationException(message);
            }

            if (existing != null && replaceExisting)
            {
                //MUPLogger.Warning($"ServiceLocator replaced existing service of type {serviceType.Name}.", existing);
            }

            poolDictionnary[serviceType] = objectPool;
        }

        public static void Remove<T>(MUP_ObjectPool objectPool) where T : Component
        {
            if (objectPool == null)
            {
                return;
            }

            var serviceType = typeof(T);
            if (poolDictionnary.TryGetValue(serviceType, out var existing) && existing == objectPool)
            {
                poolDictionnary.Remove(serviceType);
            }
        }

        public static void Clear()
        {
            if (poolDictionnary.Count == 0)
            {
                return;
            }

            poolDictionnary.Clear();
            MUPLogger.Warning("ServiceLocator cache cleared.");
        }
    }
}


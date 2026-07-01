using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Registry of <see cref="MUP_ObjectPool{T}"/> instances keyed by their pooled <see cref="Type"/>,
    /// so any script can retrieve the shared pool for a given component type without holding a direct reference.
    /// </summary>
    public static class MUP_ObjectPoolLocator
    {
        private static readonly Dictionary<Type, IMUPObjectPool> poolDictionary = new Dictionary<Type, IMUPObjectPool>();

        /// <summary>Retrieves the pool registered for <typeparamref name="T"/>.</summary>
        /// <exception cref="InvalidOperationException">No pool of this type was registered via <see cref="Add{T}"/>.</exception>
        public static MUP_ObjectPool<T> Get<T>() where T : Component
        {
            var poolType = typeof(T);

            if (poolDictionary.TryGetValue(poolType, out var cached))
            {
                return (MUP_ObjectPool<T>)cached;
            }

            var message = $"MUP_ObjectPoolLocator: no pool registered for type {poolType.Name}.";
            MUPLogger.Error(message);
            throw new InvalidOperationException(message);
        }

        /// <summary>Registers <paramref name="objectPool"/> under type <typeparamref name="T"/>.</summary>
        /// <param name="replaceExisting">If true, replaces an already-registered pool of the same type instead of throwing.</param>
        public static void Add<T>(MUP_ObjectPool<T> objectPool, bool replaceExisting = false) where T : Component
        {
            if (objectPool == null)
            {
                throw new ArgumentNullException(nameof(objectPool));
            }

            var poolType = typeof(T);
            if (poolDictionary.TryGetValue(poolType, out var existing) && existing != null)
            {
                if (!replaceExisting)
                {
                    var message = $"MUP_ObjectPoolLocator already contains a pool for type {poolType.Name}. Pass replaceExisting=true to overwrite.";
                    MUPLogger.Error(message);
                    throw new InvalidOperationException(message);
                }

                MUPLogger.Warning($"MUP_ObjectPoolLocator: replaced existing pool of type {poolType.Name}.");
            }

            poolDictionary[poolType] = objectPool;
            MUPLogger.Info($"MUP_ObjectPoolLocator: registered pool for type {poolType.Name}.");
        }

        /// <summary>Unregisters <paramref name="objectPool"/> if it's the instance currently registered for <typeparamref name="T"/>.</summary>
        public static void Remove<T>(MUP_ObjectPool<T> objectPool) where T : Component
        {
            if (objectPool == null)
            {
                return;
            }

            var poolType = typeof(T);
            if (poolDictionary.TryGetValue(poolType, out var existing) && ReferenceEquals(existing, objectPool))
            {
                poolDictionary.Remove(poolType);
                MUPLogger.Info($"MUP_ObjectPoolLocator: unregistered pool for type {poolType.Name}.");
            }
        }

        /// <summary>Whether a pool is currently registered for <typeparamref name="T"/>.</summary>
        public static bool Exists<T>() where T : Component => poolDictionary.ContainsKey(typeof(T));

        /// <summary>Clears every registered pool (destroying their idle pooled instances) and empties the registry.</summary>
        public static void Clear()
        {
            if (poolDictionary.Count == 0)
            {
                return;
            }

            foreach (var pool in poolDictionary.Values)
            {
                pool.Clear();
            }

            poolDictionary.Clear();
            MUPLogger.Warning("MUP_ObjectPoolLocator: cleared.");
        }
    }
}

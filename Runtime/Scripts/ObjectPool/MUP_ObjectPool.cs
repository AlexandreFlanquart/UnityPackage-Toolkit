using System;
using UnityEngine;
using UnityEngine.Pool;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Generic Unity object pool for any <see cref="Component"/> type. Works for a custom script
    /// (e.g. <c>MUP_ObjectPool&lt;Bullet&gt;</c>) or, for a prefab with no distinguishing script,
    /// by pooling its <see cref="Transform"/> (every GameObject has one).
    /// </summary>
    /// <typeparam name="T">Component type present on the pooled prefab's root GameObject.</typeparam>
    public class MUP_ObjectPool<T> : IMUPObjectPool where T : Component
    {
        private Transform parent;
        private T prefab;
        private int maxSizeActive; // max active instances; 0 or less means no limit

        /// <summary>The underlying Unity <see cref="ObjectPool{T}"/>. Prefer <see cref="Get"/>/<see cref="Release"/> below.</summary>
        public ObjectPool<T> ObjectPool_ { get; private set; }
        /// <summary>Invoked with the instance right after it's fetched (and re-activated) via <see cref="Get"/>.</summary>
        public Action<T> OnGetPoolAction;
        /// <summary>Invoked with the instance right before it's deactivated via <see cref="Release"/>.</summary>
        public Action<T> OnReturnPoolAction;
        /// <summary>Invoked with the instance right before it's destroyed (pool over capacity, or <see cref="Clear"/>).</summary>
        public Action<T> OnDestroyPoolAction;

        /// <summary>True once <see cref="Initialize"/> has been called successfully.</summary>
        public bool IsInitialized => ObjectPool_ != null;
        /// <summary>Number of instances currently checked out via <see cref="Get"/>.</summary>
        public int CountActive => ObjectPool_?.CountActive ?? 0;
        /// <summary>Number of instances currently sitting idle in the pool.</summary>
        public int CountInactive => ObjectPool_?.CountInactive ?? 0;
        /// <summary>Total instances tracked by the pool (active + inactive).</summary>
        public int CountAll => ObjectPool_?.CountAll ?? 0;

        /// <summary>
        /// Initializes the pool. Instances are still created lazily — the first <see cref="Get"/> calls
        /// instantiate on demand; nothing is spawned by this method itself.
        /// </summary>
        /// <param name="baseSizeQueue">Initial capacity hint for the internal idle-instance collection (not a proactive pre-warm).</param>
        /// <param name="maxSizeQueue">Maximum idle instances kept; excess released instances are destroyed instead of queued.</param>
        /// <param name="maxSizeActive">Maximum instances that can be active (checked out) at once. 0 or less means no limit.</param>
        /// <param name="parent">Parent transform new instances are spawned under.</param>
        /// <param name="prefab">Prefab to instantiate; must carry a <typeparamref name="T"/> component.</param>
        /// <param name="collectionCheck">If true (default), releasing an instance that's already idle logs an error instead of silently corrupting the pool.</param>
        public void Initialize(int baseSizeQueue, int maxSizeQueue, int maxSizeActive, Transform parent, T prefab, bool collectionCheck = true)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            if (baseSizeQueue < 0)
                throw new ArgumentException("Base size must be positive", nameof(baseSizeQueue));
            if (maxSizeQueue < baseSizeQueue)
                throw new ArgumentException("Max size must be greater than or equal to base size", nameof(maxSizeQueue));

            ObjectPool_ = new ObjectPool<T>(
                AddObjectToPool,
                OnGetPool,
                OnReleasePool,
                OnDestroyPool,
                collectionCheck,
                baseSizeQueue,
                maxSizeQueue);

            this.parent = parent;
            this.prefab = prefab;
            this.maxSizeActive = maxSizeActive;

            MUPLogger.Info($"MUP_ObjectPool<{typeof(T).Name}>: initialized (base={baseSizeQueue}, max={maxSizeQueue}, maxActive={maxSizeActive}).");
        }

        /// <summary>Fetches an instance from the pool, or <c>null</c> if <see cref="maxSizeActive"/> is already reached.</summary>
        public T Get()
        {
            if (ObjectPool_ == null)
                throw new InvalidOperationException("Object pool not initialized");

            if (maxSizeActive > 0 && ObjectPool_.CountActive >= maxSizeActive)
            {
                MUPLogger.Warning($"MUP_ObjectPool<{typeof(T).Name}>: Get() rejected, {maxSizeActive} active instances already reached.");
                return null;
            }

            return ObjectPool_.Get();
        }

        /// <summary>Returns <paramref name="obj"/> to the pool. No-op if <c>null</c>.</summary>
        public void Release(T obj)
        {
            if (obj == null)
                return;

            if (ObjectPool_ == null)
                throw new InvalidOperationException("Object pool not initialized");

            ObjectPool_.Release(obj);
        }

        private void OnGetPool(T obj)
        {
            OnGetPoolAction?.Invoke(obj);
            obj.gameObject.SetActive(true);
        }

        private void OnReleasePool(T obj)
        {
            OnReturnPoolAction?.Invoke(obj);
            obj.gameObject.SetActive(false);
        }

        private void OnDestroyPool(T obj)
        {
            OnDestroyPoolAction?.Invoke(obj);
            GameObject.Destroy(obj.gameObject);
        }

        private T AddObjectToPool()
        {
            T instance = GameObject.Instantiate(prefab, parent);
            MUPLogger.Info($"MUP_ObjectPool<{typeof(T).Name}>: instantiated a new pooled instance ({ObjectPool_.CountAll + 1} total).", instance);
            return instance;
        }

        /// <summary>Destroys every idle pooled instance and empties the queue. Active (checked-out) instances are unaffected.</summary>
        public void Clear()
        {
            if (ObjectPool_ == null) return;

            ObjectPool_.Clear();
            MUPLogger.Info($"MUP_ObjectPool<{typeof(T).Name}>: cleared.");
        }
    }
}

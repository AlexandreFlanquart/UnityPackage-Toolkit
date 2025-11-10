using System;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// A generic object pool implementation for Unity GameObjects
/// </summary>
namespace MyUnityPackage.Toolkit
{
    public class MUP_ObjectPool
    {
        private Transform parent;
        private GameObject prefab;
        public ObjectPool<GameObject> ObjectPool_ { get; private set; }
        public Action OnGetPoolAction;
        public Action OnReturnPoolAction;
        public Action OnDestroyPoolAction;
        private int maxSizeActive = 0;
        /// <summary>
        /// Initialize the objectpool
        /// </summary>
        /// <param name="baseSizeQueue"></param>
        /// <param name="maxSizeQueue"></param>
        /// <param name="maxSizeActive"></param>
        /// <param name="parent"></param>
        /// <param name="prefab"></param>
        public void Initialize(int baseSizeQueue, int maxSizeQueue, int maxSizeActive, Transform parent, GameObject prefab)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            if (baseSizeQueue < 0)
                throw new ArgumentException("Base size must be positive", nameof(baseSizeQueue));
            if (maxSizeQueue < baseSizeQueue)
                throw new ArgumentException("Max size must be greater than base size", nameof(maxSizeQueue));
            
            ObjectPool_ = new ObjectPool<GameObject>(
                AddObjectToPool, 
                OnGetPool,
                OnReleasePool, 
                OnDestroyPool, 
                false, 
                baseSizeQueue,
                maxSizeQueue);
            
            this.parent = parent;
            this.prefab = prefab;
            this.maxSizeActive = maxSizeActive;

            MUPLogger.Info("Init object pool ");
        }
        /// <summary>
        /// Return the object get by the object pool, return null if we exceed the capacity
        /// </summary>
        /// <returns></returns>
        public GameObject Get()
        {
            if (ObjectPool_ == null)
                throw new InvalidOperationException("Object pool not initialized");
        
            if(maxSizeActive >= 0 && ObjectPool_.CountActive >= maxSizeActive)
                return null;
            return ObjectPool_.Get();
        }
        /// <summary>
        /// Release the object given in argument
        /// </summary>
        /// <param name="obj"></param>
        public void Release(GameObject obj)
        {
            if (obj == null)
                return;
        
            if (ObjectPool_ == null)
                throw new InvalidOperationException("Object pool not initialized");
        
            ObjectPool_.Release(obj);
        }

        private void OnGetPool(GameObject obj)
        {
            OnGetPoolAction?.Invoke();
            obj.SetActive(true);
        }

        private void OnReleasePool(GameObject obj)
        {
            OnReturnPoolAction?.Invoke();
            obj.SetActive(false);
        }

        private void OnDestroyPool(GameObject obj)
        {
            OnDestroyPoolAction?.Invoke();
            GameObject.Destroy(obj);
   
        }

        private GameObject AddObjectToPool()
        {
            MUPLogger.Info("Add object to the pool");
            GameObject go = GameObject.Instantiate(prefab, parent);
            
            return go;
        }

        public void Clear()
        {
            if (ObjectPool_ != null)
            {
                ObjectPool_.Clear();
                MUPLogger.Info("Cleared object pool");
            }
        }
    }
}

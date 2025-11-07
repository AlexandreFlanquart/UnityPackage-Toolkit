using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;


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
        public void Initialize(int baseSizeQueue, int maxSizeQueue,int maxSizeActive, Transform parent, GameObject prefab)
        {

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
    }
}

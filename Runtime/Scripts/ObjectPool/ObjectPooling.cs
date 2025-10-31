using System.Collections.Generic;
using UnityEngine;


namespace MyUnityPackage.Toolkit
{
    public class ObjectPooling : MonoBehaviour
    {
        [SerializeField] GameObject parent;
        [SerializeField] GameObject prefab;
        [SerializeField] int baseSizeQueue;
        Queue<GameObject> toPoolQueue = new Queue<GameObject>();

        void Start()
        {
            for(int i = 0;i<baseSizeQueue;i++)
            {
                AddObjectToPool();
            }
        }
        public GameObject GetObject()
        {
            GameObject go;
            if(toPoolQueue.Count > 0)
            {
                go = toPoolQueue.Dequeue();
             
            }
            else
            {
                go = AddObjectToPool();
            }
            go.SetActive(true);
            return go;
        }
        public void ReturnObject(GameObject obj)
        {
            obj.SetActive(false);
            toPoolQueue.Enqueue(obj);
        }
        public GameObject AddObjectToPool()
        {
            GameObject go = Instantiate(prefab,parent.transform);
            go.SetActive(false);
            toPoolQueue.Enqueue(go);
            return go;
        }
    }

}

using MyUnityPackage.Toolkit;
using UnityEngine;

public class GameManagerObjectPool : MonoBehaviour
{
    MUP_ObjectPool<Bullett> objectPooling;
    MUP_ObjectPool<Transform> objectPooling2;

    [SerializeField] GameObject parent;
    [SerializeField] GameObject InactiveParent;
    [SerializeField] GameObject prefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitObjectPool1();
        InitObjectPool2();
    }

    void InitObjectPool1()
    {
        objectPooling = new MUP_ObjectPool<Bullett>();
        MUP_ObjectPoolLocator.Add<Bullett>(objectPooling);
        objectPooling.Initialize(10, 20, 20, parent.transform, prefab.GetComponent<Bullett>());
        objectPooling.OnGetPoolAction += bullet => MUPLogger.Info($"Get {bullet.name} from the pool");
        objectPooling.OnReturnPoolAction += bullet => MUPLogger.Info($"Release {bullet.name} to the pool");
        objectPooling.OnDestroyPoolAction += bullet => MUPLogger.Info($"Destroy {bullet.name} from the pool");
    }

    public void AddToPool()
    {
        Bullett bullet = objectPooling.Get();
        bullet?.transform.SetParent(parent.transform);
    }

    public void RemoveToPool()
    {
        int index = parent.transform.childCount - 1;
        if (index < 0)
            return;

        GameObject go = parent.transform.GetChild(index).gameObject;
        go.transform.SetParent(InactiveParent.transform);
        objectPooling.Release(go.GetComponent<Bullett>());
    }

    // Demonstrates pooling a prefab with no distinguishing script by pooling its Transform directly —
    // MUP_ObjectPool<T> works for any Component, not just custom scripts like Bullett.
    void InitObjectPool2()
    {
        objectPooling2 = new MUP_ObjectPool<Transform>();
        objectPooling2.Initialize(10, 10, 20, transform, ((GameObject)Resources.Load("Cube")).transform);
    }
}

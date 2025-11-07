using System.Collections;
using MyUnityPackage.Toolkit;
using UnityEngine;

public class GameManagerObjectPool : MonoBehaviour
{
    MUP_ObjectPool objectPooling ;
    MUP_ObjectPool objectPooling2 ;

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
        objectPooling = new MUP_ObjectPool();
        MUP_ObjectPoolLocator.Add<Bullett>(objectPooling);
        objectPooling.Initialize(10, 2,20, parent.transform,prefab);
        objectPooling.OnGetPoolAction += delegate {MUPLogger.Info("Je get depuis l'action");};
        objectPooling.OnReturnPoolAction += delegate {MUPLogger.Info("Je release depuis l'action");};
        objectPooling.OnReturnPoolAction += delegate {MUPLogger.Info("Je destroy depuis l'action");};
    }
    public void AddToPool()
    {
        GameObject go = objectPooling.Get();
        go.transform.SetParent(parent.transform);
    }
    public void RemoveToPool()
    {
        int index = parent.transform.childCount-1;
        if(index <0)
            return;
        GameObject go = parent.transform.GetChild(index).gameObject;
        go.transform.SetParent(InactiveParent.transform);
        objectPooling.Release(go);
        

    }
    void InitObjectPool2()
    {
        objectPooling2 = new MUP_ObjectPool();   
        objectPooling2.Initialize(10, 10, 20,transform,(GameObject)Resources.Load("Cube")) ;
    }
}

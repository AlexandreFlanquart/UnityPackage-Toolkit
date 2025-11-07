# ObjectPool

Gestionnaire d'objet ayant pour but de réutiliser les objet creer afin d'éviter d'économiser du temps

## Utilisation

Instantier et initialiser un nouveau object pooling
```c#
MUP_ObjectPool objectPooling = new MUP_ObjectPool();
objectPooling.Initialize(10, 2,20, parent.transform,prefab);
```

Recuperer un objet 
```c#
GameObject go = objectPooling.Get();
```

Release un objet 
```c#
objectPooling.Release(gameobject);
```
# ObjectPoolLocator

Manager ayant pour but de stocker tout les object pool créer et de pouvoir les récuperer n'importe ou dans le code, se basant sur le principe d'un ServiceLocator.


Recuperer l'objet :
Permet de récuperer un objet stocker dans le poolLocator.
```c#
MUP_ObjectPool poolBulett = MUP_ObjectPoolLocator.Get<Bulett>();
```


Ajouter l'objet:
Permet d'ajouter un **MUP_PoolObject** dans le poolLocator.
```c#
MUP_ObjectPoolLocator.Get<Bulett>(pool);
```

Enlever l'objet:
Permet de retirer un **MUP_PoolObject** du poolLocator.
```c#
MUP_ObjectPoolLocator.Remove<Bulett>(pool);
```


Nettoyer l'objet:
Permet de vider le poolLocator
```c#
MUP_ObjectPoolLocator.Clear();
```
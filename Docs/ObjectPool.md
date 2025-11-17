# ObjectPool

Gestionnaire d'objets ayant pour but de réutiliser les objets créés afin d'économiser du temps.

## Utilisation

Instancier et initialiser un nouveau pool d'objets
```c#
MUP_ObjectPool objectPooling = new MUP_ObjectPool();
objectPooling.Initialize(10, 20, 20, parent.transform, prefab);
```

Récupérer un objet 
```c#
GameObject go = objectPooling.Get();
```

Libérer un objet 
```c#
objectPooling.Release(gameObject);
```
# ObjectPoolLocator

Manager ayant pour but de stocker tous les MUP_ObjectPool créés et de pouvoir les récupérer n'importe où dans le code, en se basant sur le principe d'un ServiceLocator.


Récupérer l'objet :
Permet de récupérer un **MUP_PoolObject** de type "Bullet" stocké dans le poolLocator.
```c#
MUP_ObjectPool poolBullet = MUP_ObjectPoolLocator.Get<Bullet>();
```


Ajouter l'objet:
Permet d'ajouter un **MUP_PoolObject** de type "Bullet" dans le poolLocator.
```c#
MUP_ObjectPoolLocator.Add<Bullet>(pool);
```

Retirer l'objet:
Permet de retirer un **MUP_PoolObject** de type "Bullet" du poolLocator.
```c#
MUP_ObjectPoolLocator.Remove<Bullet>(pool);
```


Nettoyer le PoolLocator :
Permet de vider le poolLocator.
```c#
MUP_ObjectPoolLocator.Clear();
```
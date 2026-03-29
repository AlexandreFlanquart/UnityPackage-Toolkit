# ObjectPool

Object manager aimed at reusing created objects to save time.

## Usage

### Create a pool
Instantiate and initialize a new object pool:
```c#
MUP_ObjectPool objectPooling = new MUP_ObjectPool();
objectPooling.Initialize(10, 20, 20, parent.transform, prefab);
```

### Get an object
```c#
GameObject go = objectPooling.Get();
```

### Release an object
```c#
objectPooling.Release(gameObject);
```

## Best practices
- Always `Release()` objects back to the pool instead of `Destroy()` to keep allocations stable.
- Ensure pooled objects reset their state (position, rotation, enabled components, particles, etc.) when reused.
- Prefer pooling for frequently spawned/despawned objects (bullets, VFX, decals, UI popups).

# ObjectPoolLocator

Manager that stores all created `MUP_ObjectPool` instances and lets you access them anywhere in code, based on the ServiceLocator principle.


Retrieve an object:
Gets a **MUP_PoolObject** of type "Bullet" stored in the pool locator.
```c#
MUP_ObjectPool poolBullet = MUP_ObjectPoolLocator.Get<Bullet>();
```


Add an object:
Adds a **MUP_PoolObject** of type "Bullet" into the pool locator.
```c#
MUP_ObjectPoolLocator.Add<Bullet>(pool);
```

Remove an object:
Removes a **MUP_PoolObject** of type "Bullet" from the pool locator.
```c#
MUP_ObjectPoolLocator.Remove<Bullet>(pool);
```


Clear the PoolLocator:
Empties the pool locator.
```c#
MUP_ObjectPoolLocator.Clear();
```
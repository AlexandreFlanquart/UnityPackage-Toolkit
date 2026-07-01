# ObjectPool

Object manager aimed at reusing created objects to save time. Generic over any `Component` type — use a
custom script (e.g. `Bullet`) as the type parameter, or `Transform` to pool a plain prefab with no
distinguishing script (every GameObject has one).

## Usage

### Create a pool
Instantiate and initialize a new object pool:
```c#
MUP_ObjectPool<Bullet> objectPooling = new MUP_ObjectPool<Bullet>();
objectPooling.Initialize(10, 20, 20, parent.transform, prefab.GetComponent<Bullet>());
```
`baseSizeQueue` is only an initial capacity hint — instances are still created lazily on the first `Get()`
calls, nothing is spawned by `Initialize()` itself. `maxSizeActive` caps how many instances can be checked
out at once (0 or less means no limit).

### Get an object
```c#
Bullet bullet = objectPooling.Get(); // null if maxSizeActive is already reached
```

### Release an object
```c#
objectPooling.Release(bullet);
```

### React to pool events
```c#
objectPooling.OnGetPoolAction += b => b.ResetState();
objectPooling.OnReturnPoolAction += b => b.PlayDespawnVfx();
```

### Pool a prefab with no custom script
```c#
MUP_ObjectPool<Transform> cubes = new MUP_ObjectPool<Transform>();
cubes.Initialize(10, 10, 20, parent.transform, cubePrefab.transform);
```

## Best practices
- Always `Release()` objects back to the pool instead of `Destroy()` to keep allocations stable.
- Ensure pooled objects reset their own state (position, rotation, enabled components, particles, etc.) when reused — do this in `OnGetPoolAction`, not by relying on `Awake`/`OnEnable` (pooled instances only run those once).
- Prefer pooling for frequently spawned/despawned objects (bullets, VFX, decals, UI popups).
- Keep `collectionCheck` enabled (default) during development — it catches double-`Release()` bugs instead of silently corrupting the pool.

# ObjectPoolLocator

Static registry that stores `MUP_ObjectPool<T>` instances keyed by their pooled component type `T`, so any
script can reach a pool without holding a direct reference — based on the `ServiceLocator` principle.

Add a pool:
```c#
MUP_ObjectPoolLocator.Add<Bullet>(pool);
```

Retrieve it elsewhere:
```c#
MUP_ObjectPool<Bullet> pool = MUP_ObjectPoolLocator.Get<Bullet>();
```

Check / remove:
```c#
bool hasPool = MUP_ObjectPoolLocator.Exists<Bullet>();
MUP_ObjectPoolLocator.Remove<Bullet>(pool);
```

Clear every registered pool (destroys their idle instances too):
```c#
MUP_ObjectPoolLocator.Clear();
```

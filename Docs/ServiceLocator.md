# ServiceLocator

Service manager for Unity. Automatically finds components in the scene and caches them.

## When to use it
- When you want a lightweight way to access scene services without wiring references everywhere.
- When you want to avoid repeated `FindObjectOfType` calls (results are cached).

## Usage

### Get a service (recommended)
```C#
void Start()
{
    // Automatically finds the service in the scene
    MyClass instance = ServiceLocator.GetService<MyClass>();
    
    // Automatically create if not found
    MyClass instance = ServiceLocator.GetService<MyClass>(createObjectIfNotFound: true);
}
```

### Register manually (optional)
```C#
void Awake()
{
    // Register manually (for optimization or control)
    ServiceLocator.AddService<MyClass>(gameObject);
    
    // Replace an existing service
    ServiceLocator.AddService<MyClass>(gameObject, replaceExisting: true);
}
```

### Check existence
```C#
if (ServiceLocator.Exists<MyClass>())
{
    // The service exists
}
```

### Clear the cache
```C#
ServiceLocator.Clear(); // Removes all services from the cache
```

## Behavior

- **Automatic search**: `GetService<T>()` automatically finds the component in the scene
- **Smart cache**: found services are cached to avoid expensive searches
- **Inactive objects**: also finds inactive objects (`FindObjectsInactive.Include`)
- **Optional registration**: `AddService()` is useful for optimization or explicit control

## Notes
- If you change scenes, cached references may become invalid depending on your lifecycle; call `ServiceLocator.Clear()` when appropriate.
- If you have multiple instances of the same service type, prefer explicit `AddService()` to control which one is returned.

## Methods

```C#
ServiceLocator.AddService<T>(GameObject, replaceExisting?)
ServiceLocator.GetService<T>(createObjectIfNotFound?)
ServiceLocator.Exists<T>()
ServiceLocator.Clear()
```
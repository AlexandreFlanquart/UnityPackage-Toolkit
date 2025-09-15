- Add a class
```C#
void Start()
{
   ServiceLocator.AddService<MyClass>(gameobject);
}

```
- Get a class
```C#
void Start()
{
   Myclass instance = ServiceLocator.GetService<MyClass>();
}
```
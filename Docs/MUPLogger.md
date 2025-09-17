- Some exemples
```C#
void Start()
{
   //will log only in the editor
   MUPLogger.InfoEditor("Editor");
   //Will log in editor and builds
   MUPLogger.Info("Anywhere");
}

```
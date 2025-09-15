- Some exemples
```C#
void Start()
{
   //will log only in the editor
   MUPLogger.LogMessageEditor("Editor");
   //Will log in editor and builds
   MUPLogger.LogMessage("Anywhere");
}

```
# MUPLogger

Simple logger for Unity with editor-log support.

## Configuration

```C#
MUPLogger.Enabled = true;
MUPLogger.GlobalPrefix = "[MyGame]";
MUPLogger.IncludeTimestamp = true;
MUPLogger.MinimumLevel = MUPLogger.LogLevel.Info;
```

## Recommended defaults
- Use `MinimumLevel = Info` in production builds.
- Use `MinimumLevel = Debug` during development (or for specific systems).

## Log Levels 

- Debug 
- Info
- Warning 
- Error 
- Exception

## Usage

```C#
// Standard logs - appear everywhere
MUPLogger.Info("Game initialized");
MUPLogger.Warning("Warning");
MUPLogger.Error("Error");

// Editor-only logs - shown only in the editor
MUPLogger.Info("Debug info", editorOnly: true);
MUPLogger.LogDebug("Player position", editorOnly: true);

// With Unity context (click directly to the object)
MUPLogger.Info("Player initialized", this);

// Exception handling
try
{
    // Code that can throw
}
catch (Exception ex)
{
    MUPLogger.Exception(ex, this, editorOnly: true); // Detailed stack trace
    MUPLogger.Error("Loading error"); // Simple message for the user
}
```

## Methods

```C#
MUPLogger.LogDebug(message, context?, editorOnly?)
MUPLogger.Info(message, context?, editorOnly?)
MUPLogger.Warning(message, context?, editorOnly?)
MUPLogger.Error(message, context?, editorOnly?)
MUPLogger.Exception(message, context?, editorOnly?)
MUPLogger.Exception(exception, context?, editorOnly?)
```
# MUPLogger

Logger simple pour Unity avec support des logs éditeur.

## Configuration

```C#
MUPLogger.Enabled = true;
MUPLogger.GlobalPrefix = "[MonJeu]";
MUPLogger.IncludeTimestamp = true;
MUPLogger.MinimumLevel = MUPLogger.LogLevel.Info;
```

## Utilisation

```C#
// Logs normaux - apparaissent partout
MUPLogger.Info("Jeu initialisé");
MUPLogger.Warning("Attention");
MUPLogger.Error("Erreur");

// Logs éditeur uniquement - n'apparaissent que dans l'éditeur
MUPLogger.Info("Debug info", editorOnly: true);
MUPLogger.LogDebug("Position du joueur", editorOnly: true);

// Avec contexte Unity (clic direct vers l'objet)
MUPLogger.Info("Joueur initialisé", this);

// Gestion d'exceptions
try
{
    // Code qui peut lever une exception
}
catch (Exception ex)
{
    MUPLogger.Exception(ex, this, editorOnly: true); // Stack trace détaillé
    MUPLogger.Error("Erreur de chargement"); // Message simple pour l'utilisateur
}
```

## Méthodes

```C#
MUPLogger.LogDebug(message, context?, editorOnly?)
MUPLogger.Info(message, context?, editorOnly?)
MUPLogger.Warning(message, context?, editorOnly?)
MUPLogger.Error(message, context?, editorOnly?)
MUPLogger.Exception(message, context?, editorOnly?)
MUPLogger.Exception(exception, context?, editorOnly?)
```
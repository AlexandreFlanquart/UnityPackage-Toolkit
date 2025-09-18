# ServiceLocator

Gestionnaire de services pour Unity. Trouve automatiquement les composants dans la scène et les met en cache.

## Utilisation

### Récupérer un service (recommandé)
```C#
void Start()
{
    // Trouve automatiquement le service dans la scène
    MyClass instance = ServiceLocator.GetService<MyClass>();
    
    // Créer automatiquement si non trouvé
    MyClass instance = ServiceLocator.GetService<MyClass>(createObjectIfNotFound: true);
}
```

### Enregistrer manuellement (optionnel)
```C#
void Awake()
{
    // Enregistrer manuellement (pour optimisation ou contrôle)
    ServiceLocator.AddService<MyClass>(gameObject);
    
    // Remplacer un service existant
    ServiceLocator.AddService<MyClass>(gameObject, replaceExisting: true);
}
```

### Vérifier l'existence
```C#
if (ServiceLocator.Exists<MyClass>())
{
    // Le service existe
}
```

### Nettoyer le cache
```C#
ServiceLocator.Clear(); // Supprime tous les services du cache
```

## Comportement

- **Recherche automatique** : `GetService<T>()` trouve automatiquement le composant dans la scène
- **Cache intelligent** : Les services trouvés sont mis en cache pour éviter les recherches coûteuses
- **Objets inactifs** : Trouve aussi les objets inactifs (`FindObjectsInactive.Include`)
- **Enregistrement optionnel** : `AddService()` est utile pour l'optimisation ou le contrôle explicite

## Méthodes

```C#
ServiceLocator.AddService<T>(GameObject, replaceExisting?)
ServiceLocator.GetService<T>(createObjectIfNotFound?)
ServiceLocator.Exists<T>()
ServiceLocator.Clear()
```
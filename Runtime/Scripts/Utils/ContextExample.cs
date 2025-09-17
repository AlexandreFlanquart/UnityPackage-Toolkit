using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Exemple détaillé pour expliquer le paramètre context du MUPLogger.
    /// </summary>
    public class ContextExample : MonoBehaviour
    {
        [SerializeField] private GameObject targetObject;
        [SerializeField] private Material testMaterial;
        [SerializeField] private AudioClip testAudioClip;

        void Start()
        {
            // Configuration du logger pour voir les différences
            MUPLogger.GlobalPrefix = "[CONTEXT_DEMO]";
            MUPLogger.IncludeTimestamp = true;

            DemonstrateContextUsage();
        }

        void DemonstrateContextUsage()
        {
            // 1. SANS CONTEXT - Log simple sans lien cliquable
            MUPLogger.Info("Ce message n'a pas de contexte - pas de lien cliquable dans la Console");

            // 2. AVEC CONTEXT (this) - Le log sera lié à ce GameObject
            MUPLogger.Info("Ce message est lié à ce GameObject", this);
            // Dans la Console Unity, vous verrez un petit icône à côté du message
            // Cliquer dessus vous amènera directement à ce GameObject dans la hiérarchie

            // 3. AVEC CONTEXT (GameObject spécifique)
            if (targetObject != null)
            {
                MUPLogger.Warning("Problème détecté sur l'objet cible", targetObject);
                // Cliquer sur ce log vous amènera au targetObject dans la hiérarchie
            }

            // 4. AVEC CONTEXT (Component spécifique)
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                MUPLogger.Error("Erreur de rendu détectée", renderer);
                // Cliquer sur ce log vous amènera au composant Renderer dans l'Inspector
            }

            // 5. AVEC CONTEXT (Asset/Material)
            if (testMaterial != null)
            {
                MUPLogger.Info("Material chargé avec succès", testMaterial);
                // Cliquer sur ce log vous amènera au Material dans le Project
            }

            // 6. AVEC CONTEXT (AudioClip)
            if (testAudioClip != null)
            {
                MUPLogger.Info("AudioClip prêt à être joué", testAudioClip);
                // Cliquer sur ce log vous amènera à l'AudioClip dans le Project
            }

            // 7. AVEC CONTEXT (Script spécifique)
            var script = GetComponent<ContextExample>();
            if (script != null)
            {
                MUPLogger.LogDebug("Script ContextExample initialisé", script);
                // Cliquer sur ce log vous amènera au script dans l'Inspector
            }

            // 8. EXEMPLE PRATIQUE - Logging d'erreurs avec contexte
            TryToLoadAsset();
        }

        void TryToLoadAsset()
        {
            // Simulation d'une tentative de chargement d'asset
            if (testMaterial == null)
            {
                // Sans contexte - difficile de savoir d'où vient l'erreur
                MUPLogger.Error("Material manquant !");
                
                // Avec contexte - on sait exactement quel GameObject a le problème
                MUPLogger.Error("Material manquant sur ce GameObject !", this);
            }
            else
            {
                MUPLogger.Info("Material chargé avec succès", testMaterial);
            }
        }

        void OnValidate()
        {
            // Exemple d'utilisation dans OnValidate (appelé quand on modifie des valeurs dans l'Inspector)
            if (targetObject == null)
            {
                MUPLogger.Warning("Target Object n'est pas assigné", this);
            }
        }
    }
}

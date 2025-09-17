using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Exemple d'utilisation des logs éditeur du MUPLogger.
    /// </summary>
    public class EditorLogExample : MonoBehaviour
    {
        void Start()
        {
            // Configuration du logger
            MUPLogger.GlobalPrefix = "[EDITOR_DEMO]";
            MUPLogger.IncludeTimestamp = true;

            DemonstrateEditorLogs();
        }

        void DemonstrateEditorLogs()
        {
            // 1. Logs normaux - apparaissent dans l'éditeur ET dans les builds
            MUPLogger.Info("Ce message apparaît partout");
            MUPLogger.Warning("Ce warning apparaît partout");

            // 2. Logs éditeur uniquement - n'apparaissent QUE dans l'éditeur
            MUPLogger.Info("Ce message n'apparaît QUE dans l'éditeur", editorOnly: true);
            MUPLogger.LogDebug("Debug info pour le développement uniquement", editorOnly: true);
            MUPLogger.Warning("Warning de développement - pas dans le build final", editorOnly: true);

            // 3. Utilisation avec contexte
            MUPLogger.Info("Ce GameObject a été initialisé (éditeur seulement)", this, editorOnly: true);

            // 4. Logs de développement vs production
            LogDevelopmentInfo();
            LogProductionInfo();

            // 5. Gestion d'erreurs avec séparation éditeur/production
            HandleError();
        }

        void LogDevelopmentInfo()
        {
            // Informations utiles pour le développement mais pas pour l'utilisateur final
            MUPLogger.Info($"Position du joueur: {transform.position}", editorOnly: true);
            MUPLogger.Info($"FPS actuel: {1.0f / Time.deltaTime:F1}", editorOnly: true);
            MUPLogger.LogDebug($"Mémoire utilisée: {System.GC.GetTotalMemory(false) / 1024 / 1024} MB", editorOnly: true);
        }

        void LogProductionInfo()
        {
            // Informations importantes pour l'utilisateur final
            MUPLogger.Info("Jeu initialisé avec succès");
            MUPLogger.Info("Sauvegarde créée");
        }

        void HandleError()
        {
            try
            {
                // Simulation d'une erreur
                throw new System.Exception("Erreur de test");
            }
            catch (System.Exception ex)
            {
                // Log détaillé pour le développement
                MUPLogger.Exception(ex, this, editorOnly: true);
                MUPLogger.Error($"Stack trace complet disponible dans l'éditeur", editorOnly: true);

                // Log simple pour l'utilisateur final
                MUPLogger.Error("Une erreur s'est produite. Veuillez réessayer.");
            }
        }

        void Update()
        {
            // Exemple de logs conditionnels
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // Log de debug pour le développement
                MUPLogger.LogDebug("Espace pressé - debug info", this, editorOnly: true);
                
                // Log normal pour le gameplay
                MUPLogger.Info("Action effectuée");
            }
        }

        void OnValidate()
        {
            // OnValidate n'est appelé que dans l'éditeur, donc on peut utiliser editorOnly
            MUPLogger.Info("Valeurs validées dans l'éditeur", this, editorOnly: true);
        }

        void OnDrawGizmos()
        {
            // OnDrawGizmos n'est appelé que dans l'éditeur
            MUPLogger.LogDebug("Gizmos dessinés", this, editorOnly: true);
        }
    }
}

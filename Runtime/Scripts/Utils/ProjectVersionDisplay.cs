using TMPro;
using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    /// <summary>Displays <see cref="Application.version"/> on a bound <see cref="TextMeshProUGUI"/> label.</summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public sealed class ProjectVersionDisplay : MonoBehaviour
    {
        [SerializeField] private string prefix = "Version ";

        private void Awake()
        {
            string text = prefix + Application.version;
            GetComponent<TextMeshProUGUI>().text = text;
            MUPLogger.Info($"ProjectVersionDisplay: displaying '{text}'.", this);
        }
    }
}

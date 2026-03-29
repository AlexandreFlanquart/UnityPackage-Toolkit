using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public sealed class ProjectVersionDisplay : MonoBehaviour
{
    [SerializeField] private string prefix = "Version ";

    private void Awake()
    {
        GetComponent<TextMeshProUGUI>().text = prefix + Application.version;
    }
}
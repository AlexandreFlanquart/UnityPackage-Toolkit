#if I2LOC_PRESENT
    using I2.Loc;
#endif
using UnityEngine;
using UnityEngine.UI;

public class LanguageSwitcher : MonoBehaviour
{
    #if I2LOC_PRESENT
    [SerializeField] private Toggle[] toggles;

    void Start()
    {   
        SetLanguage(LocalizationManager.CurrentLanguage);
        for(int i = 0; i < toggles.Length; i++)
        {
            int index = i;
            if(LocalizationManager.CurrentLanguage == toggles[index].name)
            {
                toggles[index].isOn = true;
            }
            toggles[index].onValueChanged.AddListener(delegate {
                if(toggles[index].isOn) SetLanguage(toggles[index].name);
            });
        }
    }

    public void SetLanguage(string languageName)
    {
        LocalizationManager.CurrentLanguage = languageName;
        Debug.Log("Language changed to " + languageName);
    }
    #endif
}

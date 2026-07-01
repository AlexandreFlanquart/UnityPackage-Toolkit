using UnityEngine;
#if I2LOC_PRESENT
using I2.Loc;
using UnityEngine.Events;
using UnityEngine.UI;
#endif

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Syncs a set of UI toggles with I2 Localization's current language (no-op if I2 isn't installed).
    /// </summary>
    public class LanguageSwitcher : MonoBehaviour
    {
#if I2LOC_PRESENT
        [SerializeField] private Toggle[] toggles;
        /// <summary>Language names matching each toggle. Falls back to the toggle's GameObject name if empty.</summary>
        [SerializeField] private string[] languages;

        private UnityAction<bool>[] _toggleListeners;

        void Start()
        {
            _toggleListeners = new UnityAction<bool>[toggles.Length];
            for (int i = 0; i < toggles.Length; i++)
            {
                int index = i;
                _toggleListeners[index] = isOn => { if (isOn) SetLanguage(GetLanguageName(index)); };
                toggles[index].onValueChanged.AddListener(_toggleListeners[index]);
            }
            SyncToggles(LocalizationManager.CurrentLanguage);
            LocalizationManager.OnLocalizeEvent.AddListener(OnLanguageChanged);
        }

        void OnDestroy()
        {
            if (_toggleListeners != null)
            {
                for (int i = 0; i < toggles.Length && i < _toggleListeners.Length; i++)
                {
                    if (_toggleListeners[i] != null)
                        toggles[i].onValueChanged.RemoveListener(_toggleListeners[i]);
                }
            }
            LocalizationManager.OnLocalizeEvent.RemoveListener(OnLanguageChanged);
        }

        private void OnLanguageChanged()
        {
            SyncToggles(LocalizationManager.CurrentLanguage);
        }

        private void SyncToggles(string currentLanguage)
        {
            for (int i = 0; i < toggles.Length; i++)
            {
                toggles[i].SetIsOnWithoutNotify(GetLanguageName(i) == currentLanguage);
            }
        }

        /// <summary>Returns the language name for a toggle index, using the languages array if set, otherwise the toggle's GameObject name.</summary>
        private string GetLanguageName(int index)
        {
            if (languages != null && index < languages.Length && !string.IsNullOrEmpty(languages[index]))
                return languages[index];
            return toggles[index].name;
        }

        /// <summary>Sets the active I2 Localization language and logs the change.</summary>
        public void SetLanguage(string languageName)
        {
            LocalizationManager.CurrentLanguage = languageName;
            MUPLogger.Info("Language changed to " + languageName);
        }
#endif
    }
}

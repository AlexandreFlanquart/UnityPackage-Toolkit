using System;
using UnityEngine;
#if I2LOC_PRESENT
using I2.Loc;
#endif

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// One voice line recorded in several languages. <see cref="VoiceManager"/> plays the clip
    /// matching the current I2 Localization language, or the first entry without I2.
    /// </summary>
    [CreateAssetMenu(fileName = "VoiceLineSO", menuName = "MyUnityPackage/Audio/VoiceLine")]
    public class VoiceLineSO : ScriptableObject
    {
        [Serializable]
        public struct LocalizedClip
        {
            [Tooltip("I2 language code, e.g. \"fr\" or \"en\". \"fr\" also matches regional codes such as \"fr-FR\".")]
            [SerializeField] private string languageCode;
            [Tooltip("Recording of this line in that language.")]
            [SerializeField] private AudioClip clip;

            public string LanguageCode => languageCode;
            public AudioClip Clip => clip;
        }

        [Tooltip("One entry per language. The first entry is the fallback when the current language has no recording.")]
        [SerializeField] private LocalizedClip[] clips;

        /// <summary>Returns the clip for the current language, the first entry as fallback, or null if empty.</summary>
        public AudioClip GetClip()
        {
            if (clips == null || clips.Length == 0) return null;

#if I2LOC_PRESENT
            string languageCode = LocalizationManager.CurrentLanguageCode;
            foreach (var entry in clips)
            {
                if (entry.Clip != null && !string.IsNullOrEmpty(entry.LanguageCode) && !string.IsNullOrEmpty(languageCode)
                    && languageCode.StartsWith(entry.LanguageCode, StringComparison.OrdinalIgnoreCase))
                {
                    return entry.Clip;
                }
            }
            MUPLogger.Warning($"VoiceLineSO '{name}': no clip for language '{languageCode}', playing '{clips[0].LanguageCode}'.", this);
#endif
            return clips[0].Clip;
        }
    }
}

using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    /// <summary>Per-channel default volume and mute/unmute sprites, auto-loaded by <see cref="AudioManager"/> from Resources.</summary>
    [CreateAssetMenu(fileName = "AudioSettingsSO", menuName = "ScriptableObjects/AudioSettingsSO")]
    public class AudioSettingsSO : ScriptableObject
    {
        [Range(0, 1)]
        [SerializeField] public float defaultVolume;
        [SerializeField] public Sprite mutedImage;
        [SerializeField] public Sprite unmutedImage;
    }
}
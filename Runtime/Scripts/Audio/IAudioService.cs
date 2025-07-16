using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    public interface IAudioService
    {
        void InitVolume(AudioManager.AudioType audioType, float volume);
        void SetVolume(AudioManager.AudioType audioType, float volume);
        float GetVolume(AudioManager.AudioType audioType);
        void ToggleMute(AudioManager.AudioType audioType);
        AudioManager.AudioSetting GetAudioSettingsFromAudioType(AudioManager.AudioType audioType);
    }
}
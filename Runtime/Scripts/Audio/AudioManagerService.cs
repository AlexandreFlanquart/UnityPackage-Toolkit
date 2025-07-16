
namespace MyUnityPackage.Toolkit
{
    public class AudioManagerService : IAudioService
    {
        public void InitVolume(AudioManager.AudioType audioType, float volume)
            => AudioManager.InitVolume(audioType, volume);

        public void SetVolume(AudioManager.AudioType audioType, float volume)
            => AudioManager.SetVolume(audioType, volume);

        public float GetVolume(AudioManager.AudioType audioType)
            => AudioManager.GetVolume(audioType);

        public void ToggleMute(AudioManager.AudioType audioType)
            => AudioManager.ToggleMute(audioType);

        public AudioManager.AudioSetting GetAudioSettingsFromAudioType(AudioManager.AudioType audioType)
            => AudioManager.GetAudioSettingsFromAudioType(audioType);
    }
}
using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Abstraction for services capable of playing back audio clips for the toolkit.
    /// </summary>
    public interface IAudioPlaybackService
    {
        /// <summary>
        /// Play an <see cref="AudioClip"/> using the channel that matches the provided <see cref="AudioManager.AudioType"/>.
        /// </summary>
        /// <param name="clip">The audio clip to play.</param>
        /// <param name="audioType">The audio type used to select the underlying <see cref="AudioSource"/>.</param>
        /// <param name="loop">Should the audio clip loop.</param>
        /// <param name="volume">Playback volume between 0 and 1.</param>
        /// <returns>The <see cref="AudioSource"/> that was used to play the clip, or <c>null</c> if playback failed.</returns>
        AudioSource PlayClip(AudioClip clip, AudioManager.AudioType audioType, bool loop = false, float volume = 1f);

        /// <summary>
        /// Load an <see cref="AudioClip"/> from the Resources folder and play it on the requested channel.
        /// </summary>
        /// <param name="resourcePath">Relative path inside the Resources folder.</param>
        /// <param name="audioType">The audio channel to use for playback.</param>
        /// <param name="loop">Should the audio clip loop.</param>
        /// <param name="volume">Playback volume between 0 and 1.</param>
        /// <returns>The loaded clip, or <c>null</c> when nothing was played.</returns>
        AudioClip PlayFromResources(string resourcePath, AudioManager.AudioType audioType, bool loop = false, float volume = 1f);

        /// <summary>
        /// Stop playback on the specified audio channel.
        /// </summary>
        /// <param name="audioType">The channel to stop.</param>
        void Stop(AudioManager.AudioType audioType);

        /// <summary>
        /// Stop playback on all managed channels.
        /// </summary>
        void StopAll();
    }
}

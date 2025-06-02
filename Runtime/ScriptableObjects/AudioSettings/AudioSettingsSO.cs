using UnityEngine;
using UnityEngine.UI;
[CreateAssetMenu(fileName = "AudioSettingsSO", menuName = "ScriptableObjects/AudioSettingsSO")]
public class AudioSettingsSO : ScriptableObject
{
    [Range(0, 1)]
    [SerializeField] public float defaultVolume;

    [SerializeField] public Sprite mutedImage;
    [SerializeField] public Sprite unmutedImage;
}

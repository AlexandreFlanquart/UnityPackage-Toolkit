using UnityEngine;
using UnityEngine.EventSystems;

namespace MyUnityPackage.Toolkit
{
    public class VoiceAgent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private float delay = 0f;
        [SerializeField] private string[] keys = new string[0];
        [SerializeField] private bool onEnable = false;
        [SerializeField] private bool activeOnHover = true;
        [SerializeField] private bool stopOnDisable = true;

        private bool isPlaying = false;
        private VoiceManager voiceManager;

        private void Awake()
        {
            voiceManager = ServiceLocator.GetService<VoiceManager>();
            if (voiceManager == null)
            {
                Logger.LogMessageError("VoiceAgent: VoiceManager not found in ServiceLocator");
                enabled = false;
            }
        }

        private void OnEnable() 
        {
            if(onEnable) PlayVoice();
        }

        private void OnDisable()
        {
            if (stopOnDisable && isPlaying)
            {
                StopVoice();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (activeOnHover) PlayVoice();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (activeOnHover && isPlaying) StopVoice();
        }

        public void PlayVoice()
        {
            if (voiceManager == null) return;
            if (keys == null || keys.Length == 0) return;

            isPlaying = voiceManager.PlayVoices(delay, keys);
        }

        public void StopVoice()
        {
            if (voiceManager == null) return;
            voiceManager.StopVoice();
            isPlaying = false;
        }

        public void Initialize(float pDelay, params string[] pKeys)
        {
            if (pKeys == null || pKeys.Length == 0) return;
            keys = pKeys;
            delay = Mathf.Max(0f, pDelay);
        }

        public void SetKeys(params string[] pKeys)
        {
            if (pKeys == null || pKeys.Length == 0) return;
            keys = pKeys;
        }

        public void SetDelay(float pDelay)
        {
            delay = Mathf.Max(0f, pDelay);
        }
    }
}

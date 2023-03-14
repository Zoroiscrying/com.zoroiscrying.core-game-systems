using UnityEngine;
using Zoroiscrying.CoreGameSystems.AudioSystem.ScriptableObjectIntegration;

namespace Zoroiscrying.CoreGameSystems.AudioSystem
{
    public class AudioSourcePool : ComponentPool<AudioSource>
    {
        public AudioSourcePool(AudioSource prefab, Transform parent) : base(prefab, parent)
        {
            
        }

        protected override bool IsActive(AudioSource component)
        {
            return component.isPlaying || component.loop;
        }

        public AudioSource GetSource(AudioSourceSettingSo audioSourceSetting)
        {
            var source = Get();
            audioSourceSetting.ApplySettings(source);
            return source;
        }
        
        public AudioSource GetSource()
        {
            return Get();
        }
    }
}
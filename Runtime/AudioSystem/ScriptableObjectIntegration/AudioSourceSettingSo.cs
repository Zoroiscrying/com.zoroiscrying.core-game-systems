using UnityEngine;
using UnityEngine.Audio;

namespace Zoroiscrying.CoreGameSystems.AudioSystem.ScriptableObjectIntegration
{
    [CreateAssetMenu(fileName = "new Audio Source Setting", menuName = "Unity Core/Unity Audio System/Audio Source Setting")]
    public class AudioSourceSettingSo : ScriptableObject
    {
            public AudioMixerGroup outputMixer;
            public bool mute;
            public bool byPassEffects;
            public bool byPassReverbZones;
            public bool playOnAwake = true;
            public bool loop = false;
            [Range(0, 256)]
            public int priority = 128;
            [Range(0f, 1f)]
            public float volume = 1;
            [Range(-3f, 3f)]
            public float pitch = 1;
            [Range(-1f, 1f)]
            public float stereoPan = 0;
            [Range(0f, 1f)]
            public float spatialBlend = 0;
            [Range(0f, 1.1f)]
            public float reverbZoneMix = 1;
            [Header("3D Surround Settings")]
            [Range(0f, 5f)]
            public float dopplerLevel = 1;
            [Range(0, 360)]
            public int spread = 0;
            public AudioRolloffMode volumeRollOff = AudioRolloffMode.Logarithmic;
            public float MinDistance = 1;
            public float MaxDistance = 500;

            [Header("More Control")] [SerializeField, Range(0f, 1f)]
            private float randomPitch = 0f;
    
            private void OnValidate()
            {
                MaxDistance = Mathf.Clamp(MaxDistance, MinDistance, float.MaxValue);
                MinDistance = Mathf.Clamp(MinDistance,0, MaxDistance);
            }
    
            public void ApplySettings(AudioSource audioSource)
            {
                //audioSource.clip = this.audioClip;
                audioSource.outputAudioMixerGroup = this.outputMixer;
                audioSource.mute = this.mute;
                audioSource.bypassEffects = this.byPassEffects;
                audioSource.bypassReverbZones = this.byPassReverbZones;
                audioSource.playOnAwake = this.playOnAwake;
                audioSource.loop = this.loop;
                audioSource.priority = this.priority;
                audioSource.volume = this.volume;
                audioSource.pitch = this.pitch + Random.Range(-1f, 1f) * randomPitch;
                audioSource.panStereo = this.stereoPan;
                audioSource.spatialBlend = this.spatialBlend;
                audioSource.reverbZoneMix = this.reverbZoneMix;
                audioSource.dopplerLevel = this.dopplerLevel;
                audioSource.spread = this.spread;
                audioSource.rolloffMode = this.volumeRollOff;
                audioSource.minDistance = this.MinDistance;
                audioSource.maxDistance = this.MaxDistance;
            }
    }
}
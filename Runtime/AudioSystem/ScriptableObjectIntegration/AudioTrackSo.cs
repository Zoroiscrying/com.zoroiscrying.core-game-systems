using UnityEngine;

namespace Zoroiscrying.CoreGameSystems.AudioSystem.ScriptableObjectIntegration
{
    [CreateAssetMenu(fileName = "new Audio Track", menuName = "Unity Core/Unity Audio System/Audio Track")]
    public class AudioTrackSo : ScriptableObject
    {
        [SerializeField] private bool multiPlayingTrack = false;
        public bool MultiPlayingTrack => multiPlayingTrack;

        private AudioSource m_AudioSource = null;
        public AudioSource AudioSource => m_AudioSource;
        public AudioObjectSo PlayingObject { get; set; } = default;

        [Header("Audio Source Configurations")]
        public AudioSourceSettingSo audioSourceSetting;

        //Compared with the Editor configuration implementation of the audio track
        //this method will cause a little bit of initialization time
        //but the configurations can be saved in local files and easily reused for different purposes
        //TODO::Let audio track manage all the audio sources, since the initialization of the audio source only needs once
        public void RegisterAudioSource(AudioSource audioSource)
        {
            audioSourceSetting.ApplySettings(audioSource);
            m_AudioSource = audioSource;
        }
        
        public void RegisterAudioClip(AudioClip audioClip)
        {
            m_AudioSource.clip = audioClip;
        }

        public bool IsPlaying()
        {
            if (multiPlayingTrack)
            {
                return false;
            }
            return AudioSource.isPlaying; 
        }
    }
}
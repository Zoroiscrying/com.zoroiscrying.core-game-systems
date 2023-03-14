using UnityEngine;

namespace Zoroiscrying.CoreGameSystems.AudioSystem.ScriptableObjectIntegration
{
    [CreateAssetMenu(fileName = "new Audio Object", menuName = "Unity Core/Unity Audio System/Audio Object")]
    // /// <summary>
    // /// An audio object holds an AudioTrack and an associated AudioClip.
    // /// </summary>
    public class AudioObjectSo : ScriptableObject
    {
        public virtual AudioClip AudioClip => audioClip;
        public virtual AudioTrackSo PlayingTrack => playingTrack;
        
        [SerializeField] private AudioClip audioClip;
        [SerializeField] private AudioTrackSo playingTrack;
    }
}
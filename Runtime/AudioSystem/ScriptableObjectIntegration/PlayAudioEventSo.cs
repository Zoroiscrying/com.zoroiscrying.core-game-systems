using System;
using Zoroiscrying.ScriptableObjectCore;
using UnityEngine;

namespace Zoroiscrying.CoreGameSystems.AudioSystem.ScriptableObjectIntegration
{
    [Serializable]
    public class PlayAudioSetting
    {
        public AudioSystemPersistentManager.AudioJobAction audioAction;
        public AudioObjectSo audioObject;
        public bool fade = false;
        public float fadeTime = .2f;
        public float delay = 0f;
    }
    
    [CreateAssetMenu(fileName = "New Play Audio Event", menuName = "Unity Core/Unity Audio System/Play audio event")]
    public class PlayAudioEventSo : EventSO<PlayAudioSetting>
    {
        
    }
}
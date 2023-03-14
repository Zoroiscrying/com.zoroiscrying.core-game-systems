using System;
using Zoroiscrying.CoreGameSystems.UISystem;
using UnityEngine;
using Zoroiscrying.CoreGameSystems.AudioSystem.ScriptableObjectIntegration;

namespace ZoroiscryingUnityManagers.CoreSystems.AudioSystem
{
    [RequireComponent(typeof(BaseUiPanel))]
    public class OverridePanelAudioIntegration : MonoBehaviour
    {
        [SerializeField] private AudioObjectSo overridePanelOpenAudio;
        [SerializeField] private AudioObjectSo overridePanelCloseAudio;
        private void Awake()
        {
            var uiPanel = GetComponent<BaseUiPanel>();
            uiPanel.InjectAudioSo(overridePanelOpenAudio, overridePanelCloseAudio);
        }
    }
}
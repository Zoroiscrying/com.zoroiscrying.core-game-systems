using System;
using UnityEditor;
using UnityEngine;

namespace Zoroiscrying.CoreGameSystems.UISystem
{
    public enum PanelEventType
    {
        OnOpened,
        OnClosed,
        OnBeginOpen,
        OnBeginClose,
    }
    
    public abstract class BasePanelActionMonobehavior : MonoBehaviour
    {
        private BaseUiPanel _connectedPanel;

        private void OnEnable()
        {
            _connectedPanel = GetComponent<BaseUiPanel>();
            if (!_connectedPanel)
            {
                Debug.LogWarning(this.gameObject.name + " don't have BaseUiPanel component attached.");
                EditorGUIUtility.PingObject(this.gameObject);
                return;
            }
            RegisterEvents();
        }

        private void OnDisable()
        {
            if (_connectedPanel)
            {
                UnregisterEvents();
            }
        }

        protected virtual void RegisterEvents()
        {
            _connectedPanel.RegisterOnPanelOpened(OnPanelOpened);
            _connectedPanel.RegisterOnPanelClosed(OnPanelClosed);
            _connectedPanel.RegisterOnPanelBeginToOpen(OnPanelToOpen);
            _connectedPanel.RegisterOnPanelBeginToClose(OnPanelToClose);
        }

        protected virtual void UnregisterEvents()
        {
            _connectedPanel.UnregisterOnPanelOpened(OnPanelOpened);
            _connectedPanel.UnregisterOnPanelClosed(OnPanelClosed);
            _connectedPanel.UnregisterOnPanelBeginToOpen(OnPanelToOpen);
            _connectedPanel.UnregisterOnPanelBeginToClose(OnPanelToClose);
        }

        // Example - PlayAudio - For each list based on specific 
        protected abstract void OnPanelOpened();

        protected abstract void OnPanelClosed();
        
        protected abstract void OnPanelToOpen();

        protected abstract void OnPanelToClose();

    }
}
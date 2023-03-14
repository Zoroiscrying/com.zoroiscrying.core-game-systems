using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Zoroiscrying.CoreGameSystems.UISystem
{
    /// <summary>
    /// Organize a set of child panels and the related action to the panel bound to this panel's various behavior.
    /// </summary>
    public class UiChildPanel : BaseUiPanel
    {
        [SerializeField] private UnityEvent onThisPanelBeginToOpen;
        [SerializeField] private UnityEvent onThisPanelBeginToClose;
        [SerializeField] private UnityEvent onThisPanelOpened;
        [SerializeField] private UnityEvent onThisPanelClosed;

        private bool _registered = false;
        
        #region Unity Function

        /// <summary>
        /// Register functions
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            onPanelBeginToOpen += OnThisPanelBeginToOpen;
            onPanelBeginToClose += OnThisPanelBeginToClose;
            onPanelOpened += OnThisPanelOpened;
            onPanelClosed += OnThisPanelClosed;
            RegisterSelfToParentPanels();
        }

        /// <summary>
        /// Unregister functions
        /// </summary>
        protected virtual void OnDisable()
        {
            onPanelBeginToOpen -= OnThisPanelBeginToOpen;
            onPanelBeginToClose -= OnThisPanelBeginToClose;
            onPanelOpened -= OnThisPanelOpened;
            onPanelClosed -= OnThisPanelClosed;
            //UnregisterSelfToParentPanels();
        }

        private void OnDestroy()
        {
            UnregisterSelfToParentPanels();
        }

        #endregion

        #region Public Function

        
        
        #endregion

        #region Private Function

        private void RegisterSelfToParentPanels()
        {
            if (_registered)
            {
                return;
            }
            _registered = true;
            var foundParentPanel = this.transform.parent.gameObject.GetComponent<BaseUiPanel>();
            if (foundParentPanel != null)
            {
                parentPanel = foundParentPanel;
                parentPanel.RegisterChildPanel(this);
            }
        }

        private void UnregisterSelfToParentPanels()
        {
            if (parentPanel)
            {
                parentPanel.UnregisterChildPanel(this);
                parentPanel = null;
            }
        }
        
        private void OnThisPanelBeginToOpen()
        {
            onThisPanelBeginToOpen?.Invoke();
        }
        
        private void OnThisPanelBeginToClose()
        {
            onThisPanelBeginToClose?.Invoke();
        }
        
        private void OnThisPanelOpened()
        {
            onThisPanelOpened?.Invoke();
        }
        
        private void OnThisPanelClosed()
        {
            onThisPanelClosed?.Invoke();
        }
        #endregion
    }
}
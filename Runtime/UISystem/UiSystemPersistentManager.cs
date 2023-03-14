using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zoroiscrying.CoreGameSystems.AudioSystem;
using Zoroiscrying.CoreGameSystems.AudioSystem.ScriptableObjectIntegration;
using Zoroiscrying.CoreGameSystems.CoreSystemBase;
using Zoroiscrying.CoreGameSystems.UISystem.ScriptableObjectIntegration;

namespace Zoroiscrying.CoreGameSystems.UISystem
{
    /// <summary>
    /// Persistent Manager Handling UI for the game
    /// **Responsibility**
    /// - Handling all viable (Unique) UI panels, provide a way to handle all the UIs.
    /// - Enable and disable UI panels accordingly, including tween and animation using
    /// -
    /// - Runtime panels are created by child panels or persistent panels on the run
    ///     - And are handled by those panels and reacted by other systems and objects via SO events.
    /// **Example**
    /// 
    /// 
    /// </summary>
    public class UiSystemPersistentManager : BasePersistentManager<UiSystemPersistentManager>
    {
        [SerializeField] private PanelTypeSO entryPanel;
        [SerializeField] private List<BaseUiPanel> viablePanels;
        [SerializeField] private TurnPanelEventSo turnPanelEventSo;
        [SerializeField] private bool runtimeInitPanels;

        // Audio integration
        [SerializeField] private PlayAudioEventRaiser playAudioEventRaiser;
        [SerializeField] private AudioObjectSo defaultPanelOpenAudio;
        [SerializeField] private AudioObjectSo defaultPanelCloseAudio;
        [SerializeField] private TurnPanelEventRaiser turnPanelEventRaiser;
        
        private Hashtable _panelHashTable;

        #region Unity Events
        
        // The initialization is handled automatically in the 'InitManager' function.
        protected override void OnEnable()
        {
            base.OnEnable();
            turnPanelEventSo.Register(TurnPanel);
        }

        private void OnDisable()
        {
            turnPanelEventSo.Unregister(TurnPanel);
        }

        #endregion

        #region Public Functions

        public void TurnPanel(TurnPanelSetting turnPanelSetting)
        {
            if (turnPanelSetting.on)
            {
                TurnPanelOn(
                    turnPanelSetting.panelType, 
                    turnPanelSetting.forceTurnPanel,
                    turnPanelSetting.waitTurnPanel,
                    turnPanelSetting.forceTurnOnIncludingParent,
                    turnPanelSetting.turnOffOtherPanelsAtSameLevel,
                    turnPanelSetting.forceTurnOtherPanels,
                    turnPanelSetting.waitTurnOtherPanels
                    );
            }
            else
            {
                TurnPanelOff(
                    turnPanelSetting.panelType,
                    turnPanelSetting.forceTurnPanel,
                    turnPanelSetting.waitTurnPanel);
            }
        }
        
        /// <summary>
        /// Turn panel On.
        /// </summary>
        /// <param name="panelType">The panel type to turn on.</param>
        /// <param name="forceFinish">If force this panel type to turn off then turn it on.</param>
        /// <param name="waitForFinish">If wait for this panel type to turn off then turn it on.</param>
        /// <param name="forceTurnOnIncludingParent">If force parent panels to be turned on.</param>
        /// <param name="turnOffOtherPanelsAtSameLevel">If turn other panels at the same level off</param>
        /// <param name="forceSameLevelPanelsToFinish">Same logic as the parameters above</param>
        /// <param name="waitForSameLevelPanelsToFinish">Same logic as the parameters above</param>
        public void TurnPanelOn(
            PanelTypeSO panelType, 
            bool forceFinish = true, 
            bool waitForFinish = false, 
            bool forceTurnOnIncludingParent = false,
            bool turnOffOtherPanelsAtSameLevel = false,
            bool forceSameLevelPanelsToFinish = true,
            bool waitForSameLevelPanelsToFinish = false)
        {
            if (panelType == null)
            {
                return;
            }

            if (!PanelExists(panelType))
            {
                Debug.LogWarning("The page type [" + panelType.Id + "] is not registered!");
            }

            BaseUiPanel panelToOn = GetPanel(panelType);

            if (forceTurnOnIncludingParent)
            {
                Stack<BaseUiPanel> parentPanels = new Stack<BaseUiPanel>();
                // Loop upwards till finding the root, stack all the unactivated parent panels
                for (var panel = panelToOn; ;)
                {
                    if (!panel.IsChild || panel.ParentPanel == null)
                    {
                        break;
                    }

                    parentPanels.Push(panel.ParentPanel);
                    panel = panelToOn.ParentPanel;
                }

                if (parentPanels.Count > 0)
                {
                    for (var panel = parentPanels.Pop(); ;)
                    {
                        //Debug.Log(panel.gameObject.name);
                    
                        panel.gameObject.SetActive(true);
                        panel.AnimatePanel(true);
                        if ( parentPanels.Count == 0)
                        {
                            break;
                        }
                        panel = parentPanels.Pop();
                    }   
                }
            }

            // The panel is on and is activated.
            if (panelToOn.IsOn && panelToOn.gameObject.activeSelf && !turnOffOtherPanelsAtSameLevel)
            {
                return;
            }
            // The panel is not on, whether activated or not.
            else if (!panelToOn.IsOn)
            {
                panelToOn.gameObject.SetActive(true);
                panelToOn.AnimatePanel(true);
            }
            // The panel is turning off
            else if(!panelToOn.ShouldDoOnMotion)
            {
                if (forceFinish)
                {
                    panelToOn.gameObject.SetActive(true);
                    panelToOn.FinishBehavior();
                    panelToOn.AnimatePanel(true);
                }
                else if (waitForFinish)
                {
                    panelToOn.gameObject.SetActive(true);
                    panelToOn.ShouldTurnOnAfterwards();
                }
                // Don't do anything if not force and not wait.
            }
            // Don't care about if the panel is turning on or not.

            // Turn other panels that has the save level as this panel
            if (turnOffOtherPanelsAtSameLevel)
            {
                foreach (var panel in _panelHashTable.Values)
                {
                    var panelObject = panel as BaseUiPanel;
                    
                    if (panelObject != null && // Firstly, the panel should be valid
                        panelObject.PanelTypeSo.Id != panelToOn.PanelTypeSo.Id && // Then their panel types aren't the same
                        panelObject.ParentPanel == panelToOn.ParentPanel && // Their levels are the same (This may be accelerated by some form of data strcture)
                        (panelObject.IsOn || panelObject.ShouldDoOnMotion) ) // Then the panel should be turned on, or is turning on.
                    {
                        //Debug.Log("Detected same level panel object: \n" + 
                        //          panelObject.gameObject.name + panelObject.ParentPanel + "\n" + 
                        //          panelToOn.gameObject.name + panelToOn.ParentPanel);
                        if (panelObject.IsOn)
                        {
                            TurnPanelOff(panelObject.PanelTypeSo);
                            //Debug.Log("Turning page off:" + panelObject.PanelTypeSo.Id);
                        }
                        else if (panelObject.ShouldDoOnMotion)
                        {
                            //Debug.Log("Turning page off:" + panelObject.PanelTypeSo.Id);
                            if (forceSameLevelPanelsToFinish)
                            {
                                panelObject.FinishBehavior();
                                panelObject.AnimatePanel(false);
                            }
                            else if (waitForSameLevelPanelsToFinish)
                            {
                                panelObject.ShouldTurnOffAfterwards();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Turn a page off and turn a page on, afterwards or at the same time, controlled by the waitForExit parameter
        /// </summary>
        /// <param name="off">The panel that will be turned off</param>
        /// <param name="on">The panel that will be turned on</param>
        /// <param name="waitForExit">If the page that is going to be turned on
        /// wait for the other page to be turned off</param>
        public void TurnPanelOff(PanelTypeSO panelTypeToOff, bool forceFinish = true , bool waitForFinish = false)
        {
            if (panelTypeToOff == null)
            {
                return;
            }
            
            if (!PanelExists(panelTypeToOff))
            {
                Debug.LogWarning("The page type [" + panelTypeToOff.Value + "] is not registered!");
            }

            BaseUiPanel panelToOff = GetPanel(panelTypeToOff);

            if (panelToOff.IsOn)
            {
                panelToOff.AnimatePanel(false);
            }
            else if (panelToOff.ShouldDoOnMotion)
            {
                if (forceFinish)
                {
                    panelToOff.FinishBehavior();
                    panelToOff.AnimatePanel(false);
                    return;
                }
                // wait for the panel to turn on, then turn off this panel.
                if (waitForFinish)
                {
                    panelToOff.ShouldTurnOffAfterwards();
                }
            }
        }
        
        public bool IsPageOn(PanelTypeSO pageType)
        {
            BaseUiPanel panel = GetPanel(pageType);
            if (panel)
            {
                return panel.IsOn;
            }
            return false;
        }

        #endregion

        #region Private Functions
        protected override void InitManager()
        {
            _panelHashTable = new Hashtable();
            RegisterAllPanels();
        }

        private void RegisterAllPanels()
        {
            if (runtimeInitPanels)
            {
                viablePanels = FindObjectsOfType<BaseUiPanel>().ToList();
            }

            foreach (var panel in viablePanels)
            {
                RegisterPanel(panel);
            }
        }
        
        private void RegisterPanel(BaseUiPanel panel)
        {
            if (PanelExists(panel.PanelTypeSo))
            {
                Debug.LogWarning("Trying to register a page that is already Registered! Type: " + panel.PanelTypeSo.Value +
                           ". GameObject name: " + panel.gameObject.name);
                return;
            }
            
            _panelHashTable.Add(panel.PanelTypeSo.Id, panel);
            
            // Play Audio Event Raiser
            if (!panel.CustomAudioIntegration)
            {
                panel.InjectAudioSo(playAudioEventRaiser, defaultPanelOpenAudio, defaultPanelCloseAudio);
            }
            else
            {
                panel.InjectEventRaiser(playAudioEventRaiser);
            }

            // Turn panel event raiser
            panel.InjectEventRaiser(turnPanelEventRaiser);
        }
        
        private BaseUiPanel GetPanel(PanelTypeSO panelType)
        {
            if (!PanelExists(panelType))
            {
                Debug.LogWarning("You are trying to get a page that has not been registered. Type:[" + panelType +"]");
                return null;
            }
            
            return (BaseUiPanel)_panelHashTable[panelType.Id];
        }
        
        private bool PanelExists(PanelTypeSO panelType)
        {
            return _panelHashTable.ContainsKey(panelType.Id);
        }

        #endregion

    }
}
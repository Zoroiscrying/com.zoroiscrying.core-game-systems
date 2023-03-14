using System.Collections.Generic;
using UnityEngine;

namespace Zoroiscrying.CoreGameSystems.UISystem
{
    [RequireComponent(typeof(BaseUiPanel))]
    public class OverridePanelActionIntegration : MonoBehaviour
    {
        [SerializeField] private List<PanelActionPair> panelActionPairsBeginToOpen 
            = new List<PanelActionPair>();
        [SerializeField] private List<PanelActionPair> panelActionPairsBeginToClose 
            = new List<PanelActionPair>();
        [SerializeField] private List<PanelActionPair> panelActionPairsOpened 
            = new List<PanelActionPair>();
        [SerializeField] private List<PanelActionPair> panelActionPairsClosed 
            = new List<PanelActionPair>();
        private void Awake()
        {
            var uiPanel = GetComponent<BaseUiPanel>();
            if (panelActionPairsBeginToOpen.Count > 0)
            {
                uiPanel.InjectPanelActions(panelActionPairsBeginToOpen, PanelEventType.OnBeginOpen);
            }
            
            if (panelActionPairsBeginToClose.Count > 0)
            {
                uiPanel.InjectPanelActions(panelActionPairsBeginToClose, PanelEventType.OnBeginClose);
            }
            
            if (panelActionPairsOpened.Count > 0)
            {
                uiPanel.InjectPanelActions(panelActionPairsOpened, PanelEventType.OnOpened);
            }
            
            if (panelActionPairsClosed.Count > 0)
            {
                uiPanel.InjectPanelActions(panelActionPairsClosed, PanelEventType.OnClosed);
            }
        }
    }
}
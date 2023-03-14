using System;
using Zoroiscrying.ScriptableObjectCore;
using UnityEngine;

namespace Zoroiscrying.CoreGameSystems.UISystem.ScriptableObjectIntegration
{
    [Serializable]
    public class TurnPanelSetting
    {
        public PanelTypeSO panelType;
        public bool on = true;
        public bool turnOffOtherPanelsAtSameLevel = false;
        public bool forceTurnPanel = true;
        public bool waitTurnPanel = false;
        public bool forceTurnOtherPanels = true;
        public bool waitTurnOtherPanels = false;
        public bool forceTurnOnIncludingParent = false;
    }
    
    [CreateAssetMenu(fileName = "New Turn Panel Event So", menuName = "Unity Core/Unity Panel System/SO Event/Turn Panel Event")]
    public class TurnPanelEventSo : EventSO<TurnPanelSetting>
    {
        
    }
}
using System;
using Zoroiscrying.CoreGameSystems.UISystem;
using Zoroiscrying.CoreGameSystems.UISystem.ScriptableObjectIntegration;
using UnityEditor;
using UnityEngine;
using Zoroiscrying.CoreGameSystems.CoreSystemUtility.Editor;

namespace Zoroiscrying.CoreGameSystems.UISystem.Editor
{
    [CustomEditor(typeof(UiPersistentPanel))]
    public class UiPersistentPanelEditor : BaseUiPanelEditor
    {
        private UiPersistentPanel _uiPersistentPanel;

        protected override void GetTarget()
        {
            base.GetTarget();
            _uiPersistentPanel = (UiPersistentPanel)target;
        }

        protected override void DrawPanelTypes()
        {
            EditorGUILayout.LabelField("Persistent Panel", EditorStyles.centeredGreyMiniLabel);
            base.DrawPanelTypes();
        }
    }
}
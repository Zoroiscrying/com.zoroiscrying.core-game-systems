using Zoroiscrying.CoreGameSystems.UISystem;
using UnityEditor;
using Zoroiscrying.CoreGameSystems.CoreSystemUtility.Editor;

namespace Zoroiscrying.CoreGameSystems.UISystem.Editor
{
    [CustomEditor(typeof(UiChildPanel))]
    public class UiChildPanelEditor : BaseUiPanelEditor
    {
        private UiChildPanel _uiChildPanel;

        private SerializedProperty _childPanelActionPairsBeginToOpenProperty;
        private SerializedProperty _childPanelActionPairsBeginToCloseProperty;
        private SerializedProperty _childPanelActionPairsOpenedProperty;
        private SerializedProperty _childPanelActionPairsClosedProperty;

        private bool _childPanelAction;
        private bool _unityEventAction;

        protected override void GetSerializedProperty()
        {
            base.GetSerializedProperty();
            _childPanelActionPairsBeginToOpenProperty =
                serializedObject.FindProperty("childPanelActionPairsBeginToOpen");
            _childPanelActionPairsBeginToCloseProperty =
                serializedObject.FindProperty("childPanelActionPairsBeginToClose");
            _childPanelActionPairsOpenedProperty =
                serializedObject.FindProperty("childPanelActionPairsOpened");
            _childPanelActionPairsClosedProperty =
                serializedObject.FindProperty("childPanelActionPairsClosed");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                serializedObject.Update();

                DrawChildPanelActionPairs();

                serializedObject.ApplyModifiedProperties();
            }
        }

        protected override void GetTarget()
        {
            base.GetTarget();
            _uiChildPanel = (UiChildPanel)target;
        }

        protected override void DrawPanelTypes()
        {
            EditorGUILayout.LabelField("Child Panel", CustomEditorStyles.middleMinorBoldLabel);
            
            base.DrawPanelTypes();
        }

        protected virtual void DrawChildPanelActionPairs()
        {
            EditorGUILayout.PropertyField(_childPanelActionPairsBeginToOpenProperty);
            EditorGUILayout.PropertyField(_childPanelActionPairsBeginToCloseProperty);
            EditorGUILayout.PropertyField(_childPanelActionPairsOpenedProperty);
            EditorGUILayout.PropertyField(_childPanelActionPairsClosedProperty);
        }
    }
}
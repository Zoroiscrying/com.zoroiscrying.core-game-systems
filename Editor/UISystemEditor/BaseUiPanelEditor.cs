using System;
using System.IO;
using Zoroiscrying.CoreGameSystems.UISystem;
using Zoroiscrying.CoreGameSystems.UISystem.ScriptableObjectIntegration;
using UnityEditor;
using UnityEngine;
using Zoroiscrying.CoreGameSystems.CoreSystemUtility.Editor;

namespace Zoroiscrying.CoreGameSystems.UISystem.Editor
{
    [CustomEditor(typeof(BaseUiPanel))]
    public class BaseUiPanelEditor : UnityEditor.Editor
    {
        private BaseUiPanel _uiBasePanel;

        // Properties for the panel class
        private SerializedProperty _onMotionTypeProperty;
        private SerializedProperty _offMotionTypeProperty;
        private SerializedProperty _panelTypeSOProperty;
        private SerializedProperty _customTweenOrganizerProperty;
        
        // Properties for the AnimatableObject class
        private SerializedProperty _animateEasingTypeProperty;
        private SerializedProperty _loopAnimateProperty;
        private SerializedProperty _pingPongAnimateProperty;

        // Properties for custom tween or tween organizer
        private SerializedProperty _onAnimateTimeProperty;
        private SerializedProperty _offAnimateTimeProperty;
        private SerializedProperty _customPanelAction;
        private SerializedProperty _useTweenOrganizer;

        private SerializedProperty _turnPanelOnAtStart;

        // Custom bool to store the states of the editor.
        private bool enableTweenInspector = true;
        private bool enableAnimatorInspector = true;

        private bool _userChooseToChangeType = false;
        
        // Default SO object creation folder path.
        private static string _defaultPathForBaseUiPanel = "";
        
        // Properties for Animator Organizer
        private SerializedProperty _rectTransformAnimatorOrganizer;
        private SerializedProperty _canvasGroupAnimatorOrganizer;

        protected virtual void OnEnable()
        {
            if (_defaultPathForBaseUiPanel == "")
            {
                var pathList = AssetDatabase.FindAssets("t:PanelTypeSo");
                if (pathList.Length > 0)
                {
                    _defaultPathForBaseUiPanel = AssetDatabase.GUIDToAssetPath(pathList[0]);
                    var index = _defaultPathForBaseUiPanel.LastIndexOf('/');
                    _defaultPathForBaseUiPanel = _defaultPathForBaseUiPanel.Substring(0, index);
                }
                else
                {
                    _defaultPathForBaseUiPanel = "Assets/";
                }
            }
            
            GetSerializedProperty();
            GetTarget();
        }

        public override void OnInspectorGUI()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                serializedObject.Update();

                DrawPanelTypes();
                
                DrawInitialStateInfo();
                
                DrawMotionTypes();

                DrawAnimationConfiguration();

                serializedObject.ApplyModifiedProperties();
            }
        }

        protected virtual void GetSerializedProperty()
        {
            _onMotionTypeProperty = serializedObject.FindProperty("onMotionType");
            _offMotionTypeProperty = serializedObject.FindProperty("offMotionType");
            _panelTypeSOProperty = serializedObject.FindProperty("panelType");
            _customTweenOrganizerProperty = serializedObject.FindProperty("onMotionType");

            _onAnimateTimeProperty = serializedObject.FindProperty("onAnimateTime");
            _offAnimateTimeProperty = serializedObject.FindProperty("offAnimateTime");
            
            _animateEasingTypeProperty = serializedObject.FindProperty("animateEasingType");
            _loopAnimateProperty = serializedObject.FindProperty("loopAnimate");
            _pingPongAnimateProperty = serializedObject.FindProperty("pingPongAnimate");

            _useTweenOrganizer = serializedObject.FindProperty("useTweenOrganizerSO");
            _customPanelAction = serializedObject.FindProperty("customPanelAction");

            _turnPanelOnAtStart = serializedObject.FindProperty("turnPanelOnAtStart");

            _rectTransformAnimatorOrganizer = serializedObject.FindProperty("rectTransformAnimatorOrganizer");
            _canvasGroupAnimatorOrganizer = serializedObject.FindProperty("canvasGroupAnimatorOrganizer");
        }
        
        protected virtual void GetTarget()
        {
            _uiBasePanel = (BaseUiPanel)target;
        }

        protected virtual void DrawInitialStateInfo()
        {
            // Initial states
            CustomEditorLayout.HorizontalLine(Color.white);
            EditorGUILayout.PropertyField(_turnPanelOnAtStart, new GUIContent("Panel On At Start?"));
        }

        protected virtual void DrawMotionTypes()
        {
            // Motion types
            CustomEditorLayout.HorizontalLine(Color.white);
            EditorGUILayout.LabelField("Motion Types", CustomEditorStyles.middleBoldLabel);

            EditorGUILayout.PropertyField(_onMotionTypeProperty);
            EditorGUILayout.PropertyField(_offMotionTypeProperty);

            enableTweenInspector = _onMotionTypeProperty.enumValueIndex == 2 ||
                                       _offMotionTypeProperty.enumValueIndex == 2 ||
                                       _onMotionTypeProperty.enumValueIndex == 3 ||
                                       _offMotionTypeProperty.enumValueIndex == 3;
            enableAnimatorInspector = _onMotionTypeProperty.enumValueIndex == 1 ||
                                          _offMotionTypeProperty.enumValueIndex == 1 ||
                                          _onMotionTypeProperty.enumValueIndex == 3 ||
                                          _offMotionTypeProperty.enumValueIndex == 3;
        }
        
        protected virtual void DrawPanelTypes()
        {
            // Panel types
            CustomEditorLayout.HorizontalLine(Color.white);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Panel Type", CustomEditorStyles.middleBoldLabel);
            DrawScriptField();
            EditorGUILayout.EndHorizontal();

            _panelTypeSOProperty.objectReferenceValue = EditorGUILayout.ObjectField(
                new GUIContent("Panel Type"), _panelTypeSOProperty.objectReferenceValue,
                typeof(PanelTypeSO), false);

            EditorGUILayout.BeginHorizontal();

            if (_panelTypeSOProperty.objectReferenceValue != null)
            {
                EditorGUILayout.LabelField("Current Panel ID: " + _uiBasePanel.PanelTypeSo.Id);
            
                if (!_userChooseToChangeType && GUILayout.Button("Change", EditorStyles.miniButton, GUILayout.MaxWidth(60)))
                {
                    _userChooseToChangeType = true;
                }   
            }

            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            // Panel Settings Custom Change
            if (_userChooseToChangeType && _panelTypeSOProperty.objectReferenceValue != null)
            {
                var panelType = _uiBasePanel.PanelTypeSo;
                panelType.Id = EditorGUILayout.DelayedTextField("New Panel ID: ", panelType.Id);
                
                if (EditorGUI.EndChangeCheck())
                {
                    var id = panelType.Id;
                    Debug.Log(panelType.name);
                    var assetPath = AssetDatabase.GetAssetPath(panelType.GetInstanceID());
                    AssetDatabase.RenameAsset(assetPath, panelType.Id);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    panelType.Id = id;
                    _userChooseToChangeType = false;
                }
            }
            
            CustomPanelTypeDisplay();
        }

        protected virtual void CustomPanelTypeDisplay()
        {
            EditorGUILayout.LabelField("Folder:"+_defaultPathForBaseUiPanel, EditorStyles.wordWrappedMiniLabel);
            
            if (GUILayout.Button("Choose Default Panel Type Folder", EditorStyles.miniButton))
            {
                // Creates a window and let the user choose where to create the file for the SO file.   
                var path = EditorUtility.OpenFolderPanel(
                    "Choose a folder to save the Scriptable Files",
                    _defaultPathForBaseUiPanel,
                    "");
                if (path.Length > 0)
                {
                    var assetIndex = path.IndexOf("Asset", StringComparison.Ordinal);
                    _defaultPathForBaseUiPanel = path.Substring(assetIndex, path.Length - assetIndex);
                }
            }
            
            if (GUILayout.Button("New panel type for this one", EditorStyles.miniButton))
            {
                string path = _defaultPathForBaseUiPanel + "/NewPanelType.asset";
                if (path.Length > 0)
                {
                    PanelTypeSO asset = ScriptableObject.CreateInstance<PanelTypeSO>();
                    path = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(path);
                    Debug.Log("Created panel type at:" + path);

                    var index = path.LastIndexOf('/');
                    var index2 = path.LastIndexOf('.');
                    var assetName = path.Substring(index+1, index2-index-1);

                    AssetDatabase.CreateAsset(asset, path);
                    AssetDatabase.SaveAssets();
                    EditorUtility.FocusProjectWindow();
                    AssetDatabase.Refresh();
                    _panelTypeSOProperty.objectReferenceValue = asset;
                    asset.Id = assetName;
                    EditorGUIUtility.PingObject(asset);
                }
            }
        }
        
        protected virtual void DrawAnimationConfiguration()
        {
            // Animation configuration
            CustomEditorLayout.HorizontalLine(Color.white);
            EditorGUILayout.LabelField("Animation Configuration", CustomEditorStyles.middleBoldLabel);

            if (enableTweenInspector)
            {
                EditorGUILayout.PropertyField(_useTweenOrganizer, new GUIContent("Use Tween Organizer"));
                EditorGUILayout.PropertyField(_onAnimateTimeProperty, new GUIContent("Time To Turn On"));
                EditorGUILayout.PropertyField(_offAnimateTimeProperty, new GUIContent("Time To Turn Off"));
            }

            if (enableTweenInspector && !_useTweenOrganizer.boolValue)
            {
                EditorGUILayout.PropertyField(_animateEasingTypeProperty, new GUIContent("Ease Type"));
                EditorGUILayout.PropertyField(_customPanelAction, new GUIContent("Custom Panel Action"));
            }
            else if (enableTweenInspector && _useTweenOrganizer.boolValue)
            {
                DrawAnimatorOrganizers();
                // Draw the properties of the scriptable tween, and enable changes.
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_loopAnimateProperty);
            EditorGUILayout.PropertyField(_pingPongAnimateProperty);
            EditorGUILayout.EndHorizontal();
        }

        protected virtual void DrawAnimatorOrganizers()
        {
            // Draw the custom scriptable tween object.
            EditorGUILayout.PropertyField(_rectTransformAnimatorOrganizer, new GUIContent("RectTransform Tween Organizer"));
            EditorGUILayout.PropertyField(_canvasGroupAnimatorOrganizer, new GUIContent("CanvasGroup Tween Organizer"));
        }
        
        protected virtual void DrawScriptField()
        {
            // Disable editing
            EditorGUI.BeginDisabledGroup(true); 
            EditorGUILayout.ObjectField("", MonoScript.FromMonoBehaviour((BaseUiPanel) target), typeof(BaseUiPanel), false, GUILayout.MaxWidth(160));
            EditorGUI.EndDisabledGroup();
        }
    }
}
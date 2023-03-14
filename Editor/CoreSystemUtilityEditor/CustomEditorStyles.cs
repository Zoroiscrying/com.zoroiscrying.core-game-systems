using UnityEditor;
using UnityEngine;

namespace Zoroiscrying.CoreGameSystems.CoreSystemUtility.Editor
{
    public sealed class CustomEditorStyles
    {
        public static GUIStyle middleBoldLabel = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
        };

        public static GUIStyle horizontalLine = new GUIStyle(EditorStyles.miniButton)
        {
            normal =
            {
               background = EditorGUIUtility.whiteTexture,
            },
            margin = new RectOffset(0, 0, 4, 4),
            fixedHeight = 1,
        };

        public static GUIStyle middleMinorBoldLabel = new GUIStyle(EditorStyles.miniBoldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
        };
    }
}
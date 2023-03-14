using UnityEngine;

namespace Zoroiscrying.CoreGameSystems.CoreSystemUtility.Editor
{
    public sealed class CustomEditorLayout
    {
        public static void HorizontalLine(Color color)
        {
            var c = GUI.color;
            GUI.color = color;
            GUILayout.Box( GUIContent.none, CustomEditorStyles.horizontalLine );
            GUI.color = c;
        }
    }
}
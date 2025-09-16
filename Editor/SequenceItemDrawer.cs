using UnityEditor;
using UnityEngine;

namespace com.fscigliano.SequentialEvents.Editor
{
    [CustomPropertyDrawer(typeof(SequenceItem))]
    public class SequenceItemDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Color backgroundColor = GUI.backgroundColor;
            EditorGUI.BeginProperty(position, label, property);

            float sLine = EditorGUIUtility.singleLineHeight + 4;

            GUI.Box(position, GUIContent.none, EditorStyles.helpBox);
            position.x += 1; position.y += 1;
            position.width -= 2; position.height -= 2;

            position.x += 8; position.width -= 8;
            float halfWidth = position.width / 2;

            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 60;

            var enabledProp = property.FindPropertyRelative("enabled");
            var colorProp = property.FindPropertyRelative("color");
            var asyncProp = property.FindPropertyRelative("async");
            var realtimeProp = property.FindPropertyRelative("realtime");
            var descProp = property.FindPropertyRelative("description");
            var actionProp = asyncProp.boolValue ? property.FindPropertyRelative("actionAsync") : property.FindPropertyRelative("action");
            var waitBeforeProp = property.FindPropertyRelative("waitBefore");
            var waitAfterProp = property.FindPropertyRelative("waitAfter");

            Rect enabledRect = new Rect(position.x, position.y, halfWidth - 10, sLine);
            Rect colorRect = new Rect(position.x + halfWidth, position.y, halfWidth, sLine);
            Rect asyncRect = new Rect(position.x, position.y + sLine, halfWidth - 10, sLine);
            Rect realtimeRect = new Rect(position.x + halfWidth, position.y + sLine, halfWidth, sLine);

            EditorGUI.PropertyField(enabledRect, enabledProp, new GUIContent("Enabled"));
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(colorRect, colorProp, new GUIContent("Color"));
            if (EditorGUI.EndChangeCheck())
            {
                var c = colorProp.colorValue; c.a = 1f; colorProp.colorValue = c;
            }
            EditorGUI.PropertyField(asyncRect, asyncProp, new GUIContent("Async"));
            EditorGUI.PropertyField(realtimeRect, realtimeProp, new GUIContent("Realtime"));

            float descHeight = EditorGUI.GetPropertyHeight(descProp);
            Rect descRect = new Rect(position.x, position.y + (sLine * 2), position.width, descHeight);
            EditorGUI.PropertyField(descRect, descProp, new GUIContent("Description"));

            EditorGUIUtility.labelWidth = 100;
            Rect waitBeforeRect = new Rect(position.x, position.y + (sLine * 2) + descHeight, halfWidth - 10, sLine);
            Rect waitAfterRect = new Rect(position.x + halfWidth, position.y + (sLine * 2) + descHeight, halfWidth, sLine);

            EditorGUI.PropertyField(waitBeforeRect, waitBeforeProp, new GUIContent("Wait Before"));
            EditorGUI.PropertyField(waitAfterRect, waitAfterProp, new GUIContent("Wait After"));

            float actionHeight = EditorGUI.GetPropertyHeight(actionProp);
            Rect actionRect = new Rect(position.x, position.y + (sLine * 3) + descHeight + 10, position.width, actionHeight);
            EditorGUI.PropertyField(actionRect, actionProp, new GUIContent(asyncProp.boolValue ? "Async Action" : "Action"));

            EditorGUIUtility.labelWidth = labelWidth;
            GUI.backgroundColor = backgroundColor;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float sLine = EditorGUIUtility.singleLineHeight + 2;
            float descHeight = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("description"));
            bool isAsync = property.FindPropertyRelative("async").boolValue;
            float actionHeight = EditorGUI.GetPropertyHeight(isAsync ? property.FindPropertyRelative("actionAsync") : property.FindPropertyRelative("action"));
            return descHeight + actionHeight + (sLine * 3) + 15;
        }
    }
}

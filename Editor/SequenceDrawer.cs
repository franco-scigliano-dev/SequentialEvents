using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace com.fscigliano.SequentialEvents.Editor
{
    [CustomPropertyDrawer(typeof(Sequence))]
    public class SequenceDrawer : PropertyDrawer
    {
        private class ViewData
        {
            public ReorderableList list;
            public SerializedProperty listProp;
            public List<SequenceItem> itemsList;
        }

        private Dictionary<string, ViewData> _viewDatas = new();
        private ViewData viewData;
        private static Texture2D selectedTexture, executingTexture, timeTexture;
        private float listItemHeight = 24;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            GUI.Box(position, GUIContent.none, EditorStyles.helpBox);
            position.x += 4; position.y += 4; position.width -= 8; position.height -= 8;

            GetViewData(property);

            float listHeight = viewData.list.GetHeight();
            Rect listRect = new Rect(position.x, position.y, position.width, listHeight);
            viewData.list.draggable = viewData.list.displayAdd = viewData.list.displayRemove = !Application.isPlaying;
            viewData.list.DoList(listRect);

            SerializedProperty onFinishProp = property.FindPropertyRelative("_onFinishEvt");
            float evtH = EditorGUI.GetPropertyHeight(onFinishProp);
            Rect evtRect = new Rect(position.x, listRect.y + listHeight + 10, position.width, evtH);
            EditorGUI.PropertyField(evtRect, onFinishProp);

            if (viewData.list.index != -1 && viewData.list.index < viewData.listProp.arraySize)
            {
                SerializedProperty item = viewData.listProp.GetArrayElementAtIndex(viewData.list.index);
                float itemHeight = EditorGUI.GetPropertyHeight(item);
                Rect labelRect = new Rect(position.x, evtRect.y + evtH, position.width, 20);
                GUI.Label(labelRect, "Selected Item:");
                Rect itemRect = new Rect(position.x, labelRect.y + 22, position.width, itemHeight);
                EditorGUI.PropertyField(itemRect, item, true);
            }

            EditorGUI.EndProperty();
        }

        private void GetViewData(SerializedProperty property)
        {
            if (!_viewDatas.TryGetValue(property.propertyPath, out viewData))
            {
                viewData = new ViewData();
                _viewDatas[property.propertyPath] = viewData;
                viewData.listProp = property.FindPropertyRelative("_items");
                viewData.itemsList = ReflectionExtensions.Editor.ReflectionExtensions.GetTargetObjectOfProperty(viewData.listProp) as List<SequenceItem>;

                InitTextures();

                viewData.list = new ReorderableList(viewData.listProp.serializedObject, viewData.listProp)
                {
                    drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Items"),
                    elementHeightCallback = index => listItemHeight,
                    onRemoveCallback = l =>
                    {
                        if (l.index >= 0 && l.index < l.count)
                            ReorderableList.defaultBehaviours.DoRemoveButton(l);
                        if (l.count == 0) l.index = -1;
                    },
                    drawElementCallback = (rect, index, active, focused) =>
                    {
                        var itemProp = viewData.listProp.GetArrayElementAtIndex(index);
                        var item = viewData.itemsList[index];
                        Color bc = GUI.backgroundColor;

                        if (Application.isPlaying && item.isCurrent)
                        {
                            Rect r = new Rect(rect.x - 4, rect.y, rect.width + 8, rect.height);
                            GUI.DrawTexture(r, item.isExecuting ? executingTexture : selectedTexture);
                            GUI.backgroundColor = item.isExecuting ? Color.blue : Color.cyan;
                            GUI.Box(r, GUIContent.none, EditorStyles.helpBox);
                        }
                        else
                        {
                            GUI.backgroundColor = itemProp.FindPropertyRelative("color").colorValue;
                            GUI.Box(rect, GUIContent.none, EditorStyles.helpBox);
                        }

                        GUI.backgroundColor = bc;
                        Rect descRect = new Rect(rect.x + 20, rect.y + 3, rect.width * 0.5f - 20, 18);
                        EditorGUI.TextField(descRect, itemProp.FindPropertyRelative("description").stringValue);

                        Rect beforeRect = new Rect(rect.x + rect.width * 0.5f + 10, rect.y + 3, rect.width * 0.25f - 15, 18);
                        Rect afterRect = new Rect(rect.x + rect.width * 0.75f + 5, rect.y + 3, rect.width * 0.25f - 15, 18);
                        GUI.Box(beforeRect, GUIContent.none, EditorStyles.helpBox);
                        GUI.Box(afterRect, GUIContent.none, EditorStyles.helpBox);

                        GUI.Label(beforeRect, $"wait before: {itemProp.FindPropertyRelative("waitBefore").floatValue}");
                        GUI.Label(afterRect, $"wait after: {itemProp.FindPropertyRelative("waitAfter").floatValue}");

                        if (Application.isPlaying && item.isCurrent)
                        {
                            float bw = item.waitBeforeProgress * beforeRect.width;
                            float aw = item.waitAfterProgress * afterRect.width;

                            if (item.isWaitingBefore) GUI.DrawTexture(new Rect(beforeRect.x, beforeRect.y, bw, beforeRect.height), timeTexture);
                            if (item.isWaitingAfter) GUI.DrawTexture(new Rect(afterRect.x, afterRect.y, aw, afterRect.height), timeTexture);
                        }
                    }
                };
            }
        }

        private static void InitTextures()
        {
            if (selectedTexture != null) return;

            selectedTexture = CreateTexture(new Color(0, 1, 1, 0.25f));
            executingTexture = CreateTexture(new Color(0, 0, 1, 0.25f));
            timeTexture = CreateTexture(new Color(0, 1, 0, 0.75f));
        }

        private static Texture2D CreateTexture(Color c)
        {
            var tex = new Texture2D(2, 2);
            tex.SetPixels(Enumerable.Repeat(c, 4).ToArray());
            tex.Apply();
            return tex;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            GetViewData(property);
            float h = 70 + viewData.list.GetHeight();

            if (viewData.list.index != -1 && viewData.list.index < viewData.listProp.arraySize)
                h += EditorGUI.GetPropertyHeight(viewData.listProp.GetArrayElementAtIndex(viewData.list.index)) + 22;

            h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("_onFinishEvt")) + 10;
            return h;
        }
    }
}

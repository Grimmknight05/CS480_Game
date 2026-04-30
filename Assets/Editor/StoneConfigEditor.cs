using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(StoneConfiguration))]
public class StoneConfigurationEditor : Editor
{
    private ReorderableList list;

    private void OnEnable()
    {
        list = new ReorderableList(
            serializedObject,
            serializedObject.FindProperty("requirements"),
            true, true, true, true
        );

        // Header
        list.drawHeaderCallback = rect =>
        {
            EditorGUI.LabelField(rect, "Requirements");
        };

        // Draw each item
        list.drawElementCallback = (rect, index, active, focused) =>
        {
            var element = list.serializedProperty.GetArrayElementAtIndex(index);

            EditorGUI.PropertyField(
                rect,
                element,
                GUIContent.none,
                true
            );
        };

        // Auto height (important)
        list.elementHeightCallback = index =>
        {
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(element, true) + 4;
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        list.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }
}
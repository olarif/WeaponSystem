using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProjectileController))]
public class ProjectileControllerEditor : Editor
{
    SerializedProperty pSpeed;
    SerializedProperty pLifetime;
    SerializedProperty pUseGravity;
    SerializedProperty pActions;

    bool showActions = true;

    void OnEnable()
    {
        pSpeed       = serializedObject.FindProperty("speed");
        pLifetime    = serializedObject.FindProperty("lifetime");
        pUseGravity  = serializedObject.FindProperty("useGravity");
        pActions     = serializedObject.FindProperty("onHitActions");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Default fields
        EditorGUILayout.PropertyField(pSpeed);
        EditorGUILayout.PropertyField(pLifetime);
        EditorGUILayout.PropertyField(pUseGravity);

        EditorGUILayout.Space();
        showActions = EditorGUILayout.Foldout(showActions, "On Hit Actions");
        if (showActions)
        {
            EditorGUI.indentLevel++;

            // Draw existing actions
            for (int i = 0; i < pActions.arraySize; i++)
            {
                var elem = pActions.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginHorizontal();
                // Draw the inline data (polymorphic)
                EditorGUILayout.PropertyField(elem, new GUIContent(elem.managedReferenceFullTypename), true);
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    pActions.DeleteArrayElementAtIndex(i);
                    serializedObject.ApplyModifiedProperties();
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add DealDamageOnHit"))
                AddAction<DealDamageOnHitData>();
            if (GUILayout.Button("Add SpawnOnHit"))
                AddAction<SpawnOnHit>();
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void AddAction<T>() where T : ProjectileActionData
    {
        serializedObject.Update();

        // Append new element
        int idx = pActions.arraySize;
        pActions.InsertArrayElementAtIndex(idx);

        // Instantiate the managed reference
        var elem = pActions.GetArrayElementAtIndex(idx);
        var instance = (ProjectileActionData)System.Activator.CreateInstance(typeof(T));
        elem.managedReferenceValue = instance;

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }
}

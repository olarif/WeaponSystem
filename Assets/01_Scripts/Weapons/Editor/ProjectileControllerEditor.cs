using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProjectileController))]
public class ProjectileControllerEditor : Editor
{
    SerializedProperty pHitLayer;
    SerializedProperty pSpeed;
    SerializedProperty pLifetime;
    SerializedProperty pUseGravity;
    SerializedProperty pComponents;

    bool showComponents = true;
    List<Type> componentTypes;

    void OnEnable()
    {
        // grab built-in fields
        pHitLayer   = serializedObject.FindProperty(nameof(ProjectileController.hitLayer));
        pSpeed      = serializedObject.FindProperty(nameof(ProjectileController.speed));
        pLifetime   = serializedObject.FindProperty(nameof(ProjectileController.lifetime));
        pUseGravity = serializedObject.FindProperty(nameof(ProjectileController.useGravity));

        // managed-reference list on ProjectileController
        pComponents = serializedObject.FindProperty(nameof(ProjectileController.components));
        if (pComponents == null)
            Debug.LogError("Could not find 'components' on ProjectileController!");

        // cache all concrete ProjectileComponent types
        componentTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsSubclassOf(typeof(ProjectileComponent)) && !t.IsAbstract)
            .OrderBy(t => t.Name)
            .ToList();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // draw the regular fields
        EditorGUILayout.PropertyField(pHitLayer);
        EditorGUILayout.PropertyField(pSpeed);
        EditorGUILayout.PropertyField(pLifetime);
        EditorGUILayout.PropertyField(pUseGravity);

        EditorGUILayout.Space();
        showComponents = EditorGUILayout.Foldout(showComponents, "Projectile Components", true);
        if (showComponents && pComponents != null)
        {
            EditorGUI.indentLevel++;
            // list existing
            for (int i = 0; i < pComponents.arraySize; i++)
            {
                var elem = pComponents.GetArrayElementAtIndex(i);
                // derive a friendly type name
                string full = elem.managedReferenceFullTypename;
                string nicename = "None";
                if (!string.IsNullOrEmpty(full))
                {
                    var afterSpace = full.Substring(full.IndexOf(' ') + 1);
                    nicename = afterSpace.Split('.').Last();
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(elem, new GUIContent(nicename), true);
                // remove button
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    // safe remove of managedReference
                    pComponents.DeleteArrayElementAtIndex(i);
                    pComponents.DeleteArrayElementAtIndex(i);
                    serializedObject.ApplyModifiedProperties();
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            // add-new dropdown
            if (GUILayout.Button("Add Component"))
            {
                var menu = new GenericMenu();
                foreach (var t in componentTypes)
                {
                    // split camelCase for readability
                    string label = System.Text.RegularExpressions.Regex
                        .Replace(t.Name, "(?<!^)([A-Z])", " $1");
                    menu.AddItem(new GUIContent(label), false, () => {
                        AddComponent(t);
                    });
                }
                menu.ShowAsContext();
            }

            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void AddComponent(Type type)
    {
        serializedObject.Update();

        // grow the array
        int idx = pComponents.arraySize;
        pComponents.arraySize++;
        serializedObject.ApplyModifiedProperties();
        serializedObject.Update();

        // assign a fresh instance
        var elem = pComponents.GetArrayElementAtIndex(idx);
        elem.managedReferenceValue = Activator.CreateInstance(type);

        serializedObject.ApplyModifiedProperties();
        // mark dirty so scene knows to save
        EditorUtility.SetDirty(target);
    }
}

using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[CustomEditor(typeof(ProjectileController))]
public class ProjectileControllerEditor : Editor
{
    SerializedProperty pHitLayer;
    SerializedProperty pSpeed;
    SerializedProperty pLifetime;
    SerializedProperty pUseGravity;
    SerializedProperty pActions;

    bool showActions = true;
    List<Type> actionTypes;

    void OnEnable()
    {
        pHitLayer   = serializedObject.FindProperty("hitLayer");
        pSpeed      = serializedObject.FindProperty("speed");
        pLifetime   = serializedObject.FindProperty("lifetime");
        pUseGravity = serializedObject.FindProperty("useGravity");
        pActions    = serializedObject.FindProperty("onHitActions");

        RefreshActionTypes();
    }

    void RefreshActionTypes()
    {
        actionTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsSubclassOf(typeof(ProjectileActionData)) && !t.IsAbstract)
            .ToList();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Default fields
        EditorGUILayout.PropertyField(pHitLayer);
        EditorGUILayout.PropertyField(pSpeed);
        EditorGUILayout.PropertyField(pLifetime);
        EditorGUILayout.PropertyField(pUseGravity);

        EditorGUILayout.Space();
        showActions = EditorGUILayout.Foldout(showActions, "On Hit Actions", true);
        if (showActions)
        {
            EditorGUI.indentLevel++;

            // Draw existing actions
            for (int i = 0; i < pActions.arraySize; i++)
            {
                var elem = pActions.GetArrayElementAtIndex(i);
                string fullTypeName = elem.managedReferenceFullTypename;
                string afterSpace = fullTypeName.Substring(fullTypeName.LastIndexOf(' ') + 1);
                string typeName = afterSpace.Contains('.') ? afterSpace.Substring(afterSpace.LastIndexOf('.') + 1) : afterSpace;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(elem, new GUIContent(typeName), true);
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
            if (EditorGUILayout.DropdownButton(new GUIContent("Add Action"), FocusType.Passive))
            {
                var menu = new GenericMenu();
                foreach (var t in actionTypes)
                {
                    // pretty name: split camelcase
                    string menuName = System.Text.RegularExpressions.Regex.Replace(t.Name, "(?<!^)([A-Z])", " $1");
                    menu.AddItem(new GUIContent(menuName), false, () => AddAction(t));
                }
                menu.ShowAsContext();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void AddAction(Type actionType)
    {
        serializedObject.Update();
        int idx = pActions.arraySize;
        pActions.InsertArrayElementAtIndex(idx);
        var elem = pActions.GetArrayElementAtIndex(idx);
        elem.managedReferenceValue = Activator.CreateInstance(actionType);
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }
}

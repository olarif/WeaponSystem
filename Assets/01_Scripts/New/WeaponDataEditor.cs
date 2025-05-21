using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[CustomEditor(typeof(WeaponDataSO))]
public class WeaponDataEditor : Editor
{
    SerializedProperty pName, pDescription, pRightHandModel, pLeftHandModel,pBindings;
    SerializedProperty pHand, pModelPositionOffset, pModelRotationOffset;
    bool showBindings = true;

    // Keep track of foldouts per binding
    List<bool> bindingFoldouts = new List<bool>();

    // All concrete WeaponActionData subclasses
    List<Type> actionTypes;

    void OnEnable()
    {
        pName                 = serializedObject.FindProperty("weaponName");
        pDescription          = serializedObject.FindProperty("weaponDescription");
        pRightHandModel       = serializedObject.FindProperty("rightHandModel");
        pLeftHandModel        = serializedObject.FindProperty("leftHandModel");
        pHand                 = serializedObject.FindProperty("defaultHand");
        pModelPositionOffset  = serializedObject.FindProperty("modelPositionOffset");
        pModelRotationOffset  = serializedObject.FindProperty("modelRotationOffset");
        pBindings             = serializedObject.FindProperty("bindings");

        SyncFoldouts();
        RefreshActionTypes();
    }

    void SyncFoldouts()
    {
        while (bindingFoldouts.Count < pBindings.arraySize)
            bindingFoldouts.Add(true);
        while (bindingFoldouts.Count > pBindings.arraySize)
            bindingFoldouts.RemoveAt(bindingFoldouts.Count - 1);
    }

    void RefreshActionTypes()
    {
        // Load all non-abstract subclasses of WeaponActionData
        actionTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsSubclassOf(typeof(WeaponActionData)) && !t.IsAbstract)
            .ToList();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        SyncFoldouts();

        EditorGUILayout.PropertyField(pName, new GUIContent("Weapon Name"));
        EditorGUILayout.PropertyField(pDescription, new GUIContent("Weapon Description"));
        EditorGUILayout.PropertyField(pRightHandModel, new GUIContent("Right Hand Model"));
        EditorGUILayout.PropertyField(pLeftHandModel, new GUIContent("Left Hand Model"));
        EditorGUILayout.PropertyField(pHand, new GUIContent("Hands to use"));
        EditorGUILayout.PropertyField(pModelPositionOffset, new GUIContent("Model Position Offset"));
        EditorGUILayout.PropertyField(pModelRotationOffset, new GUIContent("Model Rotation Offset"));

        showBindings = EditorGUILayout.Foldout(showBindings, "Input Bindings", true);
        if (showBindings)
        {
            EditorGUILayout.Space();

            if (GUILayout.Button("Add Binding"))
            {
                serializedObject.Update();
                int newIndex = pBindings.arraySize;
                pBindings.InsertArrayElementAtIndex(newIndex);

                var newBP = pBindings.GetArrayElementAtIndex(newIndex);
                newBP.FindPropertyRelative("eventType").enumValueIndex = 0;
                newBP.FindPropertyRelative("inputAction").objectReferenceValue = null;
                newBP.FindPropertyRelative("fireHand").enumValueIndex = 0;
                newBP.FindPropertyRelative("holdTime").floatValue = 0f;
                newBP.FindPropertyRelative("fireRate").floatValue = 0f;
                newBP.FindPropertyRelative("actions").ClearArray();

                serializedObject.ApplyModifiedProperties();
                SyncFoldouts();
            }

            for (int i = 0; i < pBindings.arraySize; i++)
            {
                var bp      = pBindings.GetArrayElementAtIndex(i);
                var eType   = bp.FindPropertyRelative("eventType");
                var iAction = bp.FindPropertyRelative("inputAction");
                var fHand   = bp.FindPropertyRelative("fireHand");
                var hTime   = bp.FindPropertyRelative("holdTime");
                var rRate   = bp.FindPropertyRelative("fireRate");
                var actions = bp.FindPropertyRelative("actions");

                string header = ((WeaponInputEvent)eType.enumValueIndex).ToString();
                if (iAction.objectReferenceValue != null)
                    header += " → " + iAction.objectReferenceValue.name;

                bindingFoldouts[i] = EditorGUILayout.Foldout(bindingFoldouts[i], header, true);
                if (!bindingFoldouts[i])
                    continue;

                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical(GUI.skin.box);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Remove Binding", GUILayout.MaxWidth(140)))
                {
                    pBindings.DeleteArrayElementAtIndex(i);
                    serializedObject.ApplyModifiedProperties();
                    SyncFoldouts();
                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;
                    break;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(eType, new GUIContent("Event Type"));
                EditorGUILayout.PropertyField(iAction, new GUIContent("Input Action"));
                EditorGUILayout.PropertyField(fHand, new GUIContent("Fire Hand"));
                switch ((WeaponInputEvent)eType.enumValueIndex)
                {
                    case WeaponInputEvent.Press:
                        EditorGUILayout.PropertyField(rRate, new GUIContent("Cooldown (s)"));
                        break;
                    case WeaponInputEvent.Hold:
                        EditorGUILayout.PropertyField(hTime, new GUIContent("Hold Time (s)"));
                        break;
                    case WeaponInputEvent.Continuous:
                        EditorGUILayout.PropertyField(rRate, new GUIContent("Fire Rate (s)"));
                        break;
                    
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Actions:", EditorStyles.boldLabel);

                // Existing actions list
                for (int j = 0; j < actions.arraySize; j++)
                {
                    var ap = actions.GetArrayElementAtIndex(j);
                    string fullTypeName = ap.managedReferenceFullTypename;
                    
                    var afterSpace = fullTypeName.Substring(fullTypeName.LastIndexOf(' ') + 1);
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(ap, new GUIContent(afterSpace), true);
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        actions.DeleteArrayElementAtIndex(j);
                        serializedObject.ApplyModifiedProperties();
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                }

                // Dropdown for adding actions
                EditorGUILayout.Space();
                if (EditorGUILayout.DropdownButton(new GUIContent("Add Action"), FocusType.Passive))
                {
                    var menu = new GenericMenu();
                    foreach (var t in actionTypes)
                    {
                        string name = t.Name;
                        name = System.Text.RegularExpressions.Regex.Replace(name, "(?<!^)([A-Z])", " $1").Replace(" Action", "");
                        menu.AddItem(new GUIContent(name), false, () => AddInline(t, actions));
                    }
                    menu.ShowAsContext();
                }

                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void AddInline(Type actionType, SerializedProperty listProp)
    {
        serializedObject.Update();
        int idx = listProp.arraySize;
        listProp.InsertArrayElementAtIndex(idx);
        var elem = listProp.GetArrayElementAtIndex(idx);
        elem.managedReferenceValue = Activator.CreateInstance(actionType);
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }
}

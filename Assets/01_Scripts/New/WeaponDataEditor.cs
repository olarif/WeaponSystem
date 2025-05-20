// Assets/Editor/WeaponDataEditor.cs
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(WeaponDataSO))]
public class WeaponDataEditor : Editor
{
    SerializedProperty pName, pBindings;
    bool showBindings = true;

    // Keep track of foldouts per binding
    List<bool> bindingFoldouts = new List<bool>();

    void OnEnable()
    {
        pName     = serializedObject.FindProperty("weaponName");
        pBindings = serializedObject.FindProperty("bindings");
        SyncFoldouts();
    }

    void SyncFoldouts()
    {
        // Make sure we have one bool per binding
        while (bindingFoldouts.Count < pBindings.arraySize)
            bindingFoldouts.Add(true);
        while (bindingFoldouts.Count > pBindings.arraySize)
            bindingFoldouts.RemoveAt(bindingFoldouts.Count - 1);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        SyncFoldouts();

        EditorGUILayout.PropertyField(pName, new GUIContent("Weapon Name"));

        showBindings = EditorGUILayout.Foldout(showBindings, "Input Bindings", true);
        if (showBindings)
        {
            EditorGUILayout.Space();

            // ▸ Add Binding Button
            if (GUILayout.Button("Add Binding"))
            {
                serializedObject.Update();
                int newIndex = pBindings.arraySize;          // position at end
                pBindings.InsertArrayElementAtIndex(newIndex);

                // Clear the cloned data so we start fresh
                var newBP = pBindings.GetArrayElementAtIndex(newIndex);
                newBP.FindPropertyRelative("eventType").enumValueIndex = 0;
                newBP.FindPropertyRelative("inputAction").objectReferenceValue = null;
                newBP.FindPropertyRelative("holdTime").floatValue = 0f;
                newBP.FindPropertyRelative("fireRate").floatValue = 0f;
                var newActions = newBP.FindPropertyRelative("actions");
                newActions.ClearArray();

                serializedObject.ApplyModifiedProperties();
                SyncFoldouts();
            }

            // Iterate bindings
            for (int i = 0; i < pBindings.arraySize; i++)
            {
                var bp      = pBindings.GetArrayElementAtIndex(i);
                var eType   = bp.FindPropertyRelative("eventType");
                var iAction = bp.FindPropertyRelative("inputAction");
                var hTime   = bp.FindPropertyRelative("holdTime");
                var rRate   = bp.FindPropertyRelative("fireRate");
                var actions = bp.FindPropertyRelative("actions");

                // Header shows event and action name
                string header = ((WeaponInputEvent)eType.enumValueIndex).ToString();
                if (iAction.objectReferenceValue != null)
                    header += " → " + iAction.objectReferenceValue.name;

                bindingFoldouts[i] = EditorGUILayout.Foldout(bindingFoldouts[i], header, true);
                if (!bindingFoldouts[i])
                    continue;

                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical(GUI.skin.box);

                // Remove this binding
                if (GUILayout.Button("Remove Binding", GUILayout.MaxWidth(140)))
                {
                    pBindings.DeleteArrayElementAtIndex(i);
                    serializedObject.ApplyModifiedProperties();
                    SyncFoldouts();
                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;
                    break;
                }

                // Binding fields
                EditorGUILayout.PropertyField(eType,   new GUIContent("Event Type"));
                EditorGUILayout.PropertyField(iAction, new GUIContent("Input Action"));
                switch ((WeaponInputEvent)eType.enumValueIndex)
                {
                    case WeaponInputEvent.Hold:
                        EditorGUILayout.PropertyField(hTime, new GUIContent("Hold Time (s)"));
                        break;
                    case WeaponInputEvent.Continuous:
                        EditorGUILayout.PropertyField(rRate, new GUIContent("Fire Rate (s)"));
                        break;
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Actions:", EditorStyles.boldLabel);

                // Draw existing actions
                for (int j = 0; j < actions.arraySize; j++)
                {
                    var ap = actions.GetArrayElementAtIndex(j);
                    // Get clean type name instead of assembly name
                    string fullType  = ap.managedReferenceFieldTypename;
                    string typeName  = fullType.Substring(fullType.LastIndexOf(' ') + 1);

                    EditorGUILayout.BeginHorizontal();
                    // Inline drawer
                    EditorGUILayout.PropertyField(ap, new GUIContent(typeName), true);
                    // Remove button
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        actions.DeleteArrayElementAtIndex(j);
                        serializedObject.ApplyModifiedProperties();
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                }

                // Add-action buttons (never cloned across bindings)
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Deal Melee Damage"))   AddInline<DealDamageAction>(actions);
                if (GUILayout.Button("Spawn VFX"))            AddInline<CreateVFXAction>(actions);
                if (GUILayout.Button("Spawn Projectile"))    AddInline<SpawnProjectileAction>(actions);
                if (GUILayout.Button("Player Force"))         AddInline<PlayerForceAction>(actions);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void AddInline<T>(SerializedProperty listProp) where T : WeaponActionData
    {
        serializedObject.Update();
        int idx = listProp.arraySize;
        listProp.InsertArrayElementAtIndex(idx);
        var elem = listProp.GetArrayElementAtIndex(idx);
        // Create a brand-new instance, never shared
        elem.managedReferenceValue = System.Activator.CreateInstance(typeof(T));
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }
}

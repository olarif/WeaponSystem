// Assets/Editor/WeaponDataSOEditor.cs
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WeaponDataSO))]
public class WeaponDataEditor : Editor
{
    // Top‐level SO fields
    SerializedProperty pName, pDesc;
    SerializedProperty pRightModel, pLeftModel, pPickupPrefab;
    SerializedProperty pModelPos, pModelRot, pDefaultHand;

    // Bindings list
    SerializedProperty pBindings;
    string _bindingsFieldName;

    // All your IWeaponAction types
    List<Type> _actionTypes;

    void OnEnable()
    {
        serializedObject.FindProperty(""); // force serialization init

        // Grab your fields
        pName = serializedObject.FindProperty("weaponName");
        pDesc = serializedObject.FindProperty("weaponDescription");
        pRightModel = serializedObject.FindProperty("rightHandModel");
        pLeftModel = serializedObject.FindProperty("leftHandModel");
        pPickupPrefab = serializedObject.FindProperty("pickupPrefab");
        pModelPos = serializedObject.FindProperty("modelPositionOffset");
        pModelRot = serializedObject.FindProperty("modelRotationOffset");
        pDefaultHand = serializedObject.FindProperty("defaultHand");

        // Try both possible names
        pBindings = serializedObject.FindProperty("inputBindings");
        _bindingsFieldName = "inputBindings";
        if (pBindings == null)
        {
            pBindings = serializedObject.FindProperty("bindings");
            _bindingsFieldName = "bindings";
        }

        // Cache action types
        _actionTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IWeaponAction).IsAssignableFrom(t)
                        && !t.IsInterface && !t.IsAbstract)
            .OrderBy(t => t.Name)
            .ToList();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // --- Top‐level fields ---
        EditorGUILayout.PropertyField(pName, new GUIContent("Name"));
        EditorGUILayout.PropertyField(pDesc, new GUIContent("Description"));

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(pRightModel, new GUIContent("Right Model"));
        EditorGUILayout.PropertyField(pLeftModel, new GUIContent("Left Model"));
        EditorGUILayout.PropertyField(pPickupPrefab, new GUIContent("Pickup Prefab"));

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(pModelPos, new GUIContent("Model Position"));
        EditorGUILayout.PropertyField(pModelRot, new GUIContent("Model Rotation"));
        EditorGUILayout.PropertyField(pDefaultHand, new GUIContent("Default Hand"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Input Bindings", EditorStyles.boldLabel);

        if (pBindings == null)
        {
            EditorGUILayout.HelpBox(
                $"Could not find a List<InputBindingData> named '{_bindingsFieldName}'.",
                MessageType.Error
            );
        }
        else
        {
            DrawBindings();
        }

        serializedObject.ApplyModifiedProperties();
    }

    void DrawBindings()
    {
        // Add new input-binding
        if (GUILayout.Button("Add Binding", GUILayout.MaxWidth(120)))
        {
            Undo.RecordObject(target, "Add Input Binding");
            pBindings.arraySize++;
            serializedObject.ApplyModifiedProperties();
        }

        // Iterate existing bindings
        for (int bi = 0; bi < pBindings.arraySize; bi++)
        {
            var bindProp = pBindings.GetArrayElementAtIndex(bi);
            bindProp.isExpanded = EditorGUILayout.Foldout(
                bindProp.isExpanded, $"Binding #{bi + 1}", true);
            if (!bindProp.isExpanded) continue;

            EditorGUI.indentLevel++;
            var modeProp = bindProp.FindPropertyRelative("bindingMode");
            if (modeProp != null)
                EditorGUILayout.PropertyField(
                    modeProp,
                    new GUIContent("Binding Mode"));
            EditorGUILayout.PropertyField(
                bindProp.FindPropertyRelative("actionRef"),
                new GUIContent("Input Action"));
            EditorGUILayout.PropertyField(
                bindProp.FindPropertyRelative("hand"),
                new GUIContent("Hand"));
            EditorGUILayout.PropertyField(
                bindProp.FindPropertyRelative("holdTime"),
                new GUIContent("Hold Time"));
            var cooldownProp = bindProp.FindPropertyRelative("cooldown");
            if (cooldownProp != null)
                EditorGUILayout.PropertyField(
                    cooldownProp,
                    new GUIContent("Input Cooldown"));

            // Remove binding (single pass)
            if (GUILayout.Button("Remove Binding", GUILayout.MaxWidth(140)))
            {
                Undo.RecordObject(target, "Remove Input Binding");
                pBindings.DeleteArrayElementAtIndex(bi);
                serializedObject.ApplyModifiedProperties();
                EditorGUI.indentLevel--;
                break;
            }

            // Actions list inside this binding
            var actionsProp = bindProp.FindPropertyRelative("bindings");
            if (actionsProp == null)
            {
                EditorGUILayout.HelpBox(
                    "Missing 'bindings' list!", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(
                    "Actions", EditorStyles.boldLabel);

                // Show context menu to add a new action
                if (GUILayout.Button("Add Action", GUILayout.MaxWidth(120)))
                    ShowAddActionMenu(bi);

                // Iterate existing actions
                for (int ai = 0; ai < actionsProp.arraySize; ai++)
                {
                    var actProp = actionsProp.GetArrayElementAtIndex(ai);
                    var typeProp = actProp.FindPropertyRelative("action");
                    string typeName = typeProp != null
                        ? GetNiceManagedTypeName(typeProp)
                        : $"Action {ai + 1}";

                    actProp.isExpanded = EditorGUILayout.Foldout(
                        actProp.isExpanded, typeName, true);
                    if (!actProp.isExpanded) continue;

                    EditorGUI.indentLevel++;
                    // Trigger phase
                    EditorGUILayout.PropertyField(
                        actProp.FindPropertyRelative("triggerPhase"),
                        new GUIContent("Trigger Phase"));

                    // Tick rate only for OnTick
                    var phaseProp = actProp.FindPropertyRelative("triggerPhase");
                    if (phaseProp != null
                        && (TriggerPhase)phaseProp.enumValueIndex == TriggerPhase.OnTick)
                    {
                        EditorGUILayout.PropertyField(
                            actProp.FindPropertyRelative("tickRate"),
                            new GUIContent("Tick Rate"));
                    }

                    // Draw concrete action data
                    if (typeProp != null && typeProp.managedReferenceValue != null)
                    {
                        EditorGUILayout.PropertyField(
                            typeProp,
                            new GUIContent("Action Data"),
                            includeChildren: true);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox(
                            "Action data missing or improperly configured. Try adding the action again.",
                            MessageType.Error);
                    }

                    // Remove action
                    if (GUILayout.Button("Remove Action", GUILayout.MaxWidth(120)))
                    {
                        Undo.RecordObject(target, "Remove Weapon Action");
                        actionsProp.DeleteArrayElementAtIndex(ai);
                        serializedObject.ApplyModifiedProperties();
                        EditorGUI.indentLevel--;
                        break;
                    }

                    EditorGUI.indentLevel--;
                }
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }
    }

// Helper for displaying managed-reference type names
    static string GetNiceManagedTypeName(SerializedProperty prop)
    {
        if (prop == null)
            return "None";

        var full = prop.managedReferenceFullTypename;
        if (string.IsNullOrEmpty(full))
            return "None";

        // "AssemblyName Namespace.TypeName"
        int spaceIdx = full.IndexOf(' ');
        if (spaceIdx < 0 || spaceIdx + 1 >= full.Length)
            return full;

        var afterSpace = full.Substring(spaceIdx + 1);
        int lastDot = afterSpace.LastIndexOf('.');
        return lastDot >= 0
            ? afterSpace.Substring(lastDot + 1)
            : afterSpace;
    }

    void ShowAddActionMenu(int bindingIndex)
    {
        var menu = new GenericMenu();
        foreach (var t in _actionTypes)
        {
            string niceName = ObjectNames.NicifyVariableName(t.Name);
            menu.AddItem(new GUIContent(niceName), false, () =>
            {
                serializedObject.Update();

                var bindingProp = pBindings.GetArrayElementAtIndex(bindingIndex);
                var actionsProp = bindingProp.FindPropertyRelative("bindings");

                Undo.RecordObject(target, "Add Weapon Action");

                // Increase array size and fetch the new element
                int newIndex = actionsProp.arraySize;
                actionsProp.arraySize++;
                serializedObject.ApplyModifiedProperties();

                // Explicitly instantiate the managed reference correctly
                serializedObject.Update();

                bindingProp = pBindings.GetArrayElementAtIndex(bindingIndex);
                actionsProp = bindingProp.FindPropertyRelative("bindings");
                var newActionProp = actionsProp.GetArrayElementAtIndex(newIndex);

                var actionProp = newActionProp.FindPropertyRelative("action");
                actionProp.managedReferenceValue = Activator.CreateInstance(t);

                var triggerPhaseProp = newActionProp.FindPropertyRelative("triggerPhase");
                triggerPhaseProp.enumValueIndex = (int)TriggerPhase.OnPerform;

                serializedObject.ApplyModifiedProperties();
                serializedObject.Update(); // Final update ensures proper sync
            });
        }

        menu.ShowAsContext();
    }
}

